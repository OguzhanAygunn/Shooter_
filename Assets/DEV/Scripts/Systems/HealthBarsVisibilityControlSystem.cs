using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarsVisibilityControlSystem : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] Transform aimDirection;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] Enemy targetEnemy;
    private void Update()
    {
        VisibilityController();
    }

    private void VisibilityController()
    {
        RaycastHit hit;

        if (Physics.Raycast(aimDirection.position, aimDirection.forward, out hit, 1000, enemyLayer))
        {
            Enemy newEnemy = hit.collider.GetComponent<Enemy>();

            if(targetEnemy)
            {
                if(targetEnemy != newEnemy)
                {
                    targetEnemy.healthBarUI.SetVisibility(active: false);
                    targetEnemy.outlineController?.SetActive(active: false);
                }
            }

            targetEnemy = newEnemy;
            targetEnemy.healthBarUI.SetVisibility(active: true);
            targetEnemy.outlineController?.SetActive(active: true);
        }
        else
        {
            if (targetEnemy)
            {
                targetEnemy.healthBarUI.SetVisibility(active: false);
                targetEnemy.outlineController?.SetActive(active: false);
            }
            
            targetEnemy = null;
        }
    }
}
