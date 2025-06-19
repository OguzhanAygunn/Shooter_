using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyWeapon : MonoBehaviour
{
    [Title("Attack")]
    [SerializeField] bool attackActive = false;
    [SerializeField] bool isFree = false;
    [SerializeField] bool attackable = false;
    public bool Attackable
    {
        get
        {
            return attackable;
        }
        set
        {
            attackable = value;
        }
    }
    [SerializeField] float duration;
    [SerializeField] float counter;


    [Space(6)]

    [Title("Effects")]
    [SerializeField] FXType effect1Type;
    [SerializeField] FXType effect2Type;
    [SerializeField] Transform effectSpawnPos;


    private Rigidbody rigid;
    private MeshCollider coll;
    private ArmedEnemy armedEnemy;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        coll = GetComponent<MeshCollider>();
        armedEnemy = GetComponentInParent<ArmedEnemy>();
        Free(active: false);
        Attackable = false;
    }

    private void Update()
    {
        ShootController();
    }


    private void ShootController()
    {
        attackActive = true;//armedEnemy.animator.GetFloat("Blend") == 1;

        if (!attackActive || isFree || !attackable)
            return;

        counter = Mathf.MoveTowards(counter, duration, Time.deltaTime);

        if (counter == duration)
        {
            Shoot();
        }
    }

    public async UniTaskVoid SetAttackable(bool active,float delay)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

        Attackable = active;
    }

    [Button(size: ButtonSizes.Large)]
    public void Shoot()
    {
        //Counter Reset
        counter = 0;

        //Play FX
        FXManager.PlayParticle(effect1Type, effectSpawnPos.position, 2f).Forget();
        FXManager.PlayParticle(effect2Type, effectSpawnPos.position, 2f).Forget();

        //Bullet
        Bullet bullet = BulletManager.GetBullet();
        bullet.gameObject.SetActive(true);

        Vector3 lookPos = GetLookPos();

        bullet.transform.position = effectSpawnPos.position;
        bullet.transform.LookAt(lookPos);
        bullet.Shoot();
    }

    private Vector3 GetLookPos()
    {
        Vector3 lookPos = PlayerController.instance.transform.position;

        lookPos.x += Random.Range(-4f, 4f);
        lookPos.y += Random.Range(-4f, 4f);
        lookPos.z += Random.Range(-4f, 4f);

        return lookPos;
    }


    public void Free(bool active)
    {
        if (active)
        {
            transform.SetParent(null);
            rigid.constraints = RigidbodyConstraints.None;
            coll.enabled = true;
        }
        else
        {
            rigid.constraints = RigidbodyConstraints.FreezeAll;
            coll.enabled = true;
        }

        isFree = active;

    }
}
