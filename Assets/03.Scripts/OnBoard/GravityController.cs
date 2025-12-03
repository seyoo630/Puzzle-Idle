using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class GravityController
{
    private readonly Board board;
    private Tile[,] tiles => board.tiles;
    private int width => board.width;
    private int height => board.height;
    private BlockPool pool => board.blockPool;

    public GravityController(Board board)
    {
        this.board = board;
    }

    public IEnumerator ApplyGravity()
    {
        List<Tile> dirty = new();
        List<Tweener> tweens = new();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (tiles[x, y].currentBlock == null)
                {
                    for (int ny = y + 1; ny < height; ny++)
                    {
                        Block upper = tiles[x, ny].currentBlock;
                        if (upper != null)
                        {
                            tiles[x, ny].currentBlock = null;
                            tiles[x, y].currentBlock = upper;

                            upper.currentTile = tiles[x, y];
                            upper.gridPos = tiles[x, y].gridPos;
                            dirty.Add(tiles[x, y]);

                            var t = upper.transform.DOMove(tiles[x, y].transform.position, 0.25f)
                                .SetEase(Ease.OutQuad);
                            tweens.Add(t);
                            break;
                        }
                    }
                }
            }
        }

        yield return new WaitUntil(() => !tweens.Exists(t => t.IsActive() && t.IsPlaying()));
        BoardUtils.SnapTiles(dirty);
        yield return FillEmptyTiles();
    }

    private IEnumerator FillEmptyTiles()
    {
        float spawnOffsetY = 1.0f;
        float dropTime = 0.3f;

        for (int x = 0; x < width; x++)
        {
            for (int y = height - 1; y >= 0; y--)
            {
                if (tiles[x, y].currentBlock == null)
                {
                    Vector3 spawnPos = tiles[x, y].transform.position + Vector3.up * spawnOffsetY;
                    BlockType randType = (BlockType)Random.Range(0, System.Enum.GetValues(typeof(BlockType)).Length);
                    GameObject newObj = pool.GetNodeFromPool(randType, spawnPos);
                    Block newBlock = newObj.GetComponent<Block>();
                    newBlock.Init(randType, new Vector2Int(x, y), tiles[x, y]);
                    tiles[x, y].currentBlock = newBlock;

                    newBlock.transform.DOJump(tiles[x, y].transform.position, 0.4f, 1, dropTime)
                        .SetEase(Ease.OutQuad);
                }
            }
        }

        yield return new WaitForSeconds(0.35f);
        BoardUtils.SnapAll(board);
        board.CheckBoardPlayable();
        board.isProcessing = false;
        board.CheckMatch();

    }
}
