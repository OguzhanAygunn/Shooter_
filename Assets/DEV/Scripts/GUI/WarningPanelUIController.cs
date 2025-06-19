using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WarningPanelUIController : MonoBehaviour
{
    [Title("Main")]
    [SerializeField] bool active;
    [SerializeField] Animator animator;

    [Space(6)]

    [Title("Images")]
    [SerializeField] Image background;
    [SerializeField] Image backgroundOutline;

    [Space(6)]

    [Title("Texts")]
    [SerializeField] TextMeshProUGUI titleTMPRO;
    [SerializeField] TextMeshProUGUI desTMPRO;
    private string titleText;
    private string desText;


    private void Awake()
    {
        titleText = titleTMPRO.text;
        desText = desTMPRO.text;
    }


    private void Start()
    {
        Init().Forget();
    }

    private async UniTaskVoid Init()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        PlayAnim();
    }

    [Button(size: ButtonSizes.Large)]
    public void PlayAnim()
    {
        animator.SetTrigger("OpenAnim");
    }

}
