using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReloadUIController : MonoBehaviour
{
    public static ReloadUIController instance;

    [Title("UI")]
    [SerializeField] CanvasGroup group;
    [SerializeField] TextMeshProUGUI titleText;

    [Space(6)]

    [Title("Bars")]
    [SerializeField] Transform barParent;
    [SerializeField] RectTransform barParentRect;
    [SerializeField] List<ReloadBar> reloadBars;

    [Space(6)]

    [Title("Counter")]
    [SerializeField] CanvasGroup counterGroup;
    [SerializeField] float counterTime;
    [SerializeField] Image counterImage;
    [SerializeField] TextMeshProUGUI counterText;


    private Vector3 barScale;
    private Vector2 barSizeDelta;
    private readonly string titleTextStr = "Reloading...";

    private void Awake()
    {
        instance = (!instance) ? this : instance;
    }
    private void Start()
    {
        barScale = barParent.localScale;
        titleText.text = "";

        barSizeDelta = barParentRect.sizeDelta;

        barParentRect.sizeDelta = new Vector2(0, 0);

        counterGroup.alpha = 0;
    }


    [Button(size: ButtonSizes.Large)]
    public async UniTaskVoid PlayAnim()
    {
        group.DOFade(1f, 0.2f);

        TitleTextAnim().Forget();
        PlayBarParents().Forget();
        PlayCounter().Forget();

        await UniTask.Delay(TimeSpan.FromSeconds(1f + 1.9f));
        group.DOFade(0f, 0.25f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

        titleText.text = "";
        barParentRect.sizeDelta = new Vector2(0, 0);
        reloadBars.ForEach(bar => bar.ResetBar());
    }

    public async UniTaskVoid PlayCounter()
    {
        float fakeTimer = counterTime;
        counterText.text = fakeTimer.ToString() + "s";
        counterGroup.DOFade(1, 0.25f);


        int index = 0;
        while (fakeTimer >= 0)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1));
            fakeTimer -= 0.1f;


            if (fakeTimer % 1 < 0.1f)
            {
                counterText.text = ((int)fakeTimer).ToString() + ",0" + "s";
            }
            else
            {
                counterText.text = fakeTimer.ToString("0.#") + "s";
            }
            index++;


        }

        counterGroup.DOFade(0, 0.25f);

    }

    public async UniTaskVoid TitleTextAnim()
    {
        titleText.DOFade(0, 0);
        titleText.text = titleTextStr;
        titleText.DOFade(1, 0.5f);
        await UniTask.Delay(0);
    }

    public async UniTaskVoid PlayBarParents()
    {
        return;
        Vector2 firstSizeDelta = barSizeDelta;
        firstSizeDelta.x = firstSizeDelta.x * 0.05f;
        barParentRect.DOSizeDelta(firstSizeDelta, 0.3f);

        await UniTask.Delay(TimeSpan.FromSeconds(0.35f));
        barParentRect.DOSizeDelta(barSizeDelta, 0.8f);
        await UniTask.Delay(TimeSpan.FromSeconds(0.8f));

        float delay = 0f;

        float totalDelay = 0;
        foreach (ReloadBar bar in reloadBars)
        {
            bar.PlayAnim(delay).Forget();
            delay += 0.08f;
            totalDelay += delay;
        }

        await UniTask.Delay(TimeSpan.FromSeconds(totalDelay));
    }


}
