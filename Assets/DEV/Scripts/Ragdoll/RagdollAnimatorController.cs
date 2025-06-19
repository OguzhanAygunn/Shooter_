using FIMSpace.FProceduralAnimation;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RagdollAnimatorController : MonoBehaviour
{
    public bool active;
    [SerializeField] RagdollAnimator2 animator;
    private List<RagdollChainBone> bones = new List<RagdollChainBone>();
    ObjectTypeHandler typeHandler;

    // Start is called before the first frame update
    void Start()
    {
        typeHandler = GetComponent<ObjectTypeHandler>();
        bones = animator.User_GetAllRagdollDummyBoneSetups();
    }

    [Button(size: ButtonSizes.Large)]
    public void AddForce(Vector3 pos,float forceMultiplier)
    {
        if (!active)
            return;

        RagdollChainBone bone = bones.OrderBy(b => Vector3.Distance(b.Posing.AnimatorPosition, pos)).ToList().First();

        Vector3 force = bone.Posing.AnimatorPosition - pos;
        force *= forceMultiplier;

        animator.User_AddBoneImpact(bone, force, 0.1f, ForceMode.VelocityChange);
    }
}
