using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AnomalyController : MonoBehaviour
{
    public static AnomalyController instance;
    private Animator animator;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
        animator = GetComponent<Animator>();
    }



    public static void PlayAnim()
    {
        instance.animator.SetTrigger("AnomalyIntensity");
    }

}

