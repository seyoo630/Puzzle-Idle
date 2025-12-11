using DG.Tweening;
using UnityEngine;
using TMPro;

public class TurnNotifier : MonoBehaviour
{
    public static TurnNotifier Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI turnText;

    [Header("Main Motion")]
    public float enterTime = 0.38f;
    public float centerSlowTime = 1f;
    public float exitTime = 0.35f;

    [Header("Ghost Trail (ReadyFight Style)")]
    public int ghostCount = 6;
    public float ghostRadiusMin = 120f;
    public float ghostRadiusMax = 220f;
    public float ghostStartAlpha = 0.7f;
    public float ghostScaleMin = 1.05f;
    public float ghostScaleMax = 1.25f;

    private void Awake()
    {
        Instance = this;
    }

    // 플레이어 턴 시작
    public void PlayPlayerTurn()
    {
        GameManager.Instance.UnlockInput();
        Color playerColor;
        if (!ColorUtility.TryParseHtmlString("#FFAEA5", out playerColor))
            playerColor = Color.white;

        turnText.text = "PLAYER TURN";
        PlayTurnAnimation(playerColor, () =>
        {
            UIManager.Instance.moveCount = 3;
            UIManager.Instance.UpdateMoves(UIManager.Instance.moveCount);
        });
    }

    public void PlayPlayerAttack()
    {
        GameManager.Instance.LockInput();
        UIManager.Instance.moveText.text = "PLAYER ATTACK";
        if (PlayerAttackManager.Instance != null)
            PlayerAttackManager.Instance.StartPlayerAttack();
        else
            PlayEnemyTurn();

    }

    // 적 턴 시작
    public void PlayEnemyTurn()
    {
        GameManager.Instance.LockInput();
        turnText.text = "ENEMY TURN";
        PlayTurnAnimation(new Color(1f, 0.4f, 0.4f), () =>
        {
            Debug.Log("적 턴 행동 수행 중...");
            // 실제 적 로직 수행
            DOVirtual.DelayedCall(2f, () =>
            {
                // 턴 종료 후 다시 플레이어 턴 복귀
                UIManager.Instance.ResetPlayerMoves();
            });
        });
    }

    // ------------------------ 애니메이션 ------------------------

    private void PlayTurnAnimation(Color textColor, TweenCallback onComplete = null)
    {
        var rt = turnText.rectTransform;
        turnText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        turnText.transform.localScale = Vector3.one * 1.2f;
        rt.localPosition = new Vector3(600f, 0f, 0f); // 오른쪽 밖에서 시작

        Sequence seq = DOTween.Sequence();

        // 중앙 진입
        seq.Append(rt.DOLocalMoveX(0f, enterTime).SetEase(Ease.OutCubic));
        seq.Join(turnText.DOFade(1f, enterTime * 0.7f));

        // 중앙 고스팅 효과
        seq.AppendCallback(() => SpawnGhostTrail(textColor));
        seq.Append(turnText.transform.DOScale(Vector3.one * 1.4f, centerSlowTime * 0.5f).SetEase(Ease.InOutSine));
        seq.Append(turnText.transform.DOScale(Vector3.one, centerSlowTime * 0.5f).SetEase(Ease.InOutSine));

        // 왼쪽으로 퇴장
        seq.Append(rt.DOLocalMoveX(-600f, exitTime).SetEase(Ease.InCubic));
        seq.Join(turnText.DOFade(0f, exitTime * 0.8f));

        seq.OnComplete(() =>
        {
            rt.localPosition = new Vector3(600f, 0f, 0f);
            turnText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
            onComplete?.Invoke();
        });
    }

    private void SpawnGhostTrail(Color baseColor)
    {
        var parent = turnText.rectTransform.parent;

        for (int i = 0; i < ghostCount; i++)
        {
            TextMeshProUGUI ghost = Instantiate(turnText, parent);
            ghost.name = $"TurnGhost_{i}_{Time.frameCount}";
            ghost.text = turnText.text;
            ghost.color = new Color(baseColor.r, baseColor.g, baseColor.b, ghostStartAlpha);

            var grt = ghost.rectTransform;
            grt.localPosition = turnText.rectTransform.localPosition;
            grt.localScale = Vector3.one;
            grt.localRotation = Quaternion.identity;

            float baseAngle = (360f / Mathf.Max(1, ghostCount)) * i;
            float angle = baseAngle + Random.Range(-12f, 12f);
            float rad = angle * Mathf.Deg2Rad;

            float dist = Random.Range(ghostRadiusMin, ghostRadiusMax);
            Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
            Vector3 target = (Vector3)(dir * dist);
            float targetScale = Random.Range(ghostScaleMin, ghostScaleMax);

            Sequence gs = DOTween.Sequence();
            gs.Join(ghost.DOFade(ghostStartAlpha, 0.06f))
              .Join(grt.DOScale(targetScale, 0.09f).SetEase(Ease.OutQuad))
              .Append(grt.DOLocalMove(target, 0.18f).SetEase(Ease.OutCubic))
              .Join(ghost.DOFade(0f, 0.22f).SetEase(Ease.InQuad))
              .OnComplete(() => Destroy(ghost.gameObject));
        }
    }
}
