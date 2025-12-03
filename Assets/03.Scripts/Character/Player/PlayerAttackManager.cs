using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlayerAttackManager : MonoBehaviour
{
    public static PlayerAttackManager Instance { get; private set; }

    private Dictionary<BlockType, Character> characterMap;

    private Dictionary<BlockType, int> bubbleCount =>
        BubbleBlockSpawner.Instance.GetBubbleCountStack();

    private void Awake()
    {
        Instance = this;

        characterMap = new Dictionary<BlockType, Character>
        {
            { BlockType.Sword,  Object.FindFirstObjectByType<Warrior>() },
            { BlockType.Bow,    Object.FindFirstObjectByType<Archer>() },
            { BlockType.Magic,  Object.FindFirstObjectByType<Magician>() },
            { BlockType.Heal,   Object.FindFirstObjectByType<Healer>() },
            { BlockType.Shield, Object.FindFirstObjectByType<Tanker>() }
        };
    }

    public void StartPlayerAttack()
    {
        StartCoroutine(PlayerAttackRoutine());
    }

    private IEnumerator PlayerAttackRoutine()
    {
        var valid = bubbleCount
            .Where(kvp => kvp.Value > 0)
            .OrderByDescending(kvp => kvp.Value)
            .ThenBy(kvp => Random.value)
            .ToList();

        if (valid.Count == 0)
            yield break;

        foreach (var kvp in valid)
        {
            BlockType type = kvp.Key;
            int count = kvp.Value;

            Character cha = characterMap[type];
            if (cha == null) continue;

            yield return cha.PerformAttack(count, type);
        }

        UIManager.Instance.ResetPlayerMoves();

        foreach (var cha in characterMap.Values)
            cha?.ReduceBuffTurn();
    }
}
