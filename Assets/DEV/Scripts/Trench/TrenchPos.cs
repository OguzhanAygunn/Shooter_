using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrenchPos : MonoBehaviour
{
    private void Start()
    {
        Destroy(GetComponent<MeshFilter>());
        Destroy(GetComponent<MeshRenderer>());

        GameObject child = transform.GetChild(0).gameObject;
        Destroy(child);
    }
}
