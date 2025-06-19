using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCarWheele : MonoBehaviour
{
    [SerializeField] bool collGround;
    [SerializeField] bool rotateActive;
    [SerializeField] float rotateSpeed;

    [SerializeField] ParticleSystem particle;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] private readonly float rayDistance = 1.25f;

    private float defaultRotateSpeed;
    private void Awake()
    {
        defaultRotateSpeed = rotateSpeed;
    }
    private void Update()
    {
        ActiveControl();
        RotateWheel();
    }

    private void RotateWheel()
    {
        transform.Rotate(Vector3.right * rotateSpeed * Time.deltaTime, Space.Self);
    }

    public void SetActiveRotate(bool active) { 
        rotateActive = active;

        DOTween.To(() => rotateSpeed, x => rotateSpeed = x, active ? defaultRotateSpeed : 0, 0.15f);
    }


    private void ActiveControl()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, rayDistance, groundLayer))
        {
            if (!collGround)
                particle.Play(withChildren: true);

            collGround = true;
        }
        else
        {
            if (collGround)
                particle.Stop(withChildren: true);

            collGround = false;

        }
    }
}
