using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBarUI : MonoBehaviour
{
    public static PlayerHealthBarUI instance;

    [Title("Visual")]
    [SerializeField] Slider slider;


    private PlayerHealth playerHealth;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    private void Start()
    {
        playerHealth = PlayerHealth.instance;
    }


    public void UpdateSlider()
    {
        slider.value = playerHealth.health / playerHealth.maxHealth;
    }

}
