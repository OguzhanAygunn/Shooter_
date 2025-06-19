using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandWeaponManager : MonoBehaviour
{
    [Title("Mains")]
    [SerializeField] Transform targetPos;
    [SerializeField] WeaponType weaponType;

    [Space(6)]

    [Title("Weapons")]
    [SerializeField] Weapon currentWeapon;
    public Weapon Weapon
    {
        get { return currentWeapon; }
        set { currentWeapon = value; }
    }
    [SerializeField] List<Weapon> weapons;
    private Vector3 defaultPos;
    private Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        defaultPos = transform.localPosition;
        Init();
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        //transform.localPosition = defaultPos + (-offset * 0.25f);
    }

    public void Init()
    {
        UpdateCurrentWeapon();
    }

    private void UpdateCurrentWeapon()
    {
        DisableAllWeapons();

        Weapon newWeapon = GetWeapon(weaponType);

        if (!newWeapon)
            return;

        currentWeapon = newWeapon;
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.PlayActiveAnim();
    }

    private Weapon GetWeapon(WeaponType weaponType)
    {
        return weapons.Find(weapon => weapon.type == weaponType);
    }

    private void DisableAllWeapons()
    {
        weapons.ForEach(weapon => weapon.gameObject.SetActive(false));
    }
}
