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

    private Tile[,] tiles;

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
}
