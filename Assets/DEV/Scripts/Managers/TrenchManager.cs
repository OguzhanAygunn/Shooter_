using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrenchManager : MonoBehaviour
{
    public static TrenchManager instance;
    [Title("Trencs")]
    public List<TrenchInfo> trenchs;
    public Trench currentTrench;

    public TrenchArea currentArea;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
        SetTrench(index: 0);
    }


    public void SetTrench(int index)
    {
        currentTrench = trenchs[index].trench;
        currentArea = currentTrench.areas[1];
    }



}

[System.Serializable]
public class TrenchInfo
{
    [Title("Main")]
    public Trench trench;
    public List<TrenchArea> areas;

    [Space(6)]

    [Title("Events")]
    public List<EventInfo> startEvents;
    public List<EventInfo> endEvents;


    public void PlayStartEvents()
    {
        startEvents.ForEach(e => e.PlayEvent().Forget());
    }

    public void PlayEndEvents()
    {
        endEvents.ForEach(e => e.PlayEvent().Forget());
    }
}


[System.Serializable]
public class EventInfo
{
    public UnityEvent func;
    public float delay;

    public async UniTaskVoid PlayEvent()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));
        func.Invoke();
    }
}
