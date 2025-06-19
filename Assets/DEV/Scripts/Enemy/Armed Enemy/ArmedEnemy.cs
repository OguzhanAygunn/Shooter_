using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ArmedEnemyPosType { Crouch, CrouchFire, Idle, Run }

public class ArmedEnemy : Enemy
{
    [Title("Armed Enemy")]
    [SerializeField] EnemyWeapon weapon;
    [SerializeField] ArmedEnemyPosType posType;

    [Space(6)]

    [Title("Blend")]
    [SerializeField] float targetBlend;
    [SerializeField] float blendSpeed;



    private void Update()
    {
        AnimatorController();
    }

    public EnemyWeapon GetWeapon()
    {
        return weapon;
    }


    [Button(size: ButtonSizes.Large)]
    public async void SetPosType(ArmedEnemyPosType newType,float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        posType = newType;

        switch (posType)
        {
            case ArmedEnemyPosType.Crouch:
                targetBlend = -1;
                break;
            case ArmedEnemyPosType.CrouchFire:
                targetBlend = 0;
                break;
            case ArmedEnemyPosType.Idle:
                targetBlend = 1;
                break;
            case ArmedEnemyPosType.Run:
                targetBlend = 2;
                break;
        }

        healthBarPosController.SetActiveToUpPos(active: targetBlend >= 1);

        movementController.MoveActive = posType is ArmedEnemyPosType.Run;
    }

    private void AnimatorController()
    {
        float blend = animator.GetFloat("Blend");
        blend = Mathf.MoveTowards(blend, targetBlend, blendSpeed * Time.deltaTime);
        animator.SetFloat("Blend", blend);
    }

    [Button(size: ButtonSizes.Large)]
    public void Shoot()
    {
        weapon.Shoot();
    }


    private void ShootController()
    {

    }

}
