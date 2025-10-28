using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game References")]
    public Board board;
    public BlockPool blockPool;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        blockPool.CreateNodes();
        blockPool.CreatePool();

        board.GenerateBoard();
        
    }

}
