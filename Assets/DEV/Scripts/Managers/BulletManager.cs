using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager instance;

    public GameObject bulletPrefab;
    public GameObject cartridgePrefab;

    public List<Bullet> bullets;
    public int bulletCount;

    public List<Cartridge> cartridges;
    public int cartridgeCount;

    private Transform bulletParent;
    private Transform cartridgeParent;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    private void Start()
    {
        bulletParent = new GameObject("Bullet Parent").transform;
        cartridgeParent = new GameObject("Cartridge Parent").transform;
        Pool();
    }


    private void Pool()
    {
        int index = 0;

        while (index < bulletCount)
        {
            Bullet bullet = Instantiate(bulletPrefab).GetComponent<Bullet>();
            bullet.transform.parent = bulletParent;

            bullet.gameObject.SetActive(false);
            bullets.Add(bullet);
            index++;
        }

        index = 0;

        while (index < cartridgeCount)
        {
            Cartridge cartridge = Instantiate(cartridgePrefab).GetComponent<Cartridge>();
            cartridge.transform.parent = cartridgeParent;
            cartridge.transform.rotation = cartridgePrefab.transform.rotation;

            cartridge.gameObject.SetActive(false);
            cartridges.Add(cartridge);
            index++;
        }
    }

    public static Bullet GetBullet()
    {
        return instance.bullets.Find(bullet => !bullet.gameObject.activeInHierarchy);
    }

    public static Cartridge GetCartridge()
    {
        return instance.cartridges.Find(cartridge => !cartridge.gameObject.activeInHierarchy);
    }
}
