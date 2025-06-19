using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;


    public PlayerCollision collision;
    public PlayerHealth health;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeHit(damage: 10);
        }
    }


    [Button(size: ButtonSizes.Large)]
    public void TakeHit(int damage)
    {
        TakeHitRedFilter.instance.Play();
        CameraManager.Shake("Take Hit");
    }
}
