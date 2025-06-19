using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineController : MonoBehaviour
{
    [SerializeField] Outline outline;

    public void SetActive(bool active)
    {
        outline.enabled = active;
    }
}
