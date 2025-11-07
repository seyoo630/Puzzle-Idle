using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class BubbleBlockSpawner : MonoBehaviour
{
    [Header("Bubble Prefabs by Type")]
    public GameObject bubbleShield;
    public GameObject bubbleSword;
    public GameObject bubbleBow;
    public GameObject bubbleMagic;
    public GameObject bubbleHeal;

    private Dictionary<BlockType, GameObject> bubbleDict;

    private void Awake()
    {
        bubbleDict = new Dictionary<BlockType, GameObject>
        {
            { BlockType.Shield, bubbleShield },
            { BlockType.Sword,  bubbleSword },
            { BlockType.Bow,    bubbleBow },
            { BlockType.Magic,  bubbleMagic },
            { BlockType.Heal,   bubbleHeal }
        };
    }

    public void SpawnBubble(BlockType type, int count)
    {
        if (!bubbleDict.ContainsKey(type))
        {
            Debug.LogWarning($"[BubbleSpawner] Bubble prefab not assigned for {type}");
            return;
        }

        GameObject prefab = bubbleDict[type];
        GameObject bubble = Instantiate(prefab, transform.position, Quaternion.identity);
        bubble.transform.localScale = Vector3.one * 0.2f;

        // ������������������������������ ���� ���� ������������������������������
        Color color = Color.white;
        if (count == 4) color = new Color(1f, 0.27f, 0.27f);       // ����
        else if (count == 5) color = new Color(0.27f, 0.6f, 1f);   // �Ķ�
        else if (count == 6) color = new Color(0.67f, 0.27f, 1f);  // ����
        else if (count >= 7) color = Color.black;                  // ����

        SpriteRenderer sr = bubble.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = color;
            sr.sortingLayerName = "Foreground";  // ���� ����
            sr.sortingOrder = 50;
        }

        // ������������������������������ ���� ���� ������������������������������
        Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
        Collider2D col = bubble.GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic; // Tween �� ���� OFF
        }

        if (col != null)
            col.enabled = false; // ���� �� �浹 ��Ȱ��ȭ

        // ������������������������������ �̵� ���� ���� ������������������������������
        // X�� �̵��� ���� (�¿�� �ణ��), Y���� �����Ӱ�
        float xRange = Random.Range(-1.2f, 1.2f);
        float yRange = Random.Range(1.5f, 3.5f);
        Vector3 target = transform.position + new Vector3(xRange, yRange, 0f);

        float jumpHeight = Random.Range(1.0f, 1.8f);
        float duration = Random.Range(0.8f, 1.2f);

        // ������������������������������ ���� ���� ������������������������������
        Sequence seq = DOTween.Sequence();

        seq.Join(bubble.transform
            .DOJump(target, jumpHeight, 1, duration)
            .SetEase(Ease.OutQuad));

        seq.Join(bubble.transform
            .DORotate(new Vector3(0, 0, Random.Range(-360f, 360f)),
            duration, RotateMode.FastBeyond360));

        // ������������������������������ ���� ���� ������������������������������
        seq.OnComplete(() =>
        {
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic; // �߷� ����
                rb.gravityScale = Random.Range(0.8f, 1.3f);
                rb.AddTorque(Random.Range(-2f, 2f), ForceMode2D.Impulse);
            }

            if (col != null)
                col.enabled = true; // �浹 �ٽ� �ѱ�
        });

        seq.Play();
    }

}
