using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager instance;

    [Title("Random")]
    [SerializeField] int RANDOM_min_gold = 10;
    [SerializeField] int RANDOM_max_gold = 25;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    public static int GetRandomGoldValue()
    {
        return Random.Range(instance.RANDOM_min_gold, instance.RANDOM_max_gold + 1);
    }

}
