using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class Block : MonoBehaviour
{
    public BlockType type;
    public Vector2Int gridPos;
    [HideInInspector] public Tile currentTile;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalWorldPos;
    private Vector2Int originalGridPos;
    private Vector3 dragStartPos;

    private Camera mainCam;
    private List<Tile> adjacentTiles = new List<Tile>();
    private Tile closestTile;
    private Block previewBlock;
    private Block prevPreviewBlock;

    [Header("Block Settings")]
    public bool isDragging = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCam = Camera.main;
    }

    public void Init(BlockType type, Vector2Int pos, Tile tile = null)
    {
        this.type = type;
        this.gridPos = pos;
        this.currentTile = tile;
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

    private void OnMouseDown()
    {
        if (GameManager.Instance.board.isProcessing)
            return;

        if (currentTile == null)
        {
            Debug.LogWarning($"{name} has no currentTile assigned!");
            return;
        }

        GameManager.Instance.board.isProcessing = true;
        isDragging = true;

        originalWorldPos = transform.position;
        originalGridPos = gridPos;

        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        dragStartPos = mouseWorld;

        adjacentTiles = GameManager.Instance.board.GetAdjacentTiles(currentTile);

        closestTile = currentTile;
        previewBlock = null;
        prevPreviewBlock = null;

        SetTransparency(0.7f);
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;
        Vector3 mouseWorld = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 dragVector = mouseWorld - dragStartPos;
        Vector2 dragDir = new Vector2(dragVector.x, dragVector.y).normalized;
        float dragDistance = dragVector.magnitude;

        if (adjacentTiles == null || adjacentTiles.Count == 0)
            return;

        // 짧은 드래그면 제자리 유지
        if (dragDistance < 0.3f)
        {
            if (closestTile != currentTile)
            {
                ResetPreview();
                closestTile = currentTile;
                previewBlock = null;
            }

            transform.position = originalWorldPos;
            SetTransparency(1f);
            return;
        }

        // 방향 기준으로 가장 가까운 타일 탐색
        float maxDot = -1f;
        Tile newClosest = null;

        foreach (Tile t in adjacentTiles)
        {
            Vector2 toNeighbor = (t.gridPos - currentTile.gridPos);
            float dot = Vector2.Dot(dragDir, toNeighbor.normalized);

            if (dot > maxDot)
            {
                maxDot = dot;
                newClosest = t;
            }
        }

        // closest 변경 시 프리뷰 갱신
        if (newClosest != null && newClosest != closestTile)
        {
            ResetPreview();

            closestTile = newClosest;
            previewBlock = closestTile.currentBlock;

            if (previewBlock != null)
            {
                previewBlock.transform.position = originalWorldPos;
                previewBlock.SetTransparency(0.7f);
                prevPreviewBlock = previewBlock;
            }
        }

        // 내 블록을 인접 타일 자리로 이동
        if (closestTile != null)
        {
            transform.position = closestTile.transform.position;
            SetTransparency(0.7f);
        }
    }

    private void OnMouseUp()
    {
        if (!isDragging) return;
        StartCoroutine(PlaySwap());
    }

    private IEnumerator PlaySwap()
    {
        ResetPreview();
        SetTransparency(1f);

        isDragging = false;

        if (closestTile == null || closestTile == currentTile)
        {
            transform.DOMove(originalWorldPos, 0.15f).SetEase(Ease.OutQuad);
            GameManager.Instance.board.isProcessing = false;
            yield break;
        }

        Block targetBlock = closestTile.currentBlock;
        if (targetBlock == null)
        {
            Debug.LogWarning("Swap 대상 블록이 존재하지 않습니다.");
            transform.DOMove(originalWorldPos, 0.15f).SetEase(Ease.OutQuad);
            GameManager.Instance.board.isProcessing = false;
            yield break;
        }

        float dropTime = 0.18f;
        float scaleUp = 1.15f;
        float scaleDown = 0.8f;
        Ease dropEase = Ease.OutBack;

        Vector3 myTargetPos = closestTile.transform.position;
        Vector3 otherTargetPos = originalWorldPos;

        Vector3 startPosOffset = (originalWorldPos - myTargetPos) * 0.2f;
        transform.position = myTargetPos + startPosOffset;

        Vector3 targetStartOffset = (otherTargetPos - targetBlock.transform.position) * 0.2f;
        targetBlock.transform.position = otherTargetPos + targetStartOffset;

        transform.localScale = Vector3.one;
        targetBlock.transform.localScale = Vector3.one;

        // DOTween 시퀀스 생성 (AutoKill 비활성화)
        Sequence mySeq = DOTween.Sequence().SetAutoKill(false);
        mySeq.Append(transform.DOScale(Vector3.one * scaleUp, 0.08f).SetEase(Ease.OutQuad));
        mySeq.Join(transform.DOMove(myTargetPos, dropTime).SetEase(dropEase));
        mySeq.Append(transform.DOScale(Vector3.one * scaleDown, 0.07f).SetEase(Ease.InQuad));

        Sequence otherSeq = DOTween.Sequence().SetAutoKill(false);
        otherSeq.Append(targetBlock.transform.DOScale(Vector3.one * scaleUp, 0.08f).SetEase(Ease.OutQuad));
        otherSeq.Join(targetBlock.transform.DOMove(otherTargetPos, dropTime).SetEase(dropEase));
        otherSeq.Append(targetBlock.transform.DOScale(Vector3.one * scaleDown, 0.07f).SetEase(Ease.InQuad));

        int completedCount = 0;
        mySeq.OnComplete(() => completedCount++);
        otherSeq.OnComplete(() => completedCount++);

        yield return new WaitUntil(() => completedCount >= 2);

        GameManager.Instance.board.SwapBlocks(currentTile, closestTile);

        transform.localScale = Vector3.one * scaleDown;
        targetBlock.transform.localScale = Vector3.one * scaleDown;

        // 트윈 정리 (메모리 관리)
        mySeq.Kill();
        otherSeq.Kill();
    }



    private void ResetPreview()
    {
        if (prevPreviewBlock != null && prevPreviewBlock.currentTile != null)
        {
            prevPreviewBlock.transform.position = prevPreviewBlock.currentTile.transform.position;
            prevPreviewBlock.SetTransparency(1f);
            prevPreviewBlock = null;
        }
    }
}
