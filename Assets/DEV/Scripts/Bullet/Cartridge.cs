using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cartridge : MonoBehaviour
{
    public async UniTaskVoid PlayAnim()
    {
        //Default Values
        Transform defaultParent = transform.parent;
        Vector3 defaultPos = transform.position;
        Vector3 defaultRot = transform.localEulerAngles;

        //Anim Values;
        float jumpPower = UnityEngine.Random.Range(2f, 2.1f);
        float jumpDuration = UnityEngine.Random.Range(2.2f, 2.5f);
        Vector3 targetPos = defaultPos + Vector3.right * UnityEngine.Random.Range(1.2f, 2.2f);
        targetPos.y -= 3f;

        Vector3 targetRotate = defaultRot + new Vector3(UnityEngine.Random.Range(-120f, 120f), UnityEngine.Random.Range(-120f, 120f), UnityEngine.Random.Range(-120f, 120f));


        //Play Anim
        transform.parent = null;
        transform.DOJump(endValue: targetPos, jumpPower: jumpPower, numJumps: 1, duration: jumpDuration);
        transform.DOLocalRotate(targetRotate, jumpDuration);

        //Wait
        await UniTask.Delay(TimeSpan.FromSeconds(jumpDuration));

        //DeActive
        transform.parent = defaultParent;
        transform.localEulerAngles = Vector3.zero;
        transform.localPosition = defaultPos;

        gameObject.SetActive(false);
    }
}
