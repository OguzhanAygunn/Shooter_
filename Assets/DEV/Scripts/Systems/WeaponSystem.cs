using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] HandWeaponManager weaponManager;


    private void Start()
    {

    }


    public void Shoot()
    {
        weaponManager.Weapon.Shoot();
    }

    public void Reload()
    {
        weaponManager.Weapon.Reload();
    }
}
