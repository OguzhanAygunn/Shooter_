using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FXType { Null, ShootRed, BulletExplosionGround, BloodEffectA, ShootRed1, EnemyShootRed }
public class FXManager : MonoBehaviour
{
    public static FXManager instance;

    public List<FXPoolInfo> poolInfos;

    private Transform fxParent;

    public List<FX> fxs = new List<FX>();
    private void Awake()
    {
        instance = (!instance) ? this : instance;

        fxParent = new GameObject("FX Parent").transform;
    }

    private void Start()
    {
        Pool();
    }


    private void Pool()
    {
        foreach (FXPoolInfo info in poolInfos)
        {
            int index = 0;

            while (index < info.count)
            {
                // Create Effect;
                GameObject fxObj = Instantiate(info.fxPrefab, fxParent);
                fxObj.gameObject.SetActive(false);

                // Create FX
                FX fx = new FX();
                fx.particleObj = fxObj;
                fx.type = info.type;

                //Add
                fxs.Add(fx);

                //Increase Index
                index++;
            }
        }
    }

    public async static UniTaskVoid PlayParticle(FXType type, Vector3 pos, float desTime)
    {

        GameObject fxObj = GetParticle(type: type);

        if (!fxObj)
            return;

        ParticleSystem fx = fxObj.GetComponent<ParticleSystem>();
        fx.transform.parent = instance.fxParent;
        fx.gameObject.SetActive(true);
        fx.Clear();
        fx.transform.position = pos;

        await UniTask.Delay(TimeSpan.FromSeconds(desTime));

        fx.gameObject.SetActive(false);
    }

    public async static UniTaskVoid PlayParticle(FXType type, Vector3 pos, Vector3 rot, float desTime)
    {

        GameObject fxObj = GetParticle(type: type);

        if (!fxObj)
            return;

        ParticleSystem fx = fxObj.GetComponent<ParticleSystem>();
        fx.transform.parent = instance.fxParent;
        fx.gameObject.SetActive(true);
        fx.Clear();
        fx.transform.position = pos;
        fx.transform.eulerAngles = rot;

        await UniTask.Delay(TimeSpan.FromSeconds(desTime));

        fx.gameObject.SetActive(false);
    }

    public async static UniTaskVoid PlayParticle(FXType type, Vector3 pos, Transform _parent, float desTime)
    {

        GameObject fxObj = GetParticle(type: type);



        if (!fxObj)
            return;

        ParticleSystem fx = fxObj.GetComponent<ParticleSystem>();
        fx.transform.parent = instance.fxParent;
        fx.gameObject.SetActive(true);
        fx.Clear();
        fx.transform.parent = _parent;
        fx.transform.localPosition = pos;

        await UniTask.Delay(TimeSpan.FromSeconds(desTime));

        fx.gameObject.SetActive(false);

    }

    public static GameObject GetParticle(FXType type)
    {
        return instance.fxs.Find(x => !x.particleObj.activeInHierarchy && x.type == type).particleObj;
    }
}


[System.Serializable]
public class FXPoolInfo
{
    public FXType type;
    public GameObject fxPrefab;
    public int count;
}

[System.Serializable]
public class FX
{
    public FXType type;
    public GameObject particleObj;
}
