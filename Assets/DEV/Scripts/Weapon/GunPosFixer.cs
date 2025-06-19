using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunPosFixer : MonoBehaviour
{
    Transform targetParent;
    Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        targetParent = transform.parent;

        transform.parent = null;

        offset = transform.position - targetParent.position;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = targetParent.position + offset;
    }
}
