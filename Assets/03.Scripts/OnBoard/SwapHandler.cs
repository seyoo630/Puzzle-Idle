using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class SwapHandler
{
    private readonly Board board;
    private Tile[,] tiles => board.tiles;

    public SwapHandler(Board board)
    {
        this.board = board;
    }

    public void SwapBlocks(Tile tileA, Tile tileB)
    {
        if (tileA == null || tileB == null || tileA == tileB) return;

        Block blockA = tileA.currentBlock;
        Block blockB = tileB.currentBlock;

        if (blockA == null || blockB == null)
        {
            Debug.LogWarning("SwapBlocks 실패: 블록이 비어있습니다.");
            return;
        }

        tileA.currentBlock = blockB;
        tileB.currentBlock = blockA;

        Vector2Int tempGrid = blockA.gridPos;
        blockA.gridPos = blockB.gridPos;
        blockB.gridPos = tempGrid;

        Tile tempTile = blockA.currentTile;
        blockA.currentTile = blockB.currentTile;
        blockB.currentTile = tempTile;

        blockA.transform.position = blockA.currentTile.transform.position;
        blockB.transform.position = blockB.currentTile.transform.position;

        BoardUtils.SnapTiles(new List<Tile> { tileA, tileB });

        blockA.SetTransparency(1f);
        blockB.SetTransparency(1f);

        bool hasMatch = board.CheckMatchWithResult();

        if (!hasMatch)
        {
            board.StartCoroutine(RevertSwap(blockA, blockB, tileA, tileB));
        }

    }

    public IEnumerator PlayMatch(List<Block> matchedBlocks)
    {
        foreach (Block block in matchedBlocks)
        {
            block.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);
            block.SetTransparency(0.3f);
        }

        yield return new WaitForSeconds(0.25f);

        foreach (Block block in matchedBlocks)
        {
            Tile tile = block.currentTile;
            board.blockPool.ReturnToPool(block.gameObject, block.type);
            tile.currentBlock = null;
        }
        BoardUtils.SnapAll(board);
    }

    private IEnumerator RevertSwap(Block blockA, Block blockB, Tile tileA, Tile tileB)
    {
        float shakeTime = 0.2f;
        float moveTime = 0.25f;

        // 진동 연출
        blockA.transform.DOShakePosition(shakeTime, 0.1f, 10, 90, false, true);
        blockB.transform.DOShakePosition(shakeTime, 0.1f, 10, 90, false, true);

        yield return new WaitForSeconds(shakeTime);

        // 위치 복귀
        blockA.transform.DOMove(tileA.transform.position, moveTime).SetEase(Ease.OutBack);
        blockB.transform.DOMove(tileB.transform.position, moveTime).SetEase(Ease.OutBack);

        // 참조 복귀
        yield return new WaitForSeconds(moveTime);

        tileA.currentBlock = blockA;
        tileB.currentBlock = blockB;

        blockA.currentTile = tileA;
        blockB.currentTile = tileB;

        (blockA.gridPos, blockB.gridPos) = (tileA.gridPos, tileB.gridPos);

        BoardUtils.SnapTiles(new List<Tile> { tileA, tileB });
        board.isProcessing = false;
        GameManager.Instance.UnlockInput();
    }

}
