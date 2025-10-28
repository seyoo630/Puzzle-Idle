using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public enum BlockType
{
    Shield,
    Sword,
    Bow,
    Magic,
    Heal
}
public class BlockPool : MonoBehaviour
{
    [Header("Block Prefabs")]
    public GameObject[] prefabs; //블록 프리팹 생성

    [System.Serializable]
    public class PoolNode //풀에 들어갈 블록 노드들, 타입 및 개수정보 포함
    {
        public BlockType type;
        public GameObject prefab;
        public int count = 10;
    }

    public PoolNode[] poolNodes;
    private Dictionary<BlockType, Queue<GameObject>> poolDict; 

    public void CreateNodes()
    {
        if(poolDict != null)
        {
            Debug.LogWarning("이미 생성된 Pool 존재");
            return;
        }

        var types = System.Enum.GetValues(typeof(BlockType));
        var nodes = new List<PoolNode>();

        for(int i = 0; i < types.Length; i++)
        {
            var node = new PoolNode();
            node.type = (BlockType)types.GetValue(i);
            node.prefab = i < prefabs.Length ? prefabs[i] : null;
            node.count = 10;
            nodes.Add(node);
        }
        this.poolNodes = nodes.ToArray();
       
    }

    public void CreatePool()
    {
        poolDict = new Dictionary<BlockType, Queue<GameObject>>();

        foreach (var item in poolNodes)
        {

            Queue<GameObject> queue = new Queue<GameObject>();
            for (int i = 0; i < item.count; i++)
            {
                GameObject obj = Instantiate(item.prefab, transform);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDict.Add(item.type, queue);
        }
    }

    public GameObject GetNodeFromPool(BlockType type, Vector3 pos)
    {
        if (!poolDict.ContainsKey(type))
        {
            Debug.LogError($"풀에 등록된 타입 없음");
            return null;
        }

        GameObject obj = poolDict[type].Count > 0 ? poolDict[type].Dequeue() : Instantiate(poolNodes[0].prefab, transform);

        obj.SetActive(true);
        obj.transform.localPosition = pos;

        Debug.Log($"풀에 등록된 블록 소환: {obj}");
        return obj;
    }

    public void ReturnToPool(GameObject obj, BlockType type)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
        poolDict[type].Enqueue(obj);
    }
}
