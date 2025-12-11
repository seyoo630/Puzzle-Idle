using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    [SerializeField] private BubbleBlockSpawner bubbleSpawner;

    [Header("Board State")]
    public bool isProcessing = false;

    public Tile[,] tiles;

    // 하위 모듈
    private MatchFinder matchFinder;
    private GravityController gravity;
    private SwapHandler swapper;

    private int comboCount = 0;
    private bool comboActive = false;

    private void Awake()
    {
        matchFinder = new MatchFinder(this);
        gravity = new GravityController(this);
        swapper = new SwapHandler(this);
    }

    /* 초기 보드 생성 */
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

        Vector3 startOffset = new Vector3(
            -((width - 1) * spacingX) / 2f + transform.position.x,
            -((height - 1) * spacingY) / 2f + transform.position.y,
            0);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x * spacingX, y * spacingY, 0) + startOffset;
                GameObject tileObj = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                Tile tile = tileObj.GetComponent<Tile>();
                tile.Init(new Vector2Int(x, y));
                tiles[x, y] = tile;

                BlockType randType;
                int safety = 0;

                do
                {
                    randType = (BlockType)Random.Range(0, System.Enum.GetValues(typeof(BlockType)).Length);
                    safety++;
                    if (safety > 10) break;
                } while (matchFinder.WouldCauseMatch(x, y, randType));

                GameObject blockObj = blockPool.GetNodeFromPool(randType, pos);
                Block block = blockObj.GetComponent<Block>();
                block.Init(randType, new Vector2Int(x, y), tile);

                blockObj.transform.position = tileObj.transform.position;
                tile.currentBlock = block;
            }
        }
        CheckBoardPlayable();
    }

    /* 인접 타일 반환 */
    public List<Tile> GetAdjacentTiles(Tile tile)
    {
        List<Tile> neighbors = new();
        int[,] offsets = { { 0, 1 }, { 0, -1 }, { -1, 0 }, { 1, 0 } };

        for (int i = 0; i < 4; i++)
        {
            int nx = tile.gridPos.x + offsets[i, 0];
            int ny = tile.gridPos.y + offsets[i, 1];

            if (nx >= 0 && nx < width && ny >= 0 && ny < height && tiles[nx, ny] != null)
                neighbors.Add(tiles[nx, ny]);
        }
        return neighbors;
    }

    /* 블록 스왑 요청 */
    public void SwapBlocks(Tile tileA, Tile tileB)
        => swapper.SwapBlocks(tileA, tileB);

    /* 매치 판정 및 처리 */
    public void CheckMatch()
    {
        isProcessing = true;
        var matchDict = matchFinder.FindMatches();
        if (matchDict.Count == 0)
        {
            comboActive = false;
            comboCount = 0;
            isProcessing = false;
            GameManager.Instance.UnlockInput();
            return;
        }

        StringBuilder log = new();
        log.Append("매치 발견: ");
        foreach (var kvp in matchDict)
        {
            log.Append($"{kvp.Key} {kvp.Value.Count}개 / ");
        }
        Debug.Log(log.ToString());
        if (!comboActive)
        {
            comboActive = true;
            comboCount = 1;    // 첫 매치 = 콤보 1
        }
        else
        {
            comboCount++;      // 연쇄 매치 = 증가
        }

        if (comboCount >= 2)
            BoardNotifier.Instance.ShowCombo(comboCount);
        StartCoroutine(HandleMatches(matchDict));
    }


    private IEnumerator HandleMatches(Dictionary<BlockType, HashSet<Vector2Int>> matchDict)
    {
        List<Block> matchedBlocks = new();

        foreach (var kvp in matchDict)
        {
            foreach (var pos in kvp.Value)
            {
                if (tiles[pos.x, pos.y].currentBlock != null)
                    matchedBlocks.Add(tiles[pos.x, pos.y].currentBlock);
            }
        }

        yield return StartCoroutine(swapper.PlayMatch(matchedBlocks));

        foreach (var kvp in matchDict)
        {
            if (bubbleSpawner != null)
                bubbleSpawner.SpawnBubble(kvp.Key, kvp.Value.Count);
        }

        yield return StartCoroutine(gravity.ApplyGravity());


        if (matchFinder.FindMatches().Count == 0)
        {
            if (UIManager.Instance.moveCount <= 0)
            {
                // ★ 1. Moves를 다 씀 -> 턴 종료 프로세스 시작
                yield return new WaitForEndOfFrame();
                if (BubbleBlockSpawner.Instance != null)
                    BubbleBlockSpawner.Instance.DestroyAllBubbles();

                GameManager.Instance.board.isProcessing = false;


                TurnNotifier.Instance.PlayPlayerAttack();
            }
            else
            {
                GameManager.Instance.board.isProcessing = false;
                GameManager.Instance.UnlockInput();
            }
            yield break;
        }
    }

    public bool CheckMatchWithResult()
    {
        var matchDict = matchFinder.FindMatches();

        if (matchDict.Count == 0)
        {
            return false;
        }

        comboActive = true;
        comboCount = 1;

        var nextMoveCnt = --UIManager.Instance.moveCount;

        UIManager.Instance.UpdateMoves(nextMoveCnt);

        GameManager.Instance.LockInput();


        StartCoroutine(HandleMatches(matchDict));
        return true;
    }

    public void CheckBoardPlayable()
    {
        if (!HasAvailableMoves())
        {
            comboActive = false;
            comboCount = 0;
            Debug.Log("Dead board detected! Shuffle executed.");
            ShuffleBoard();
            BoardNotifier.Instance.ShowShuffle();   // ★ Shuffle 표시
        }
    }

    public bool HasAvailableMoves()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tile a = tiles[x, y];
                foreach (Tile b in GetAdjacentTiles(a))
                {
                    SwapTileContents(a, b);   // 임시 스왑
                    bool matchPossible = matchFinder.FindMatches().Count > 0;
                    SwapTileContents(a, b);   // 복귀

                    if (matchPossible)
                        return true;
                }
            }
        }
        return false;
    }

    private void SwapTileContents(Tile a, Tile b)
    {
        Block ba = a.currentBlock;
        Block bb = b.currentBlock;

        a.currentBlock = bb;
        b.currentBlock = ba;

        if (bb != null) { bb.gridPos = a.gridPos; bb.currentTile = a; }
        if (ba != null) { ba.gridPos = b.gridPos; ba.currentTile = b; }
    }

    public void ShuffleBoard()
    {
        List<Block> all = new();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                all.Add(tiles[x, y].currentBlock);

        for (int i = all.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (all[i], all[r]) = (all[r], all[i]);
        }

        int index = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Block b = all[index++];
                Tile t = tiles[x, y];

                t.currentBlock = b;
                b.currentTile = t;
                b.gridPos = t.gridPos;
                b.transform.position = t.transform.position;
            }
        }
    }
}
