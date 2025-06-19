using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSlotPanelUI : MonoBehaviour
{
    public static WeaponSlotPanelUI instance;

    [Title("Main")]
    public bool buttonSelectable;


    [Space(6)]

    [Title("Slots")]
    [SerializeField] WeaponSlotUI currentSlot;
    [SerializeField] List<WeaponSlotUI> slots;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    public async UniTaskVoid SetActiveSelectable(bool active,float delay)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delay));

        buttonSelectable = active;
    }


    public void ChangeActiveSlot(WeaponSlotUI newSlot)
    {
        if (!buttonSelectable)
            return;




    }
}
