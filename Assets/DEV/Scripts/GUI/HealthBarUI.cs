using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Title("Main")]
    public bool active = true;
    public bool visibility = false;
    [SerializeField] Slider slider; 
    [SerializeField] EnemyHealth health;
    [SerializeField] Transform target;
    [SerializeField] Camera cam;
    [SerializeField] CanvasGroup group;

    [Space(6)]

    [Title("Highlight Effect")]
    [SerializeField] bool highlightEffectActive;
    [SerializeField] Image highlightImage;
    [SerializeField] float highlightEffectSpeed;
    [SerializeField] Color highlightColor;
    [SerializeField] float highlightDuration;
    private float highlightCounter;
    private Color defaultColor;
    private float visibilityCounter;

    private void Start()
    {
        defaultColor = highlightImage.color;

        UpdateSliderValue();
    }

    public void AssignHealth(EnemyHealth newHealth)
    {
        health = newHealth;
    }

    public void AssignTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void AssignCam(Camera newCam)
    {
        cam = newCam;
    }

    private void LateUpdate()
    {
        if (!target || !cam)
            return;

        Vector3 pos = cam.WorldToScreenPoint(target.position);
        transform.position = pos;


        HighlightController();
        CounterController();
        VisibilityController();

    }



    Color color;
    Color targetColor;
    private void HighlightController()
    {
        color = highlightImage.color;
        targetColor = highlightEffectActive ? highlightColor : defaultColor;

        color = Color.Lerp(color, targetColor, highlightEffectSpeed * Time.deltaTime);

        highlightImage.color = color;
    }

    [Button(size: ButtonSizes.Large)]
    public void HighlightActive()
    {
        highlightEffectActive = true;

        highlightCounter = 0;
    }

    public void UpdateSliderValue()
    {
        float value = health.health / health.maxHealth;

        slider.value = value;
    }

    public void TakeHit()
    {
        UpdateSliderValue();
        HighlightActive();
    }

    private void CounterController()
    {
        if (!highlightEffectActive)
            return;

        highlightCounter = Mathf.MoveTowards(highlightCounter, highlightDuration, Time.deltaTime);

        if (highlightCounter == highlightDuration)
        {
            highlightEffectActive = false;
        }
    }

    private void VisibilityController()
    {
        if (active)
        {
            float alpha = group.alpha;
            float targetAlpha = 0;

            if (visibility)
            {
                targetAlpha = 1f;
            }
            else
            {
                if(visibilityCounter == 1)
                {
                    targetAlpha = 0;
                }
            }

            alpha = Mathf.MoveTowards(alpha, targetAlpha, 10 * Time.deltaTime);
            group.alpha = alpha;
            visibilityCounter = Mathf.MoveTowards(visibilityCounter, 1f, Time.deltaTime);
        }
    }

    public void SetVisibility(bool active)
    {
        visibilityCounter = 0;
        visibility = active;
    }


    public void DeActive()
    {
        active = false;
        group.DOFade(0, 0.25f).OnComplete( () =>
        {
            gameObject.SetActive(false);
        });
    }
}
