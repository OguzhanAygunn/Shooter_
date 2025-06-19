using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmojiTemplateController : MonoBehaviour
{
    [Title("Emoji")]
    [SerializeField] Camera cam;
    [SerializeField] Transform worldPos;
    [SerializeField] Image image;
    [SerializeField] Animator animator;

    [Space(6)]

    [Title("Kill Counter")]
    [SerializeField] Image background;
    [SerializeField] TextMeshProUGUI goldText;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    void Start()
    {
        
    }

    private void Update()
    {
        Movement();
    }

    private void Movement()
    {
        if (!animator.enabled)
            return;

        Vector3 pos = cam.WorldToScreenPoint(worldPos.position);
        transform.position = pos;
    }


    public void AssignWorldPos(Transform worldPos)
    {
        this.worldPos = worldPos;
    }

    public void AssignSprite(Sprite sprite)
    {
        this.image.sprite = sprite;
    }

    public void AssignCamera(Camera camera)
    {
        this.cam = camera;
    }

    public void PlayAnim()
    {
        animator.enabled = true;

        int goldVal = CurrencyManager.GetRandomGoldValue();
        goldText.text = "+" + goldVal.ToString();
    }

    public void DestroyObject()
    {
        Destroy(gameObject);
    }

}
