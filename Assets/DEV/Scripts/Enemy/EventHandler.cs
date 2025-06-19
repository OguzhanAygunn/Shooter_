using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class EventHandler : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool active = true;
    [SerializeField] string startID;

    [Space(6)]
    [Title("Events")]
    [SerializeField] List<UnityEventInfo> eventHandlers;

    [Space(20)]

    [Title("Buttons")]
    public bool showButtons;


    //Others
    List<Renderer> renderers = new List<Renderer>();
    List<Collider> colliders = new List<Collider>();
    List<Rigidbody> rigidies = new List<Rigidbody>();
    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>().ToList().FindAll(renderer => renderer.enabled == true).ToList();
        colliders = GetComponentsInChildren<Collider>().ToList();
        rigidies = GetComponentsInChildren<Rigidbody>().ToList().FindAll(rigid => rigid.isKinematic == false).ToList();
        PlayeEvents(startID);
    }

    [ShowIf("showButtons")]
    [Button(size: ButtonSizes.Large)]
    public void PlayeEvents(string id)
    {
        UnityEventInfo eventInfo = GetEventInfo(id);

        eventInfo?.PlayEvents();
    }

    public UnityEventInfo GetEventInfo(string id)
    {
        return eventHandlers.Find(handler => handler.id == id);
    }


    public void RenderersActive(bool active)
    {
        renderers.ForEach(renderer => renderer.enabled = active);
        CollidersActive(active: active);
        RigidiesSetActive(active: active);
    }

    public void CollidersActive(bool active)
    {
        colliders.ForEach(collider => collider.enabled = active);
    }

    public void RigidiesSetActive(bool active)
    {
        rigidies.ForEach(rigid => rigid.isKinematic = active);
    }

}


[System.Serializable]
public class UnityEventInfo
{
    public string id;
    public List<UnityEvent> events;

    public void PlayEvents()
    {
        events.ForEach(e => e.Invoke());
    }
}
