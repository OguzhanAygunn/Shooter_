using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] int targetFPS;

    public bool BloodSpawn;

    private void Awake()
    {
        instance = (!instance) ? this : instance;   
    }

    void Start()
    {
        Application.targetFrameRate = targetFPS;
    }
}
