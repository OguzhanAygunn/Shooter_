using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class MovementSystem : MonoBehaviour
{
    public static MovementSystem instance;
    public bool active = false;
    public bool isMove;

    public Transform playerPos;
    private TrenchManager trenchManager;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    private void Start()
    {
        trenchManager = TrenchManager.instance;

        Init().Forget();
    }

    public async UniTask Init()
    {
        TrenchPos pos = trenchManager.currentArea.pos;
        MoveToTrenchPos(trenchPos: pos, delay: 2f).Forget();

        await UniTask.Delay(TimeSpan.FromSeconds(2f));
        active = true;
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {

            Move(rightMove: true);
            return;
            int index = trenchManager.currentArea.index;

            index++;

            if (index >= trenchManager.currentTrench.areas.Count || index < 0)
            {
                return;
            }

            TrenchPos trenchPos = trenchManager.currentTrench.areas[index].pos;
            trenchManager.currentArea = trenchManager.currentTrench.areas[index];
            MoveToTrenchPos(trenchPos: trenchPos, delay: 0f).Forget();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(rightMove: false);
            return;
            int index = trenchManager.currentArea.index;

            index--;


            if (index >= trenchManager.currentTrench.areas.Count || index < 0)
            {
                return;
            }

            TrenchPos trenchPos = trenchManager.currentTrench.areas[index].pos;
            trenchManager.currentArea = trenchManager.currentTrench.areas[index];
            MoveToTrenchPos(trenchPos: trenchPos, delay: 0f).Forget();
        }


    }

    public void Move(bool rightMove)
    {
        if (!active)
            return;

        if (isMove)
            return;

        int index = trenchManager.currentArea.index;


        index = rightMove ? index + 1 : index - 1;

        

        if(index >= trenchManager.currentTrench.areas.Count || index < 0)
        {
            return;
        }

        TrenchPos trenchPos = trenchManager.currentTrench.areas[index].pos;
        trenchManager.currentArea = trenchManager.currentTrench.areas[index];
        MoveToTrenchPos(trenchPos: trenchPos, delay: 0f).Forget();
    }


    [Button(size: ButtonSizes.Large)]
    public async UniTaskVoid MoveToTrenchPos(TrenchPos trenchPos, float delay = 0)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        if (isMove)
            return;

        CameraParentController.instance.SetActiveAnim(true);

        isMove = true;



        Vector3 startRotate = playerPos.eulerAngles;
        Vector3 startLookPos = trenchPos.transform.position + trenchPos.transform.TransformDirection(Vector3.forward * 20);
        Vector3 startPos = playerPos.position;

        Vector3 endPos = trenchPos.transform.position;
        Vector3 endLookPos = trenchPos.transform.position + trenchPos.transform.TransformDirection(Vector3.forward * 5);
        Vector3 pos = playerPos.position;

        AnimationCurve moveCurve = CurveManager.GetCurve("Move");
        AnimationCurve lookCurve = CurveManager.GetCurve("Look");


        float distance = Vector3.Distance(startPos, endPos);
        float counter = 0;
        float duration = distance * 0.05f;
        float slerpVal = 0;
        float lookDuration = duration * 0.5f;
        playerPos.DODynamicLookAt(startLookPos, lookDuration).SetEase(animCurve: lookCurve);


        while (distance > 0.25f)//slerpVal != 1f)
        {
            /*counter = Mathf.MoveTowards(counter, duration, Time.deltaTime);
            slerpVal = counter / duration;
            pos = Vector3.Slerp(startPos, endPos, slerpVal);
            playerPos.position = pos;*/


            pos = Vector3.MoveTowards(pos, endPos, 30 * Time.deltaTime);
            playerPos.position = pos;
            distance = Vector3.Distance(pos, endPos);
            await UniTask.Delay(TimeSpan.FromSeconds(Time.deltaTime));
        }


        isMove = false;
        CameraParentController.instance.SetActiveAnim(false);

        playerPos.DODynamicLookAt(endLookPos, lookDuration).SetEase(animCurve: lookCurve);
    }
}
