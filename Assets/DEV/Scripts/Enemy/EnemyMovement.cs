using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum MovementType { Transform, Agent }
public class EnemyMovement : MonoBehaviour
{
    [Title("Movement System")]
    [SerializeField] MovementType movementType;
    [SerializeField] bool moveActive;
    public bool MoveActive
    {
        get { return moveActive; }
        set
        {
            moveActive = value;
        }
    }
    [SerializeField] NavMeshAgent agent;

    [Space(6)]

    [Title("Target")]
    [SerializeField] Transform target;
    [SerializeField] float maxTargetDistance;
    [SerializeField] Transform agentTarget;
    [SerializeField] EnemyPosHandler targetPosHandler;
    [Space(6)]

    [Title("Speeds")]
    [SerializeField] bool speedUp;
    [SerializeField] float speed;
    [SerializeField] float startSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float acceleration;
    //[ShowIf("@movementType == MovementType.Transform")]
    [Space(6)]

    [Title("Others")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] bool groundOffsetActive;
    [ShowIf("@groundOffsetActive == true")][SerializeField] Vector3 groundOffset;

    [Space(6)]


    [Title("Components")]
    [SerializeField] Enemy enemy;
    [SerializeField] Rigidbody rb;
    [SerializeField] RagdollAnimatorController raController;

    //Defaults
    private Vector3 defaultPos;
    private Vector3 defaultScale;
    private Vector3 defaultRotation;
    private float defaultYpos;
    [SerializeField] private ArmedEnemy armedSc;


    private void Awake()
    {
        Init();
    }

    private void Start()
    {
        target = PlayerController.instance.transform;
        agentTarget = PlayerController.instance.transform;
    }

    public void Init()
    {
        defaultPos = transform.position;
        defaultScale = transform.localScale;
        defaultRotation = transform.eulerAngles;
        defaultYpos = defaultPos.y;
    }

    private void FixedUpdate()
    {
        MoveUpdate();
    }

    private void MoveUpdate()
    {
        if (!moveActive)
            return;

        switch (movementType)
        {
            case MovementType.Transform:
                NormalMovement();
                SpeedController();
                break;
            case MovementType.Agent:
                AgentMovement();
                SpeedController();
                break;
        }
    }

    private Vector3 GetGroundPos()
    {
        RaycastHit hit;
        Vector3 pos = Vector3.zero;

        Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1000, groundLayer);
        if (hit.collider != null)
        {
            pos = hit.point;
        }

        pos.y += groundOffset.y;
        return pos;
    }

    public bool GroundCheck()
    {
        return Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 1000, groundLayer);
    }

    private void NormalMovement()
    {
        //Values
        Vector3 pos = transform.position;
        Vector3 targetPos = target.position;

        //Update Poses
        if (groundOffsetActive && GroundCheck())
            targetPos.y = GetGroundPos().y;
        else
            targetPos.y = pos.y;


        //Pos Update V2 && distance
        pos = Vector3.MoveTowards(pos, targetPos, speed * Time.deltaTime);
        float distance = Vector3.Distance(pos, targetPos);

        //Distance Control And Move
        if (distance > maxTargetDistance)
            transform.position = pos;
    }

    private void AgentMovement()
    {
        if (!agentTarget)
            return;


        Vector3 targetPos = Vector3.MoveTowards(transform.position, agentTarget.position, speed * Time.deltaTime);

        if (!targetPosHandler)
        {
            targetPos.y = transform.position.y;
        }


        float distance = Vector3.Distance(transform.position, agentTarget.position);
        if (distance < maxTargetDistance && !targetPosHandler)
        {
            speed = 0;
        }
        else
        {
            transform.position = targetPos;
        }


        if (distance < 0.85f)
        {

            if (targetPosHandler)
                armedSc.SetPosType(targetPosHandler.armedEnemyAnimType);

            AssignAgentMoveEnemyPos(targetPosHandler.nextEnemyHandler);

        }

    }

    public void AssignAgentTarget(Transform newAgentTarget)
    {
        agentTarget = newAgentTarget;
    }

    private void SpeedController()
    {
        float targetSpeed = speedUp ? maxSpeed : 0;
        speed = Mathf.MoveTowards(speed, targetSpeed, acceleration * Time.deltaTime);
    }

    [Button(size: ButtonSizes.Large)]
    public void AssignAgentMoveEnemyPos(EnemyPosHandler posHandler)
    {
        if (posHandler)
        {
            
            targetPosHandler = posHandler;
            agentTarget = targetPosHandler?.transform;
            moveActive = true;
            speedUp = true;

        }
        else
        {

            if (targetPosHandler)
                armedSc.GetWeapon().SetAttackable(active: targetPosHandler.attackActive, delay: targetPosHandler.attackActiveDelay).Forget();

            targetPosHandler = null;
            agentTarget = null;
            moveActive = false;
            MoveActive = true;
            //armedSc.GetWeapon().SetAttackable(active: false, delay: 0).Forget();
            //armedSc.SetPosType(ArmedEnemyPosType.CrouchFire);
        }

    }

}
