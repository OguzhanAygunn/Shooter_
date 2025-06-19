using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarPosController : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool toUpPos;
    [SerializeField] float moveSpeed;

    [Space(6)]

    [Title("Poses")]
    [SerializeField] Vector3 upPos;
    [SerializeField] Vector3 defaultPos;
    

    private void Awake()
    {
        defaultPos = transform.localPosition;
    }

    private void Update()
    {
        MoveController();
    }

    private void MoveController()
    {

        Vector3 pos = transform.localPosition;
        Vector3 targetPos = toUpPos ? upPos : defaultPos;

        pos = Vector3.MoveTowards(pos, targetPos, moveSpeed * Time.deltaTime);
        transform.localPosition = pos;
    }


    public void SetActiveToUpPos(bool active)
    {
        toUpPos = active;
    }
}
