using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CityPersonMoveType { Idle, Walk, Run }
public class CityPerson : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool active = true;
    [SerializeField] CityPersonMoveType moveType = CityPersonMoveType.Idle;

    [Space(6)]

    [Title("Body")]
    [SerializeField] SkinnedMeshRenderer currentRenderer;
    [SerializeField] List<SkinnedMeshRenderer> bodyRenderers;
    [SerializeField][Range(0, 8)] int bodyIndex = 0;

    [Space(6)]

    [Title("Movement")]
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator animator;
    [SerializeField] MovePosesInfo movePosInfo;
    [Space(4)]
    [SerializeField] float targetBlend;
    [SerializeField] float blendSpeed;

    private void OnValidate()
    {
        UpdateMesh();
    }

    private void Awake()
    {
    }

    private void Start()
    {
        movePosInfo.Init();
        movePosInfo.AssignCharacterPos(transform);
        Init().Forget();
    }

    private void Update()
    {
        AnimatorController();
        MovementController();
        movePosInfo.PosesControl();
    }

    [Button(size: ButtonSizes.Large)]
    public void UpdateRandomMeshIndex()
    {
        bodyIndex = UnityEngine.Random.Range(0, bodyRenderers.Count);
        UpdateMesh();
    }

    public void UpdateMesh()
    {
        if (bodyRenderers.Count == 0)
            return;

        bodyRenderers.ForEach(r => { r.gameObject.SetActive(false); });

        currentRenderer = bodyRenderers[bodyIndex];
        currentRenderer.gameObject.SetActive(true);
    }

    private void MovementController()
    {
        if (moveType == CityPersonMoveType.Idle)
            return;

        Vector3 pos = moveType == CityPersonMoveType.Walk ? movePosInfo.walkPos.position : movePosInfo.runPos.position;
        agent.SetDestination(pos);
    }

    private void AnimatorController()
    {
        float blend = animator.GetFloat("Blend");
        blend = Mathf.MoveTowards(blend, targetBlend, blendSpeed * Time.deltaTime);
        animator.SetFloat("Blend", blend);
    }


    private async UniTaskVoid Init()
    {
        float delay1 = UnityEngine.Random.Range(0, 1f);
        await UniTask.Delay(TimeSpan.FromSeconds(delay1));
        SetMoveType(CityPersonMoveType.Walk);
    }

    public void SetMoveType(CityPersonMoveType moveType)
    {
        this.moveType = moveType;

        switch (moveType)
        {
            case CityPersonMoveType.Walk:
                targetBlend = 1;
                agent.speed = movePosInfo.walkSpeed;
                break;
            case CityPersonMoveType.Run:
                targetBlend = 2;
                agent.speed = movePosInfo.runSpeed;
                break;
            default:
                break;
        }
    }

    public CityPersonMoveType GetMoveType() { return moveType; }

}

[System.Serializable]
public class MovePosesInfo
{
    [Title("Main")]
    [SerializeField] Transform characterPos;
    [SerializeField] CityPerson person;
    [SerializeField] readonly float maxDistance = 0.83f;

    [Space(6)]

    [Title("Walk")]
    public int walkPosIndex = 0;
    public float walkSpeed;
    public Transform walkPosesParent;
    public List<Transform> walkPoses;
    [HideInInspector] public Transform walkPos;

    [Space(6)]

    [Title("Run")]
    public int runPosIndex = 0;
    public float runSpeed;
    public Transform runPosesParent;
    public List<Transform> runPoses;
    [HideInInspector] public Transform runPos;


    public void Init()
    {

        walkPoses.ForEach(p => p.GetComponent<MeshRenderer>().enabled = false);
        runPoses.ForEach(p => p.GetComponent<MeshRenderer>().enabled = false);

        walkPos = walkPoses[walkPosIndex];
        runPos = runPoses[runPosIndex];

        walkPosesParent.parent = null;
        runPosesParent.parent = null;

    }

    public void AssignCharacterPos(Transform _trs)
    {
        characterPos = _trs;
        person = _trs.GetComponent<CityPerson>();
    }

    public void PosesControl()
    {
        Vector3 targetPos = person.GetMoveType() == CityPersonMoveType.Walk ? walkPos.position : runPos.position;
        float distance = Vector3.Distance(targetPos, characterPos.position);

        if (distance > maxDistance)
            return;

        if (person.GetMoveType() == CityPersonMoveType.Walk)
        {
            walkPosIndex++;
            walkPosIndex = (walkPosIndex >= walkPoses.Count) ? 0 : walkPosIndex;
            walkPos = walkPoses[walkPosIndex];
        }
        else
        {
            return;
            runPosIndex++;
            runPosIndex = (runPosIndex >= runPoses.Count) ? 0 : runPosIndex;
            runPos = runPoses[runPosIndex];
        }
    }

}
