using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FilterUI : MonoBehaviour
{
    public static FilterUI instance;

    public Image image;
    public List<FilterAnimInfo> anims;

    private void Awake()
    {
        instance = (!instance) ? this : instance;

        anims.ForEach(anim => anim.AssignImage(_image: image));
        anims.ForEach(anim => anim.Init());
    }

    [Button(size: ButtonSizes.Large)]
    public static void PlayAnim(string id)
    {
        instance.anims.Find(anim => anim.id == id).PlayAnim();
    }
}

[System.Serializable]
public class FilterAnimInfo
{
    public string id;
    public Color targetColor;
    public float duration;
    private Image image;
    private Color defaultColor;
    public void AssignImage(Image _image)
    {
        image = _image;
    }

    public void Init()
    {
        defaultColor = image.color;
    }

    public void PlayAnim()
    {
        image.DOColor(targetColor, duration).OnComplete(() =>
        {
            image.DOColor(defaultColor, duration / 2f);
        });
    }
}
