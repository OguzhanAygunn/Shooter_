using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadBar : MonoBehaviour
{
    Vector3 scale;

    private void Awake()
    {
        scale = transform.localScale;
        transform.localScale = Vector3.zero;
    }

    public void ResetBar()
    {
        transform.localScale = Vector3.zero;
    }

    public async UniTaskVoid PlayAnim(float delay)
    {
        transform.localScale = Vector3.zero;
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        transform.DOScale(scale, 0.1f).SetEase(Ease.Unset);
    }

}
