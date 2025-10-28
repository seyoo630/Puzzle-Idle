using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour
{
    public BlockType type;          
    public Vector2Int gridPos;     
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(BlockType type, Vector2Int pos)
    {
        this.type = type;
        this.gridPos = pos;
        name = $"Block_{type}_{pos.x}_{pos.y}";
    }
    public void SetTransparency(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }

    public void MoveTo(Vector3 targetPos, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(MoveRoutine(targetPos, duration));
    }

    private IEnumerator MoveRoutine(Vector3 targetPos, float duration)
    {
        Vector3 startPos = transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }
        transform.position = targetPos;
    }

}