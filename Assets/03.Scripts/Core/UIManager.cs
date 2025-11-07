using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Sliders")]
    public Slider playerHPSlider;
    public Slider enemyHPSlider;
    public Slider progressSlider;

    [Header("Progress Skulls")]
    public Image[] progressSkulls;

    [Header("Move Text")]
    public TextMeshProUGUI moveText;
    public int moveCount = 3;

    private float playerTotalHP;
    private float enemyTotalHP;

    private void Awake()
    {
        Instance = this;
    }

    public void InitUI()
    {
        playerTotalHP = CalculateTotalHP<Archer, Warrior, Tanker, Magician, Healer>();
        enemyTotalHP = CalculateTotalHP<Enemy>();

        playerHPSlider.value = 0f;
        enemyHPSlider.value = 0f;
        progressSlider.value = 0f;
        moveText.alpha = 0f;
        moveText.text = "3 MOVES";

        DOTween.To(() => playerHPSlider.value, x => playerHPSlider.value = x, 1f, 1.0f).SetEase(Ease.OutCubic);
        DOTween.To(() => enemyHPSlider.value, x => enemyHPSlider.value = x, 1f, 1.0f).SetEase(Ease.OutCubic).SetDelay(0.2f);
        DOTween.To(() => progressSlider.value, x => progressSlider.value = x, 0.33f, 1.0f).SetEase(Ease.OutCubic).SetDelay(0.4f);
        moveText.DOFade(1f, 0.5f).SetDelay(0.4f);

        AnimateProgressSkulls();
    }

    private float CalculateTotalHP<T>() where T : Character
    {
        T[] characters = Object.FindObjectsByType<T>(FindObjectsSortMode.None);
        float total = 0f;
        foreach (var c in characters)
            total += c.maxHP;
        return total;
    }

    private float CalculateTotalHP<T1, T2, T3, T4, T5>()
        where T1 : Character where T2 : Character where T3 : Character where T4 : Character where T5 : Character
    {
        float sum = 0f;
        sum += Object.FindObjectsByType<T1>(FindObjectsSortMode.None).Sum(c => c.maxHP);
        sum += Object.FindObjectsByType<T2>(FindObjectsSortMode.None).Sum(c => c.maxHP);
        sum += Object.FindObjectsByType<T3>(FindObjectsSortMode.None).Sum(c => c.maxHP);
        sum += Object.FindObjectsByType<T4>(FindObjectsSortMode.None).Sum(c => c.maxHP);
        sum += Object.FindObjectsByType<T5>(FindObjectsSortMode.None).Sum(c => c.maxHP);
        return sum;
    }

    private void AnimateProgressSkulls()
    {
        if (progressSkulls == null || progressSkulls.Length == 0)
            return;

        foreach (var skull in progressSkulls)
        {
            if (skull == null) continue;
            skull.rectTransform
                .DOLocalMoveX(skull.rectTransform.localPosition.x + Random.Range(-5f, 5f), 0.6f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetDelay(Random.Range(0f, 0.2f));
        }
    }

    // ------------------------ 턴 관련 로직 ------------------------

    public void UpdateMoves(int moves)
    {
        moveText.text = $"{moves} MOVES";
        moveText.transform.DOPunchScale(Vector3.one * 0.15f, 0.25f, 6, 0.6f);

        // 행동이 0이 되면 자동으로 적 턴으로 전환
        if (moves <= 0)
        {
            moveText.text = "ENEMY MOVES";
            TurnNotifier.Instance.PlayEnemyTurn();
        }
    }

    public void ResetPlayerMoves()
    {
        moveCount = 3;
        moveText.text = $"{moveCount} MOVES";
        TurnNotifier.Instance.PlayPlayerTurn();
    }

    // ------------------------ 진행도 & HP ------------------------

    public void UpdateProgress(float target)
    {
        DOTween.To(() => progressSlider.value, x => progressSlider.value = x, target, 0.4f).SetEase(Ease.OutCubic);
    }

    public void SetPlayerHP(float normalized)
    {
        DOTween.To(() => playerHPSlider.value, x => playerHPSlider.value = x, Mathf.Clamp01(normalized), 0.3f).SetEase(Ease.OutCubic);
    }

    public void SetEnemyHP(float normalized)
    {
        DOTween.To(() => enemyHPSlider.value, x => enemyHPSlider.value = x, Mathf.Clamp01(normalized), 0.3f).SetEase(Ease.OutCubic);
    }
}
