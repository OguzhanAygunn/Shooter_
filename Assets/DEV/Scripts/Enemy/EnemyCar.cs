using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyCarState { Ready, Move, Idle, Dead}
public class EnemyCar : MonoBehaviour
{
    [Title("Main")]
    public EnemyCarState state;
    [SerializeField] Transform meshTrs;
    [SerializeField] List<Transform> nullParentPoses; 

    [Space(6)]

    [Title("Anim")]
    [SerializeField] Transform movePos;
    [SerializeField] List<EnemyCarDoorInfo> doors;
    [SerializeField] List<Transform> wheels;
    [SerializeField] List<EnemyCarWheele> wheelScs;

    [Space(6)]

    [SerializeField] List<EnemyCarEnemyInfo> enemies;

    private void Awake()
    {
        movePos.SetParent(null);
        //enemyPosesParent.SetParent(null);
        nullParentPoses.ForEach(p => p.SetParent(null));


        PlayAnim(mainDelay: 1.2f).Forget();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
        }
    }

    [Button(size: ButtonSizes.Large)]
    public async UniTaskVoid PlayAnim(float mainDelay = 0)
    {

        if(mainDelay > 0)
            await UniTask.Delay(TimeSpan.FromSeconds(mainDelay));

        transform.DOMove(movePos.position, 2f);
        wheelScs.ForEach(wheel => wheel.SetActiveRotate(active: true));
        
        transform.DODynamicLookAt(movePos.position, 0.25f);

        await UniTask.Delay(TimeSpan.FromSeconds(0.75f));

        meshTrs.DOLocalRotate(Vector3.forward * 20, 0.15f);

        meshTrs.DOLocalJump(Vector3.zero, 1.75f, 1, 0.5f).SetEase(Ease.Linear);
        transform.DORotate(Vector3.up * 270, 1f, RotateMode.LocalAxisAdd);

        wheels.ForEach(wheel => wheel.transform.DOLocalRotate(Vector3.up * 25, 0.5f));

        await UniTask.Delay(TimeSpan.FromSeconds(0.25f));
        wheelScs.ForEach(wheel => wheel.SetActiveRotate(active: false));
        meshTrs.DOLocalRotate(Vector3.zero, 0.25f).SetEase(Ease.Linear);
        await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

        

        MeshScaleEffect();

        enemies.ForEach(enemy => enemy.enemy.fakeRigid.FollowActive = true);
        await UniTask.Delay(TimeSpan.FromSeconds(0.7f));
        doors.ForEach(door => door.PlayAnim().Forget());




        await UniTask.Delay(TimeSpan.FromSeconds(0.2f));
        enemies.ForEach(enemy => enemy.EnemyToTarget());

    }


    private void MeshScaleEffect()
    {
        Vector3 startScale = meshTrs.localScale * 0.9f;
        Vector3 endScale = meshTrs.localScale;

        meshTrs.DOScale(startScale,0.05f).SetEase(Ease.Linear).OnComplete( () =>
        {
            meshTrs.DOScale(endScale, 0.5f).SetEase(Ease.OutElastic);
        });
    }


}


[System.Serializable]
public class EnemyCarDoorInfo
{
    public Transform doorTrs;
    public Vector3 targetRot;
    public float openDuration;
    public float openDelay;
    public Ease openEase;
    public async UniTaskVoid PlayAnim()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(openDelay));

        NPCManager.CityPersonsRunActive();
        doorTrs.DOLocalRotate(targetRot, openDuration).SetEase(openEase);
    }
}

[System.Serializable]
public class EnemyCarEnemyInfo
{
    public ArmedEnemy enemy;
    public EnemyPosHandler posHandler;

    public void EnemyToTarget()
    {
        enemy.movementController.AssignAgentMoveEnemyPos(posHandler);
    }
}