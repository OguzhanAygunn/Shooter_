using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteHandler : MonoBehaviour
{
    public static SpriteHandler instance;

    [SerializeField] List<SpriteInfo> allSprites;

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    public static Sprite GetSprite(string id)
    {
        return instance.allSprites.Find(info => info.id == id).sprite;
    }

}


[System.Serializable]
public class SpriteInfo
{
    public string id;
    public Sprite sprite;
}
