using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [Header("Board Size")]
    public int width = 8;
    public int height = 8;

    [Header("Prefabs")]
    public GameObject tilePrefab;

    [Header("References")]
    public BlockPool blockPool;

    public Tile[,] tiles;

    [Header("Swap Settings")]
    public bool isSwapping = false;

    /*보드 생성 관련 코드, 초기 블록 및 타일 생성*/
    public void GenerateBoard()
    {
        if (tilePrefab == null || blockPool == null)
        {
            Debug.LogError("Board 초기화 실패: Prefab이나 BlockPool 누락");
            return;
        }

        tiles = new Tile[width, height];

        SpriteRenderer tileRenderer = tilePrefab.GetComponent<SpriteRenderer>();
        if (tileRenderer == null)
        {
            Debug.LogError("Tile Prefab에 SpriteRenderer가 없습니다.");
            return;
        }

        Vector2 tileSize = tileRenderer.bounds.size;
        float spacingX = tileSize.x * 0.90f; 
        float spacingY = tileSize.y * 0.85f; 

        Vector3 startOffset = new Vector3(-((width - 1) * spacingX) / 2f + transform.position.x, -((height - 1) * spacingY) / 2f + transform.position.y, 0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x * spacingX, y * spacingY, 0) + startOffset;
                GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.Init(new Vector2Int(x, y));
                tiles[x, y] = tile;

                BlockType randType = (BlockType)Random.Range(0, System.Enum.GetValues(typeof(BlockType)).Length);
                GameObject blockObj = blockPool.GetNodeFromPool(randType, pos);
                Block block = blockObj.GetComponent<Block>();
                block.Init(randType, new Vector2Int(x, y));

                blockObj.transform.position = tileObj.transform.position;
                tile.currentBlock = block;
            }
        }

        Debug.Log($"보드 생성 완료: {width}x{height}");
    }

    public List<Tile> GetAdjacentTiles(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        int[,] offsets = new int[,] { { 0, 1 }, { 0, -1 }, { -1, 0 }, { 1, 0 } };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int nx = tile.gridPos.x + offsets[i, 0];
            int ny = tile.gridPos.y + offsets[i, 1];

            if (nx >= 0 && nx < width && ny >= 0 && ny < height)
            {
                if (tiles[nx, ny] != null)
                    neighbors.Add(tiles[nx, ny]);
            }
        }
        return neighbors;
    }

    /*보드 내 스왑 관련 로직 정의*/
    public void Swap(Tile tileA, Tile tileB)
    {
        if (tileA == null || tileB == null) return;
        if (isSwapping) return;

        if (Mathf.Abs(tileA.gridPos.x - tileB.gridPos.x) + Mathf.Abs(tileA.gridPos.y - tileB.gridPos.y) != 1)
        {
            Debug.LogWarning("인접하지 않은 타일 스왑 시도입니다");
            return;
        }
        StartCoroutine(SwapRoutine(tileA, tileB));
    }

    private IEnumerator SwapRoutine(Tile tileA, Tile tileB)
    {
        isSwapping = true;

        Block blockA = tileA.currentBlock;
        Block blockB = tileB.currentBlock;

        tileA.currentBlock = blockB;
        tileB.currentBlock = blockA;

        Vector2Int tempPos = blockA.gridPos;
        blockA.gridPos = blockB.gridPos;
        blockB.gridPos = tempPos;

        Vector3 posA = tileA.transform.position;
        Vector3 posB = tileB.transform.position;

        float moveTime = 0.15f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveTime;
            blockA.transform.position = Vector3.Lerp(posB, posA, t);
            blockB.transform.position = Vector3.Lerp(posA, posB, t);
            yield return null;
        }

        blockA.transform.position = posA;
        blockB.transform.position = posB;

        isSwapping = false;

        yield return StartCoroutine(CheckMatch(tileA, tileB));
    }

    private IEnumerator CheckMatch(Tile tileA, Tile tileB)
    {
        bool matchFound = true;

        if (!matchFound)
        {
            Debug.Log("매치 실패! 스왑 되돌림");
            yield return SwapBack(tileA, tileB);
        }
        else
        {
            Debug.Log("매치 성공!");
        }
    }

    private IEnumerator SwapBack(Tile tileA, Tile tileB)
    {
        yield return SwapRoutine(tileA, tileB);
    }
}
