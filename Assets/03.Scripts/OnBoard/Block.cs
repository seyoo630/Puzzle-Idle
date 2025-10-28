using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockType type;          
    public Vector2Int gridPos;     

    public void Init(BlockType type, Vector2Int pos)
    {
        this.type = type;
        this.gridPos = pos;
        name = $"Block_{type}_{pos.x}_{pos.y}";
    }
}