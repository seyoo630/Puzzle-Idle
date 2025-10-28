using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int gridPos;  
    public Block currentBlock;   

    public void Init(Vector2Int pos)
    {
        gridPos = pos;
        name = $"Tile_{pos.x}_{pos.y}";
    }
}