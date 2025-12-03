using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BubbleBlockSpawner : MonoBehaviour
{
    public static BubbleBlockSpawner Instance { get; private set; }

    [Header("Bubble Prefabs by Type")]
    public GameObject bubbleShield;
    public GameObject bubbleSword;
    public GameObject bubbleBow;
    public GameObject bubbleMagic;
    public GameObject bubbleHeal;

    [Header("Bubble Target Icons (Shield → Sword → Bow → Magic → Heal)")]
    public Transform[] blockIcons;

    [Header("Icon Counter Texts (TMP)")]
    public TextMeshProUGUI[] blockIconCounters;

    [Header("Mini +1 Text Prefab")]
    public TextMeshProUGUI miniTextPrefab;
    public TextMeshProUGUI minusTextPrefab;



    [Header("Destroy Flight Settings")]
    public float flyDuration = 0.8f;
    public float curveIntensity = 1.5f;
    public float hitPunch = 0.25f;

    private const float bubbleBaseScale = 0.2f;

    private Dictionary<BlockType, GameObject> bubbleDict;
    private List<GameObject> activeBubbles = new();

    // ★ 누적된 버블 수 (Sword 3개, Magic 4개 등)
    private Dictionary<BlockType, int> bubbleCountStack = new();

    private void Awake()
    {
        Instance = this;

        bubbleDict = new Dictionary<BlockType, GameObject>
        {
            { BlockType.Shield, bubbleShield },
            { BlockType.Sword,  bubbleSword },
            { BlockType.Bow,    bubbleBow },
            { BlockType.Magic,  bubbleMagic },
            { BlockType.Heal,   bubbleHeal }
        };

        // 누적 스택 초기화
        foreach (BlockType t in System.Enum.GetValues(typeof(BlockType)))
            bubbleCountStack[t] = 0;

        // 아이콘 카운트 텍스트 초기화
        foreach (var txt in blockIconCounters)
            txt.text = "0";
    }

    // ============================================================
    // SPAWN BUBBLE
    // ============================================================

    public void SpawnBubble(BlockType type, int count)
    {
        if (!bubbleDict.ContainsKey(type))
            return;

        GameObject prefab = bubbleDict[type];
        GameObject bubble = Instantiate(prefab, transform.position, Quaternion.identity);

        bubble.transform.localScale = Vector3.one * bubbleBaseScale;
        bubble.tag = "BubbleBlock";

        BubbleBlock bb = bubble.GetComponent<BubbleBlock>();
        if (bb == null) bb = bubble.AddComponent<BubbleBlock>();
 
        bb.type = type;
        int value = 1;
        if (count == 4) value = 2;
        else if (count == 5) value = 3;
        else if (count == 6) value = 4;
        else if (count >= 7) value = 5;

        bb.value = value;

        SpriteRenderer sr = bubble.GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            Color color = Color.white;
         
            if (count == 4) color = new Color(1f, 0.27f, 0.27f);
            else if (count == 5) color = new Color(0.27f, 0.6f, 1f);
            else if (count == 6) color = new Color(0.67f, 0.27f, 1f);
            else if (count >= 7) color = Color.black;

            sr.color = color;
            sr.sortingLayerName = "Foreground";
            sr.sortingOrder = 50;
        }

        Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
        Collider2D col = bubble.GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;
        }
        if (col != null) col.enabled = false;

        float xRange = Random.Range(-1.2f, 1.2f);
        float yRange = Random.Range(1.5f, 3.5f);
        Vector3 target = transform.position + new Vector3(xRange, yRange, 0f);

        float jumpHeight = Random.Range(1.0f, 1.8f);
        float duration = Random.Range(0.8f, 1.2f);

        Sequence seq = DOTween.Sequence();
        seq.Join(bubble.transform.DOJump(target, jumpHeight, 1, duration).SetEase(Ease.OutQuad));
        seq.Join(bubble.transform.DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)), duration, RotateMode.FastBeyond360));

        seq.OnComplete(() =>
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = Random.Range(0.8f, 1.3f);
                rb.AddTorque(Random.Range(-2f, 2f), ForceMode2D.Impulse);
            }
            if (col != null) col.enabled = true;
        });

        seq.SetLink(bubble, LinkBehaviour.KillOnDestroy);

        activeBubbles.Add(bubble);
    }

    // ============================================================
    // DESTROY → ICON 방향 이동 + 미니 텍스트 + 누적 카운트
    // ============================================================

    public void DestroyAllBubbles()
    {
        StartCoroutine(DestroyBubblesRoutine());
    }

    private IEnumerator DestroyBubblesRoutine()
    {
        foreach (GameObject bubble in new List<GameObject>(activeBubbles))
        {
            if (bubble == null) continue;

            BubbleBlock bb = bubble.GetComponent<BubbleBlock>();
            SpriteRenderer sr = bubble.GetComponent<SpriteRenderer>();

            bubble.transform.DOKill(true);
            if (sr != null) sr.DOKill(true);

            Transform targetIcon = blockIcons[(int)bb.type];
            Vector3 startPos = bubble.transform.position;
            Vector3 endPos = targetIcon.position;

            // 중간 포인트로 곡선 궤적 생성
            Vector3 control = (startPos + endPos) / 2f + (Vector3.up * curveIntensity);

            float t = 0f;
            bubble.transform.localScale = Vector3.one * bubbleBaseScale;

            Tween flyTween = DOTween.To(() => t, v =>
            {
                if (bubble == null) return;
                t = v;

                bubble.transform.position =
                    Mathf.Pow(1 - t, 2) * startPos +
                    2 * (1 - t) * t * control +
                    Mathf.Pow(t, 2) * endPos;

            }, 1f, flyDuration).SetEase(Ease.InQuad);

            flyTween.SetLink(bubble, LinkBehaviour.KillOnDestroy);
            yield return flyTween.WaitForCompletion();

            int add = bb.value;

            // 누적 스택 증가
            bubbleCountStack[bb.type] += add;

            // 아이콘 카운터 업데이트
            blockIconCounters[(int)bb.type].text = bubbleCountStack[bb.type].ToString();

            // 미니 텍스트 가중치 표시
            SpawnMiniText("+" + add, endPos);

            // 충돌 Punch
            if (targetIcon != null)
                targetIcon.DOPunchScale(Vector3.one * hitPunch, 0.25f, 6, 0.4f);

            // -----------------------------------------------------
            // 4) 버블 파괴 연출
            // -----------------------------------------------------
            if (bubble != null)
            {
                if (sr != null)
                {
                    bubble.transform.DOScale(bubbleBaseScale * 0.1f, 0.25f);
                    sr.DOFade(0f, 0.25f);
                }

                yield return new WaitForSeconds(0.1f);
                Destroy(bubble);
            }

            activeBubbles.Remove(bubble);
        }

        activeBubbles.Clear();

        yield return new WaitForSeconds(1.2f);
        TurnNotifier.Instance.PlayPlayerAttack();

        foreach (BlockType t in bubbleCountStack.Keys.ToList())
            bubbleCountStack[t] = 0;

        foreach (var txt in blockIconCounters)
            txt.text = "0";
    }

    // ============================================================
    // 미니 텍스트 생성 (+1 연출)
    // ============================================================

    private void SpawnMiniText(string text, Vector3 worldPos)
    {
        if (miniTextPrefab == null) return;

        var mini = Instantiate(miniTextPrefab, worldPos, Quaternion.identity, transform);
        mini.text = text;
        mini.alpha = 1f;

        Sequence s = DOTween.Sequence();
        s.Join(mini.transform.DOMoveY(worldPos.y + 0.6f, 0.6f).SetEase(Ease.OutQuad));
        s.Join(mini.DOFade(0f, 0.6f));
        s.OnComplete(() => Destroy(mini.gameObject));
    }

    public void SpawnMinusText(BlockType type, int count)
    {
        Transform icon = blockIcons[(int)type];
        TextMeshProUGUI counter = blockIconCounters[(int)type];

        if (miniTextPrefab == null || icon == null || counter == null)
            return;

        Vector3 pos = icon.position;

        // ------ minus text 생성 ------
        var mini = Instantiate(minusTextPrefab, pos, Quaternion.identity, transform);
        mini.text = "-" + count.ToString();
        mini.alpha = 1f;

        Sequence seq = DOTween.Sequence();
        seq.Join(mini.transform.DOMoveY(pos.y - 0.6f, 1.8f).SetEase(Ease.OutQuad));
        seq.OnComplete(() => Destroy(mini.gameObject));

        // ------ counter 텍스트 0으로 애니메이션 ------
        float current = int.Parse(counter.text);
        DOTween.To(() => current, x =>
        {
            current = x;
            counter.text = Mathf.RoundToInt(current).ToString();
        }, 0f, 1.2f).SetEase(Ease.OutCubic);
    }

    public Dictionary<BlockType, int> GetBubbleCountStack()
    {
        return bubbleCountStack;
    }
}
