using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarManager : MonoBehaviour
{
    public static HealthBarManager instance;

    [SerializeField] GameObject healthBarPrefab;


    [SerializeField] List<HealthBarUI> healthBars = new List<HealthBarUI>();
    [SerializeField] Transform barsParent;

    private Camera cam;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    private void Start()
    {
        
    }


    public void CreateHealthBars(Enemy enemy)
    {
        if (!cam)
            cam = CameraManager.instance.aimCam;

        HealthBarUI newBar = Instantiate(healthBarPrefab).GetComponent<HealthBarUI>();

        newBar.transform.parent = barsParent;
        newBar.AssignCam(newCam: cam);
        newBar.AssignTarget(newTarget: enemy.healthBarPos);
        newBar.AssignHealth(newHealth: enemy.enemyHealth);

        enemy.healthBarUI = newBar;
    }
}
