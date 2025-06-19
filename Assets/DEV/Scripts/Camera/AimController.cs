using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimController : MonoBehaviour
{
    public static AimController instance;
    [Title("Main")]
    [SerializeField] public  Transform aim;
    [SerializeField] CanvasGroup group;
    
    [Space(6)]

    [Title("Movement")]
    [SerializeField] float moveSpeed;
    [SerializeField] float sensitivity;
    Vector3 lastMousePosition;
    Vector3 targetPos;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
        targetPos = Input.mousePosition;
    }

    public Transform GetAimTrs()
    {
        return aim;
    }


    private void Update()
    {
        Movement();
    }


    bool onDown = false;
    private void Movement()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            targetPos = Input.mousePosition;
            onDown = true;
        }
        else
        {
            onDown = false;
        }


        Vector3 mousePos = Input.mousePosition;
        Vector3 deltaPos = mousePos - lastMousePosition;
        deltaPos *= sensitivity * Time.deltaTime;
        targetPos += deltaPos;
        if (Input.GetMouseButton(0))
        {

            aim.Translate(deltaPos);
            //aim.transform.localPosition = Vector3.Lerp(aim.transform.localPosition, targetPos, 1 * Time.deltaTime);
        }


        Vector3 pos = aim.transform.localPosition;
        pos.x = Mathf.Clamp(pos.x, -960, 960);
        pos.y = Mathf.Clamp(pos.y, -540, 540);
        aim.transform.localPosition = pos;
        
        lastMousePosition = mousePos;
    }
}
