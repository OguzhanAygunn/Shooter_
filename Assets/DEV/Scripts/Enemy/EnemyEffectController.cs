using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyEffectController : MonoBehaviour
{
    private Vector3 defaultScale;

    private void Start()
    {
        defaultScale = transform.localScale;
    }


    [Button(size: ButtonSizes.Large)]
    public async UniTaskVoid SmallSizeEffect()
    {
        Vector3 firstScale = defaultScale * 0.975f;
        Vector3 secondScale = defaultScale * 1.025f;
        Vector3 endScale = defaultScale;

        transform.DOScale(firstScale, 0.1f).SetEase(Ease.Linear);
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        transform.DOScale(secondScale, 0.1f).SetEase(Ease.Linear);
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
        transform.DOScale(endScale, 0.1f).SetEase(Ease.Linear);
        await UniTask.Delay(TimeSpan.FromSeconds(0.1f));
    }

}
