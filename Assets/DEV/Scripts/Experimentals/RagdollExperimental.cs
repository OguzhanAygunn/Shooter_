using FIMSpace.FProceduralAnimation;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RagdollExperimental : MonoBehaviour
{
    [SerializeField] RagdollAnimator2 animator;
    [SerializeField] Transform target;
    // Start is called before the first frame update
    void Start()
    {
    }

    [Button(size: ButtonSizes.Large)]
    public void AddForce()
    {
        List<RagdollChainBone> bones = new List<RagdollChainBone>();
        bones = animator.User_GetAllRagdollDummyBoneSetups();
        RagdollChainBone bone = animator.User_GetBoneSetupByBoneID(ERagdollBoneID.Spine);
        bone = bones.OrderBy(b => Vector3.Distance(b.Posing.AnimatorPosition, target.position)).ToList()[0];
        animator.User_AddBoneImpact(bone, Vector3.forward * 10, 0.1f, ForceMode.VelocityChange);
    }

    // Update is called once per frame
    void Update()
    {
        RagdollChainBone bone = animator.User_GetBoneSetupByBoneID(ERagdollBoneID.Spine);

        print(animator.User_GetNearestPhysicalTransformBoneToPosition(target.position));
    }
}
