using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeHitRedFilter : MonoBehaviour
{
    public static TakeHitRedFilter instance;


    public bool active;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] AnimationCurve curve;
    [SerializeField] float upDuration;
    [SerializeField] float downDuration;
    [SerializeField] float delay;


    private void Awake()
    {
        instance = this;
    }


    public void Play()
    {
        if (active)
            return;

        active = true;


        canvasGroup.DOFade(1f, upDuration).SetEase(curve).OnComplete( () =>
        {
            canvasGroup.DOFade(0f, downDuration).SetEase(curve).SetDelay(delay).OnComplete( () =>
            {
                active = false;
            });
        });
    }

}
