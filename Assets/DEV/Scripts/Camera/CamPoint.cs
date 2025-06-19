using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CamPoint : MonoBehaviour
{
    private List<MeshRenderer> renderers;
    public void Init()
    {
        renderers = GetComponentsInChildren<MeshRenderer>().ToList();
        renderers.ForEach(r => r.enabled = false);
    }
}
