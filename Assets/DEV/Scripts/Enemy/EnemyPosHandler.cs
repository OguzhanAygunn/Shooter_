using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPosHandler : MonoBehaviour
{
    public bool parentIsNull;
    public EnemyPosHandler nextEnemyHandler;
    public ArmedEnemyPosType armedEnemyAnimType;
    public bool ToPlayer;
    public bool attackActive = false;
    [ShowIf("attackActive")] public float attackActiveDelay = 0;
    MeshRenderer render;

    private void Awake()
    {
        if (parentIsNull)
            transform.SetParent(null);

        render = GetComponent<MeshRenderer>();
        render.enabled = false;
    }
}
