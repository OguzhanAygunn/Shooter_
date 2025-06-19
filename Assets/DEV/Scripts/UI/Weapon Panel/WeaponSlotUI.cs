using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponSlotUI : MonoBehaviour
{
    [Title("Main")]
    public bool isSelected;
    [SerializeField] bool isVisibility;
    [SerializeField] Weapon weapon;

    [Space(6)]

    [Title("Visuals")]
    [SerializeField] CanvasGroup group;
    [SerializeField] Image backgroundImage;
    [SerializeField] Image iconImage;
    [SerializeField] Image ammountImage;
    [SerializeField] TextMeshProUGUI ammountText;


    private void Start()
    {
        UpdateAmmountText();
    }

    public void UpdateAmmountText()
    {
        ammountText.text = weapon.BulletCount.ToString();
    }


    public void SetSelectedActive(bool active)
    {
        isSelected = active;
    }
}
