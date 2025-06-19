using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyPartController : MonoBehaviour
{
    [SerializeField] List<BodyPartInfo> bodyParts;

    private void Awake()
    {
        bodyParts.ForEach(bodyPart => bodyPart.Init());   
    }


    [Button(size: ButtonSizes.Large)]
    public async UniTaskVoid DestroyAllParts(float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        foreach(BodyPartInfo part in bodyParts)
        {
            part.FakeDestroy();
        }

    }
}


[System.Serializable]
public class BodyPartInfo
{
    [Title("Main")]
    public Transform bodyPart;
    private Vector3 defaultScale;
    private Vector3 defaultPosition;
    private Vector3 defaultRotation;

    [Space(6)]

    [Title("Anim Values")]
    public float duration;
    public float delay;
    private AnimationCurve curve;

    public void Init()
    {
        defaultScale = bodyPart.localScale;
        defaultPosition = bodyPart.localPosition;
        defaultRotation = bodyPart.localEulerAngles;
    }


    public void FakeDestroy()
    {

        bodyPart.DOScale(Vector3.zero, duration).SetEase(curve).SetDelay(delay).OnComplete( () =>
        {
            Debug.Log("Completed");
        });
        Debug.Log("Fake Destroy");
    }
}
