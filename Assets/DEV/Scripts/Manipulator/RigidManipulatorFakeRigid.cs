using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum RigidManipulatorFollowType { Fix, MoveTo, Lerp }
public enum RigidManipulatorOffsetWorldType { Global, Local }

namespace RigidManipulator
{
    public class RigidManipulatorFakeRigid : MonoBehaviour
    {
        [Title("Main")]
        [SerializeField] bool active;
        [SerializeField] Transform bodyPart;
        [SerializeField] public Transform fakeBody;

        [Space(6)]

        [Title("Main Manipule")]
        [SerializeField] bool mainManipuleActive;
        [SerializeField] Vector3 mainPosOffset;
        [SerializeField] Vector3 mainRotOffset;

        [Space(6)]

        [Title("Follow")]
        [SerializeField] bool followActive;
        public bool FollowActive
        {
            get
            {
                return followActive;
            }
            set
            {
                followActive = value;
            }
        }

        [SerializeField] RigidManipulatorFollowType followType;
        [SerializeField] RigidManipulatorOffsetWorldType offsetWorldType;
        [Space(4)]
        [SerializeField] Transform target;
        [SerializeField] Vector3 lookPosOffset;
        [SerializeField] Vector3 lookRotOffset;
        [SerializeField] Vector3 lookDirection;
        [Space(4)]
        [ShowIf("@followType == RigidManipulatorFollowType.MoveTo")][SerializeField] float moveToSpeed;
        [ShowIf("@followType == RigidManipulatorFollowType.Lerp")][SerializeField] float lerpSpeed;

        [Space(6)]

        [Title("Transform Infos")]
        [SerializeField] Vector3Info currentRotVectorInfo;
        [SerializeField] Vector3Info currentPosVectorInfo;

        [Space(12)]

        [SerializeField] List<Vector3Info> rots;
        [SerializeField] List<Vector3Info> poses;

        private void Awake()
        {
            fakeBody = new GameObject(bodyPart.name + "(Fake Body)").transform;


            fakeBody.transform.parent = bodyPart.parent;
            fakeBody.transform.localPosition = Vector3.zero;
            fakeBody.transform.localEulerAngles = Vector3.zero;
            fakeBody.transform.localScale = Vector3.one;

            bodyPart.parent = fakeBody;

            Init();
        }

        private void Init()
        {
            //Create
            Vector3Info defaultRotInfo = new Vector3Info
            {
                id = "Default Rot",
                vector = lookRotOffset,
            };

            Vector3Info defaultPosInfo = new Vector3Info
            {
                id = "Default Pos",
                vector = lookPosOffset,
            };

            //Add
            rots.Add(defaultRotInfo);
            poses.Add(defaultPosInfo);

            //Assign
            currentPosVectorInfo = defaultPosInfo;
            currentRotVectorInfo = defaultRotInfo;
        }

        public void UpdateCurrentRotVector(string id)
        {
            currentRotVectorInfo = rots.Find(rot => rot.id == id);
            lookRotOffset = currentRotVectorInfo.vector;
        }

        private void LateUpdate()
        {
            if (!active)
                return;

            Look();

            MainManipule();
        }

        public void SetActive(bool active)
        {
            this.active = active;
        }

        private void MainManipule()
        {
            if (!mainManipuleActive)
                return;


            fakeBody.localEulerAngles = mainRotOffset;
            fakeBody.localPosition = mainPosOffset;
        }

        private void Look()
        {
            if (!followActive)
                return;


            switch (followType)
            {
                case RigidManipulatorFollowType.Fix:
                    FixLook();
                    break;
                case RigidManipulatorFollowType.MoveTo:
                    break;
                case RigidManipulatorFollowType.Lerp:
                    LerpLook();
                    break;
                default:
                    break;
            }


            UseOffset();
        }

        private void FixLook()
        {
            fakeBody.transform.LookAt(target.position, transform.up);
        }

        private void ToWardLook()
        {

        }

        public void SetActiveFollow(bool active)
        {
            FollowActive = active;
        }
        private void LerpLook()
        {
            Vector3 _offset = (fakeBody.transform.position - target.position);
            Quaternion q = Quaternion.LookRotation(_offset);
            //q += Quaternion.Euler(lookRotOffset.x, lookRotOffset.y, lookRotOffset.z);

            fakeBody.transform.rotation = Quaternion.Slerp(fakeBody.transform.rotation, q, lerpSpeed * Time.deltaTime);
        }

        public void UseOffset()
        {
            if (offsetWorldType is RigidManipulatorOffsetWorldType.Global)
            {
                fakeBody.transform.Rotate(lookRotOffset, Space.World);
            }
            else if (offsetWorldType is RigidManipulatorOffsetWorldType.Local)
            {
                fakeBody.transform.Rotate(lookRotOffset, Space.Self);
            }
        }
    }
}


[System.Serializable]
public class Vector3Info
{
    public string id;
    public Vector3 vector;
}

