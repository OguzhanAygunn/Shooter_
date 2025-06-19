using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraParentController : MonoBehaviour
{
    public static CameraParentController instance;

    [Title("Run Anim")]
    [SerializeField] bool runAnimActive;
    [SerializeField] bool runAnimUp;
    [SerializeField] Vector3 runUpPos;
    [SerializeField] Vector3 runDownPos;
    [SerializeField] float speed;

    [Space(6)]

    [Title("Gun Poses")]
    [SerializeField] Transform camPos;
    [SerializeField] HandWeaponManager rightHand;
    [SerializeField] List<GunPosInfo> gunPoses;
    [SerializeField] float gunPosAnimSpeed;
    Vector3 pos;
    Transform aim;
    Vector3 offset;
    Vector3 targetOffset;
    private MovementSystem movementSystem;
    private WeaponManager weaponManager;
    private void Awake()
    {
        instance = (!instance) ? this : instance;

        pos = transform.localPosition;
    }

    private void Start()
    {

        gunPoses.ForEach(gunPos => gunPos.Init());
        aim = AimController.instance.aim.transform;
        movementSystem = MovementSystem.instance;
        weaponManager = WeaponManager.instance;
    }



    private void Update()
    {
        Vector3 mousePos = aim.localPosition;
        offset = mousePos;
        offset.z = 0;

        offset.x /= (1920) / (4.5f);
        offset.y /= (1080) / (2);
        offset.y = Mathf.Clamp(offset.y, 0, 5);
        targetOffset = offset;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetOffset, 2 * Time.deltaTime);
        //rightHand.SetOffset(transform.localPosition);

        if (!movementSystem.isMove)
        {
            Vector3 targetRot = new Vector3(-transform.localPosition.y * 20, transform.localPosition.x * 10, 0);
            //camPos.transform.localRotation = Quaternion.RotateTowards(camPos.transform.localRotation, Quaternion.Euler(targetRot), 20f*Time.deltaTime);
        }


        Vector3 targetPos = runAnimUp ? runUpPos : runDownPos;
        targetPos = runAnimActive && !weaponManager.weapon.isReload ? targetPos : Vector3.zero;
        pos = Vector3.MoveTowards(pos, targetPos, speed * Time.deltaTime);

        gunPoses.ForEach(gunPos => gunPos.TransfromUpdate(toTarget: runAnimActive ? runAnimUp : false));

        if (runAnimActive && pos == targetPos)
            runAnimUp = !runAnimUp;
    }


    public void SetActiveAnim(bool active)
    {
        runAnimActive = active;
    }

}


[System.Serializable]
public class GunPosInfo
{
    [HideInInspector] public CameraParentController controller;
    [Title("Main")]
    public string id;
    public Transform gunPos;

    [Space(6)]

    [Title("Targets")]
    public Vector3 targetPos;
    public Vector3 targetRot;

    [Space(6)]

    [Title("Defaults")]
    public Vector3 defaultPos;
    public Vector3 defaultRot;

    [Space(6)]

    [Title("Speeds")]
    public float posSpeed;
    public float rotSpeed;
    private WeaponManager weaponManager;

    public void Init()
    {
        controller = CameraParentController.instance;
        weaponManager = WeaponManager.instance;

        defaultPos = gunPos.localPosition;
        defaultRot = Vector3.zero;

        targetPos += defaultPos;
    }


    public void TransfromUpdate(bool toTarget)
    {
        if (weaponManager.weapon.isReload)
            return;


        Vector3 pos = toTarget ? targetPos : defaultPos;
        Vector3 rot = toTarget ? targetRot : defaultRot;

        gunPos.localPosition = Vector3.Lerp(gunPos.localPosition, pos, posSpeed * Time.deltaTime);
        gunPos.localEulerAngles = Vector3.Lerp(gunPos.localEulerAngles, rot, rotSpeed * Time.deltaTime);
    }
}
