using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmojiManager : MonoBehaviour
{
    public static EmojiManager instance;

    [SerializeField] Transform emojiEffectsParent;
    [SerializeField] GameObject templatePrefab;


    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }


    public static void SpawnEmojiEffect(Vector3 pos,string spriteID)
    {
        Camera cam = CameraManager.instance.mainCam;
        Sprite sprite = SpriteHandler.GetSprite(spriteID);

        Transform effectTrs = new GameObject("Emoji World Pos").transform;
        effectTrs.position = pos + Vector3.up * 1.5f;

        EmojiTemplateController emoji = Instantiate(instance.templatePrefab).GetComponent<EmojiTemplateController>();
        emoji.transform.parent = instance.emojiEffectsParent;
        emoji.AssignCamera(cam);
        emoji.AssignSprite(sprite);
        emoji.AssignWorldPos(effectTrs);
        emoji.PlayAnim();

    }
}
