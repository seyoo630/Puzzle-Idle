using UnityEngine;

public class Character : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }
}

