using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private readonly float deActiveTime = 2;
    private float counter;

    [Title("Speed")]
    public float moveSpeed = 320;


    [Title("Particle & Trail")]
    [SerializeField] ParticleSystem particle;
    [SerializeField] TrailRenderer trailRenderer;

    [Space(6)]

    [Title("Collision")]
    [SerializeField] float radius;
    [SerializeField] LayerMask collableLayers;

    [Space(6)]

    [Title("Effects")]
    [SerializeField] FXType collGroundEffect;
    [SerializeField] FXType collEnemyEffect;
    private Rigidbody rigidbody;
    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void LateUpdate()
    {
    }

    private void Update()
    {
        Move();
        DeActiveController();
    }

    private void DeActiveController()
    {
        counter = Mathf.MoveTowards(counter, deActiveTime, Time.deltaTime);

        if (counter == deActiveTime)
            gameObject.SetActive(false);
    }

    private void Move()
    {
        Vector3 pos = transform.position + transform.TransformDirection(Vector3.forward * moveSpeed) * Time.deltaTime;
        rigidbody.MovePosition(pos);
        //Radar();
    }

    public void Shoot()
    {
        particle.Clear(withChildren: true);
        trailRenderer.Clear();
        counter = 0;

        particle.Play(withChildren: true);
    }


    private void Radar()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, collableLayers);

        GameObject collObj = null;
        if (colliders.Length > 0)
            collObj = colliders.First().gameObject;
        else
            return;

        if (collObj.layer == LayerMask.NameToLayer("Player"))
        {
            FXManager.PlayParticle(collGroundEffect, transform.position, 2f).Forget();
            gameObject.SetActive(false);
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.TakeHit(damage: 5);
            gameObject.SetActive(value: false);
        }
    }
}
