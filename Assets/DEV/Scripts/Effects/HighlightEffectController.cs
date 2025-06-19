using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightEffectController : MonoBehaviour
{
    [SerializeField] bool active = true;
    public List<HighlightEffectInfo> effects;

    private void Start()
    {
        effects.ForEach(e => e.Init());
    }

    public HighlightEffectInfo GetEffect(string id)
    {
        return effects.Find(effect => effect.id == id);
    }

    [Button(size: ButtonSizes.Large)]
    public void PlayEffect(string id)
    {
        if (!active)
            return;

        GetEffect(id).PlayEffect().Forget();
    }

}


[System.Serializable]
public class HighlightEffectInfo
{
    [Title("Main")]
    public string id;
    public Renderer renderer;
    public int materialIndex;

    [Space(6)]

    [Title("Effect")]
    public float defaultEmissionMultiplier; //f*ck
    public float emissionMultiplier;

    public float startSpeed;
    public float endSpeed;

    public float startDuration;
    public float endDuration;

    public float startDelay;
    public float endDelay;

    [ColorUsage(true, true)] public Color defaultColor;
    [ColorUsage(true, true)] public Color targetColor;

    public void Init()
    {
        defaultColor = Color.gray;
    }

    public async UniTaskVoid PlayEffect()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(startDelay));
     
        Color color = renderer.materials[materialIndex].color;
        float counter = 0;
        while (counter != startDuration)
        {
            counter = Mathf.MoveTowards(counter, startDuration, Time.fixedUnscaledDeltaTime);
            color = Color.Lerp(color, targetColor, startSpeed * Time.fixedUnscaledDeltaTime);
            renderer.materials[materialIndex].color = color;
            await UniTask.Delay(TimeSpan.FromSeconds(Time.fixedDeltaTime));
        }

        await UniTask.Delay(TimeSpan.FromSeconds(endDelay));

        counter = 0;
        while (counter != endDuration)
        {
            counter = Mathf.MoveTowards(counter, endDuration, Time.fixedUnscaledDeltaTime);
            color = Color.Lerp(color, defaultColor, endSpeed * Time.fixedUnscaledDeltaTime);
            renderer.materials[materialIndex].color = color;
            await UniTask.Delay(TimeSpan.FromSeconds(Time.fixedUnscaledDeltaTime));
        }
    }
}
