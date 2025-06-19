using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    public List<TimeAnimInfo> anims;

    public bool animActive;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayAnim("1");
        }
    }*/

    public static TimeAnimInfo GetAnimInfo(string id)
    {
        return instance.anims.Find(x => x.id == id);
    }

    public static void PlayAnim(string id)
    {
        if (instance.animActive)
            return;

        TimeAnimInfo info = GetAnimInfo(id: id);

        info.PlayAnim().Forget();
    }

}


[System.Serializable]
public class TimeAnimInfo
{
    [Title("Main")]
    public string id;
    public bool active;

    [Space(6)]

    [Title("Time Scales")]
    public float startTimeScale;
    public float endTimeScale;

    [Space(6)]

    [Title("Durations")]
    public float startDuration;
    public float endDuration;

    [Space(6)]

    [Title("Delays")]
    public float startDelay;
    public float endDelay;

    [Space(6)]

    [Title("Anim Curves")]
    public AnimationCurve startCurve;
    public AnimationCurve endCurve;


    public async UniTaskVoid PlayAnim()
    {
        TimeManager.instance.animActive = true;

        DOTween.To(x => Time.timeScale = x, Time.timeScale, startTimeScale, startDuration).SetDelay(startDelay).SetEase(animCurve: startCurve);

        await UniTask.Delay(TimeSpan.FromSeconds(endDelay));

        DOTween.To(x => Time.timeScale = x, Time.timeScale, endTimeScale, endDuration).SetEase(animCurve:endCurve);

        await UniTask.Delay(TimeSpan.FromSeconds(endDuration));

        TimeManager.instance.animActive = false;

    }
}
