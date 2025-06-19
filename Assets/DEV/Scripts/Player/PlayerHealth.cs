using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;
    [Title("Main")]
    public float maxHealth;
    public float health;
    private PlayerHealthBarUI healthBarUI;

    [Title("Visual")]
    [SerializeField] Image background;
    [SerializeField] Color hitColor;
    [SerializeField] float colorSpeed;
    private Color defaultColor;
    
    private void Awake()
    {
        instance = (!instance) ? this : instance;
        maxHealth = health;
        defaultColor = background.color;
    }

    private void Start()
    {
        healthBarUI = PlayerHealthBarUI.instance;
    }

    private void Update()
    {
        background.color = Vector4.MoveTowards(background.color, defaultColor, colorSpeed * Time.deltaTime);    
    }


    [Button(size: ButtonSizes.Large)]
    public void DecreaseHealth(float decreaseVal)
    {
        health -= decreaseVal;
        healthBarUI.UpdateSlider();
        background.color = hitColor;
    }
}
