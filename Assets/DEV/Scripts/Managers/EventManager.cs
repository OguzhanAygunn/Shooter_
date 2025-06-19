using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    public static EventManager instance;

    public int killCount;

    [SerializeField] List<EventActionInfo> eventInfos;

    private void Awake()
    {
        instance = this;
    }


    public static void KillCountIncrease()
    {
        instance.killCount++;

        EventActionInfo info = instance.eventInfos.Find(_event => _event.killIndex == instance.killCount && !_event.isActivated);
        info?.PlayEvents();
    }

}


[System.Serializable]
public class EventActionInfo
{
    public bool isActivated;
    public int killIndex;
    public List<UnityEvent> events;

    public void PlayEvents()
    {
        events.ForEach(_event => _event.Invoke());
    }
}
