using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public enum DestroyType { Null, Size, Alpha }

public class DestroyController : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool active;
    [SerializeField] DestroyType destroyType;
    [SerializeField] readonly string des = "nothing -_-";
    [Space(6)]

    [Title("Size")]
    [ShowIf("@this.destroyType == DestroyType.Size")][SerializeField] Transform SIZE_destroy_object;
    [ShowIf("@this.destroyType == DestroyType.Size")][SerializeField] float SIZE_destroy_Duration;
    [ShowIf("@this.destroyType == DestroyType.Size")][SerializeField] float SIZE_destroy_Delay;
    [ShowIf("@this.destroyType == DestroyType.Size")][SerializeField] Ease SIZE_destroy_Ease;

    [Title("Alpha")]
    [ShowIf("@this.destroyType == DestroyType.Alpha")][SerializeField] float ALPHA_destroy_Duration;
    [ShowIf("@this.destroyType == DestroyType.Alpha")][SerializeField] float ALPHA_destroy_Delay;
    [ShowIf("@this.destroyType == DestroyType.Alpha")][SerializeField] List<Renderer> ALPHA_destroy_renderers;
    [ShowIf("@this.destroyType == DestroyType.Alpha")][SerializeField] GameObject ALPHA_destroy_Object;


    private void Start()
    {

    }


    [Button(size: ButtonSizes.Large)]
    public void PlayDestroy()
    {
        switch (destroyType)
        {
            case DestroyType.Size:
                SizeDestroy().Forget();
                break;
            case DestroyType.Alpha:
                AlphaDestroy().Forget();
                break;
            default:
                //nothing -_-
                break;
        }
    }


    private async UniTaskVoid SizeDestroy()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(SIZE_destroy_Delay));

        StoppedPhysicsComponents();

        SIZE_destroy_object.DOScale(Vector3.zero, SIZE_destroy_Duration).OnComplete(() =>
        {
            SIZE_destroy_object.gameObject.SetActive(false);
        });
    }



    private async UniTaskVoid AlphaDestroy()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(ALPHA_destroy_Delay));

        StoppedPhysicsComponents();

        foreach (Renderer renderer in ALPHA_destroy_renderers)
        {
            MaterialExtensions.ToFadeMode(renderer.material);
            renderer.material.DOFade(0, ALPHA_destroy_Duration);
        }


        await UniTask.Delay(TimeSpan.FromSeconds(ALPHA_destroy_Duration + 0.1f));

        ALPHA_destroy_Object.SetActive(false);

    }


    private void StoppedPhysicsComponents()
    {
        Rigidbody rigid = GetComponent<Rigidbody>();
        Collider collider = GetComponent<Collider>();

        rigid.isKinematic = true;
        collider.enabled = false;
    }

}


public static class MaterialExtensions
{
    public static void ToOpaqueMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
        material.SetInt("_ZWrite", 1);
        material.DisableKeyword("_ALPHATEST_ON");
        material.DisableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = -1;
    }

    public static void ToFadeMode(this Material material)
    {
        material.SetOverrideTag("RenderType", "Transparent");
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
    }
}