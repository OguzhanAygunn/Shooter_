using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPartsExplosion : MonoBehaviour
{

    [Title("Body Parts")]
    [SerializeField] bool active;
    [SerializeField] List<Rigidbody> parts;

    [Space(6)]

    [Title("Destroy")]
    [SerializeField] bool afterDestroy;
    [SerializeField] float afterDestroyDelay;

    [Space(6)]

    [Title("Explosion")]
    [SerializeField] Vector3 explosionPosition;
    [SerializeField] float explosionForce;
    [SerializeField] float explosionRadius;
    [SerializeField] float upwardsModifier;
    [SerializeField] ForceMode mode;


    [Button(size: ButtonSizes.Large)]
    public async void Explosion()
    {
        Vector3 expPos = transform.position + explosionPosition;
        parts.ForEach(part => part.isKinematic = false);

        parts.ForEach(bodyPart =>
        {
            bodyPart.AddExplosionForce(explosionForce: explosionForce, explosionPosition: expPos, explosionRadius: explosionRadius, upwardsModifier: upwardsModifier, mode: mode);
        });

        await UniTask.Delay(TimeSpan.FromSeconds(afterDestroyDelay));

        if (!afterDestroy)
            return;

        parts.ForEach(part => {

            part.isKinematic = true;
            float duration = UnityEngine.Random.Range(0.6f, 1.2f);
            part.transform.DOScale(Vector3.zero, duration).OnComplete( () =>
            {
                Destroy(part.gameObject);
            });
        
        });

    }

}


[System.Serializable]
public class MeshPartInfo
{
    public Rigidbody body;
    public MeshCollider collider;
}
