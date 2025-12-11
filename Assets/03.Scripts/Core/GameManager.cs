using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game References")]
    public Board board;
    public BlockPool blockPool;

    public bool IsInputLocked { get; private set; } = false;

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

        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitUI();
        }

        if (TurnNotifier.Instance != null)
            TurnNotifier.Instance.PlayPlayerTurn();

    }

    public void LockInput()
    {
        IsInputLocked = true;
        Debug.Log("Input Locked");
    }

    public void UnlockInput()
    {
        IsInputLocked = false;
        Debug.Log("Input Unlocked");
    }

}
