using FIMSpace.FProceduralAnimation;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Title("Main")]
    public float health;
    public float maxHealth;
    public int defenceVal;

    private Animator animator;
    private RagdollAnimator2 ragdollAnimator;
    [Space(6)]

    [Title("Health Increase")]
    [SerializeField] bool healingMode;
    [ShowIf("@healingMode == true")][SerializeField] bool healingActive;
    [ShowIf("@healingMode == true")][SerializeField] float increaseActiveTime;
    [ShowIf("@healingMode == true")][SerializeField] float increaseVal;

    private Enemy enemy;
    // Start is called before the first frame update
    void Start()
    {
        enemy = GetComponent<Enemy>();
        animator = GetComponent<Animator>();
        ragdollAnimator = GetComponent<RagdollAnimator2>();
    }

    public void DecreaseHealth(int decreaseVal)
    {
        if (!enemy.isAlive)
            return;


        health -= decreaseVal;

        if (health <= 0)
        {
            enemy.SetActiveAlive(active: false);
        }

    }
}
