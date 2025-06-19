using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum WeaponState { idle, Run, Reload }
public enum WeaponType { Null, Pistol1, Pistol2, Rifle1, Rifle2 }
public class WeaponManager : MonoBehaviour
{
    public static WeaponManager instance;
    [SerializeField] List<Weapon> weapons;
    public Weapon weapon;
    public Weapon Weapon
    {
        get
        {
            return weapon;
        }
        set { 
            weapon = value;
        }

    }


    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }
}
