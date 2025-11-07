using System.Collections.Generic;
using UnityEngine;

public static class BoardUtils
{
    public static void SnapTiles(List<Tile> tiles)
    {
        if (tiles == null || tiles.Count == 0) return;

        foreach (var tile in tiles)
        {
            if (tile?.currentBlock == null) continue;
            Block b = tile.currentBlock;
            b.currentTile = tile;
            b.gridPos = tile.gridPos;
            b.transform.position = tile.transform.position;
        }
    }

    public static void SnapAll(Board board)
    {
        for (int x = 0; x < board.width; x++)
        {
            for (int y = 0; y < board.height; y++)
            {
                var tile = board.tiles[x, y];
                if (tile?.currentBlock == null) continue;
                var b = tile.currentBlock;
                b.currentTile = tile;
                b.gridPos = tile.gridPos;
                b.transform.position = tile.transform.position;
            }
        }
    }
}
