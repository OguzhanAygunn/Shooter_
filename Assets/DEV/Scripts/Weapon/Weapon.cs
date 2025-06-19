using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Title("Main")]
    public WeaponType type;
    public WeaponAnimator weaponAnimator;
    [SerializeField] WeaponSlotUI weaponSlotUI;

    [Space(6)]

    [Title("States")]
    public bool isReload;
    public bool isIdle;
    [Space(6)]

    [Title("Damage")]
    [SerializeField] int minDamage;
    [SerializeField] int maxDamage;
    public int Damage
    {
        get
        {
            return UnityEngine.Random.Range(minDamage, maxDamage + 1);
        }

    }

    [Space(6)]

    [Title("Shoot")]
    [SerializeField] int bulletCapacity;
    [SerializeField] int bulletCount;
    public int BulletCount
    {
        get
        {
            return bulletCount;
        }
        set
        {
            bulletCount = value;

            if (bulletCount == 0)
            {
                Reload();
            }

            weaponSlotUI.UpdateAmmountText();

        }
    }
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform bulletSpawnPos;
    [SerializeField] float shootDuration;
    [SerializeField] LayerMask collableLayers;

    [Space(6)]
    [Title("Look")]
    public bool lookActive = false;
    [SerializeField] Transform lookTarget;
    [SerializeField] float lookSpeed;

    [Space(6)]
    [Title("Anim")]
    [SerializeField] bool shootAnimActive;
    [SerializeField] float shootAnimSpeed;
    [SerializeField] Transform meshTrs;
    [SerializeField] Vector3 shootPos;
    private Animator animator;
    private MovementSystem movementSystem;
    private void Awake()
    {
        Init();
    }
    private void Start()
    {
        lookTarget = AimController.instance.GetAimTrs();
        animator = GetComponent<Animator>();
        movementSystem = MovementSystem.instance;
    }

    public void Init()
    {
        bulletCount = bulletCapacity;
    }

    private void LateUpdate()
    {
        ShootAnimController();
    }

    private void Update()
    {
        Look();
        if (Input.GetMouseButtonDown(1))
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Reload();
        }
    }


    public void ShootAnimController()
    {
        Vector3 pos = shootAnimActive ? shootPos : Vector3.zero;
        meshTrs.localPosition = Vector3.MoveTowards(meshTrs.localPosition, pos, shootAnimSpeed * Time.deltaTime);


        if (shootAnimActive)
            shootAnimActive = meshTrs.localPosition == pos ? !shootAnimActive : shootAnimActive;
    }

    public async void PlayActiveAnim()
    {
        //Get Defaults
        Vector3 defaultPos = transform.localPosition;
        Vector3 defaultRot = transform.localEulerAngles;

        //Spawn Pos
        Vector3 spawnPos = transform.localPosition;
        spawnPos.y -= 2f;
        transform.localPosition = spawnPos;


        //Spawn Rot
        Vector3 spawnRot = transform.localEulerAngles;
        spawnRot.x += -20f;
        transform.localEulerAngles = spawnRot;

        //Delay
        await UniTask.Delay(TimeSpan.FromSeconds(0.75f));

        //Play Anim
        transform.DOLocalMove(Vector3.zero, 0.6f);
        transform.DOLocalRotate(Vector3.zero, 1.1f);

        await UniTask.Delay(TimeSpan.FromSeconds(1.1f));

        lookActive = true;
    }

    private void Look()
    {
        if (!lookActive || isReload)
            return;

        if (movementSystem.isMove)
        {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(weaponAnimator.runAnimRot), weaponAnimator.runRotSpeed * Time.deltaTime);
            transform.localPosition = Vector3.Slerp(transform.localPosition, weaponAnimator.runAnimPos, weaponAnimator.runPosSpeed * Time.deltaTime);
            return;
        }


        Vector3 lookPos = lookTarget.position - transform.position;
        Quaternion look = Quaternion.LookRotation(lookPos);

        transform.rotation = Quaternion.Slerp(transform.rotation, look, lookSpeed * Time.deltaTime);
        transform.localPosition = Vector3.Slerp(transform.localPosition, Vector3.zero, weaponAnimator.runPosSpeed * Time.deltaTime);
    }

    public void Shoot()
    {
        if (movementSystem.isMove)
            return;

        if (isReload)
            return;

        BulletCount--;
        shootAnimActive = true;

        FXManager.PlayParticle(type: FXType.ShootRed, _parent: bulletSpawnPos, pos: Vector3.zero, desTime: 2f).Forget();
        FXManager.PlayParticle(type: FXType.ShootRed1, _parent: bulletSpawnPos, pos: Vector3.zero, desTime: 2f).Forget();
        if (Physics.Raycast(bulletSpawnPos.position, bulletSpawnPos.forward, out RaycastHit hit, 1000, collableLayers))
        {
            //Object Type
            ObjectTypeHandler typeHandler = hit.collider.GetComponent<ObjectTypeHandler>();
            ObjectType type;
            if (typeHandler)
                type = typeHandler.type;
            else
                type = ObjectType.Ground;


            //FX Spawn
            Vector3 pos = hit.point;
            FXManager.PlayParticle(type: FXType.BulletExplosionGround, pos: pos, desTime: 2f).Forget();


            //Type Control
            switch (type)
            {
                case ObjectType.Null:
                    break;
                case ObjectType.Ground:
                    break;
                case ObjectType.Enemy or ObjectType.ArmEnemy:
                    Enemy enemy = hit.collider.GetComponent<Enemy>();
                    RagdollAnimatorController animatorController = enemy.animatorController;

                    enemy.TakeHit(weapon: this, pos: pos);
                    enemy.PushRagdoll(pos: pos, forceMultiplier: 9);

                    break;
            }
        }
        weaponAnimator.ActiveAllAnim();
    }

    public void SetActiveReload(int active = 1)
    {
        isReload = active == 1;

        if (!isReload)
            animator.enabled = false;
    }

    public void Reload()
    {
        if (isReload)
            return;

        bulletCount = bulletCapacity;
        transform.DOLocalRotate(Vector3.zero, 0.25f);
        weaponAnimator.Reload();
        weaponSlotUI.UpdateAmmountText();
    }

}
