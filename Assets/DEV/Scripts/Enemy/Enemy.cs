using Cysharp.Threading.Tasks;
using FIMSpace.FProceduralAnimation;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Title("Main")]
    public bool isAlive;
    public bool isActive;
    [Space(6)]

    [Title("Other Poses")]
    public Transform healthBarPos;
    public Transform emojiPos;

    [Space(6)]

    [Title("Components")]
    [SerializeField] BoxCollider myCollider;
    [SerializeField] Rigidbody myRigid;
    public Animator animator;
    public RagdollAnimator2 ragdollAnimator;
    public EnemyMovement movementController;
    public RagdollAnimatorController animatorController;
    public EnemyCollision collisionController;
    public EnemyHealth enemyHealth;
    public EnemyEffectController effectController;
    public HighlightEffectController highlightController;
    public HealthBarUI healthBarUI;
    public HealthBarPosController healthBarPosController;
    public OutlineController outlineController;
    public EventHandler eventHandler;
    [HideInInspector] public RigidManipulator.RigidManipulatorFakeRigid fakeRigid;
    private ObjectTypeHandler typeHandler;
    private void Start()
    {
        typeHandler = GetComponent<ObjectTypeHandler>();
        fakeRigid = GetComponent<RigidManipulator.RigidManipulatorFakeRigid>();
        HealthBarManager.instance.CreateHealthBars(this);
    }

    public void TakeHit(Weapon weapon, Vector3 pos)
    {
        if (!isAlive)
            return;

        int damage = weapon.Damage;
        enemyHealth.DecreaseHealth(damage);

        effectController.SmallSizeEffect().Forget();

        if (enemyHealth.health <= 0)
        {
            Death(pos: pos).Forget();
        }


        healthBarUI.TakeHit();
    }

    public async UniTaskVoid Death(Vector3 pos)
    {
        EventManager.KillCountIncrease();
        fakeRigid?.SetActive(active: false);
        ragdollAnimator.enabled = true;
        myCollider.enabled = false;
        myRigid.isKinematic = true;


        Vector3 bloodPos = transform.position + Vector3.up * 2.5f + transform.TransformDirection(Vector3.forward * 8);


        /*if (typeHandler.type == ObjectType.ArmEnemy)
        {

        }*/


        ArmedEnemy armedEnemy = GetComponent<ArmedEnemy>();

        animator.enabled = true;
        bloodPos = pos + transform.TransformDirection(Vector3.forward * 8);
        armedEnemy.GetWeapon().Free(active: true);

        FXManager.PlayParticle(type: FXType.BloodEffectA, pos: bloodPos, rot: Vector3.right * 180, desTime: 2f).Forget();

        TimeManager.PlayAnim("1");
        CameraManager.Shake("Take Hit");


        highlightController.PlayEffect("White");

        Vector3 emojiSpawnPos = emojiPos.position;
        EmojiManager.SpawnEmojiEffect(pos: emojiSpawnPos, spriteID: "Emoji - Skull");

        await UniTask.Delay(TimeSpan.FromSeconds(0.15f));


        Vector3 impactDirection = (fakeRigid.transform.TransformDirection(Vector3.forward));
        var rigidbody = ragdollAnimator.User_GetNearestRagdollRigidbodyToPosition(transform.TransformPoint(new Vector3(0f, 1.45f, 0.2f)), true, ERagdollChainType.Core);
        ragdollAnimator.User_SwitchFallState();

        float chargeAmount = 1;
        float punchPower = 30;

        float chargeMul = 1f + chargeAmount * 0.4f;
        ragdollAnimator.User_AddAllBonesImpact(impactDirection * (punchPower * 0.5f * chargeMul), 0.05f, ForceMode.VelocityChange);
        ragdollAnimator.User_AddRigidbodyImpact(rigidbody, impactDirection * (punchPower * 1.5f * chargeMul), 0.0f, ForceMode.VelocityChange);


    }

    public void PushRagdoll(Vector3 pos, float forceMultiplier, bool force = false)
    {
        if (!force)
        {
            if (!isAlive)
                return;
        }

        animatorController.AddForce(pos, forceMultiplier);
    }

    public void SetActiveAlive(bool active)
    {
        isAlive = active;
        healthBarUI.DeActive();
        if (typeHandler.type != ObjectType.ArmEnemy)
            animator.enabled = false;
    }

}
