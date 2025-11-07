using System.Collections.Generic;
using UnityEngine;

public class MatchFinder
{
    private readonly Board board;
    private Tile[,] tiles => board.tiles;
    private int width => board.width;
    private int height => board.height;

    public MatchFinder(Board board)
    {
        this.board = board;
    }

    public Dictionary<BlockType, HashSet<Vector2Int>> FindMatches()
    {
        var dict = new Dictionary<BlockType, HashSet<Vector2Int>>();
        ScanHorizontal(dict);
        ScanVertical(dict);
        return dict;
    }

    private void ScanHorizontal(Dictionary<BlockType, HashSet<Vector2Int>> dict)
    {
        for (int y = 0; y < height; y++)
        {
            int count = 1;
            BlockType prevType = tiles[0, y].currentBlock.type;

            for (int x = 1; x < width; x++)
            {
                Block current = tiles[x, y].currentBlock;
                if (current == null) continue;

                if (current.type == prevType) count++;
                else
                {
                    if (count >= 3)
                        AddMatch(dict, prevType, x - count, y, count, true);

                    count = 1;
                    prevType = current.type;
                }

                if (x == width - 1 && count >= 3)
                    AddMatch(dict, prevType, x - count + 1, y, count, true);
            }
        }
    }

    private void ScanVertical(Dictionary<BlockType, HashSet<Vector2Int>> dict)
    {
        for (int x = 0; x < width; x++)
        {
            int count = 1;
            BlockType prevType = tiles[x, 0].currentBlock.type;

            for (int y = 1; y < height; y++)
            {
                Block current = tiles[x, y].currentBlock;
                if (current == null) continue;

                if (current.type == prevType) count++;
                else
                {
                    if (count >= 3)
                        AddMatch(dict, prevType, x, y - count, count, false);

                    count = 1;
                    prevType = current.type;
                }

                if (y == height - 1 && count >= 3)
                    AddMatch(dict, prevType, x, y - count + 1, count, false);
            }
        }
    }

    private void AddMatch(Dictionary<BlockType, HashSet<Vector2Int>> dict, BlockType type, int startX, int startY, int count, bool horiz)
    {
        if (!dict.ContainsKey(type))
            dict[type] = new HashSet<Vector2Int>();

        for (int i = 0; i < count; i++)
        {
            int x = horiz ? startX + i : startX;
            int y = horiz ? startY : startY + i;
            dict[type].Add(new Vector2Int(x, y));
        }
    }

    public bool WouldCauseMatch(int x, int y, BlockType type)
    {
        if (x >= 2 &&
            tiles[x - 1, y]?.currentBlock != null &&
            tiles[x - 2, y]?.currentBlock != null)
        {
            if (tiles[x - 1, y].currentBlock.type == type &&
                tiles[x - 2, y].currentBlock.type == type)
                return true;
        }

        if (y >= 2 &&
            tiles[x, y - 1]?.currentBlock != null &&
            tiles[x, y - 2]?.currentBlock != null)
        {
            if (tiles[x, y - 1].currentBlock.type == type &&
                tiles[x, y - 2].currentBlock.type == type)
                return true;
        }

        return false;
    }
}
