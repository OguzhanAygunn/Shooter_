using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class uiExperimental : MonoBehaviour
{
    [SerializeField] bool active;
    [SerializeField] Transform target;
    [SerializeField] Camera cam;


    private void LateUpdate()
    {
        if (!active)
            return;

        if (!target || !cam)
            return;

        Vector3 pos = cam.WorldToScreenPoint(target.position);

        transform.position = pos;
    }
}
