using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public bool cityRunTrigger = false;
    public static NPCManager instance;
    [SerializeField] List<CityPerson> cityPersons;
    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            CityPersonsRunActive();
        }
    }


    [Button(size:ButtonSizes.Large)]
    public static void CityPersonsRunActive()
    {
        if (instance.cityRunTrigger)
            return;

        instance.cityPersons.ForEach(person => { person.SetMoveType(CityPersonMoveType.Run); });
        instance.cityRunTrigger = true;
    }

}
