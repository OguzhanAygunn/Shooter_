using Cinemachine;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager instance;
    [Title("Points")]
    [SerializeField] List<CamPointInfo> points;

    [Space(6)]

    [Title("Cameras")]
    [SerializeField] public Camera mainCam;
    [SerializeField] public Camera aimCam;

    [Space(6)]

    [Title("Effects")]
    public List<CameraShakeInfo> cameraShakes;
    public List<FOVAnimInfo> fovAnims;

    [Space(6)]

    [Title("Others")]
    [SerializeField] Transform camParent;


    private Transform cameraTrs;
    private Camera cam;
    private void Awake()
    {
        instance = (!instance) ? this : instance;

        cam = Camera.main;
        cameraTrs = cam.transform;

        points.ForEach(p => { p.Init(); });
        fovAnims.ForEach(anim => anim.AssignCamera(mainCam));
        fovAnims.ForEach(anim => anim.Init());
    }

    private void LateUpdate()
    {
        aimCam.transform.position = cameraTrs.position;
        aimCam.transform.rotation = cameraTrs.rotation;
    }





    public CamPoint GetCamPoint(int index = 0)
    {
        return points.Find(p => p.index == index).point;
    }


    [Button(size: ButtonSizes.Large)]
    public void ToPoint(int pointIndex)
    {
        //Info Control
        CamPointInfo info = GetCamPointInfo(pointIndex);
        if (info == null)
            return;

        //Get Values
        Vector3 endPos = info.point.transform.position;
        Vector3 endRotate = info.point.transform.eulerAngles;
        float jumpPower = info.jumpPower;
        float jumpDuration = info.duration;
        float jumpDelay = info.delay;
        int jumpCount = info.jumpCount;
        AnimationCurve curve = CurveManager.GetCurve("Jump");

        //Jump
        cameraTrs.DOJump(endValue: endPos,
            jumpPower: jumpPower,
            numJumps: jumpCount,
            duration: jumpDuration).
            SetDelay(delay: jumpDelay).
            SetEase(animCurve: curve);

        //Look
        cameraTrs.DORotate(endRotate, 1f).SetEase(animCurve: curve);
    }

    public CamPointInfo GetCamPointInfo(int index)
    {
        return points.Find(p => p.index == index);
    }

    [Button(size: ButtonSizes.Large)]
    public static void Shake(string id)
    {
        instance.cameraShakes.Find(c => c.id == id).Shake(instance.camParent);
    }

    [Button(size: ButtonSizes.Large)]
    public static void PlayFovAnim(string id)
    {
        instance.fovAnims.Find(anim => anim.id == id).PlayAnim();
    }
}


[System.Serializable]
public class CamPointInfo
{
    [Title("Main")]
    public int index;
    public CamPoint point;

    [Space(6)]

    [Title("Anim")]
    public float jumpPower;
    public int jumpCount;
    public float duration;
    public float delay;
    public Ease ease;

    public void Init()
    {
        point.Init();
    }
}

[System.Serializable]
public class CameraShakeInfo
{
    public string id;
    public float duration;
    public float strength;
    public int vibrate;
    public float randomness;
    public bool snapping;
    public bool fadeOut;

    public void Shake(Transform shakeObj)
    {
        shakeObj.DOShakePosition(duration: duration, vibrato: vibrate, strength: strength, randomness: randomness, snapping: snapping, fadeOut: fadeOut);
    }
}

[System.Serializable]
public class FOVAnimInfo
{
    public string id;
    public float targetFOV;
    public float duration;
    private Camera cam;
    private float defaultFOV;
    public void AssignCamera(Camera cam)
    {
        this.cam = cam;
    }

    public void Init()
    {
        defaultFOV = cam.fieldOfView;
    }

    public void PlayAnim()
    {
        cam.DOFieldOfView(targetFOV, duration).OnComplete( () =>
        {
            cam.DOFieldOfView(defaultFOV, duration / 2).SetDelay(0.1f);
        });
    }
}