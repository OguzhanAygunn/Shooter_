using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TenchObjectsManager : MonoBehaviour
{
    public static TenchObjectsManager instance;

    public List<TenchObjectInfo> tenchObjects;

    private void Awake()
    {
        instance = (!instance) ? instance : this;
    }

    private void OnValidate()
    {
        if(!instance)
            instance = this;
    }

    public static TenchObjectInfo GetTenchObjInfo(string id)
    {
        return instance.tenchObjects.Find(x => x.id == id);
    }

    public static GameObject GetTenchObj(string id)
    {
        return GetTenchObjInfo(id: id).prefab;
    }

    [Button(size: ButtonSizes.Large)]
    public void SpawnTenchObj(string id,Transform _parent)
    {
        GameObject prefab = GetTenchObj(id: id);
        GameObject obj = Instantiate(GetTenchObj(id: id));
        obj.transform.SetParent(_parent);
        obj.transform.localPosition = Vector3.zero;
        obj.name = prefab.name;
    }

}


[System.Serializable]
public class TenchObjectInfo
{
    public string id;
    public GameObject prefab;
}
