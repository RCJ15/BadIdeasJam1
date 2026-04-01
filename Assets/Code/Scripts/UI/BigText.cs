using BrunoMikoski.TextJuicer;
using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class BigText : Singleton<BigText>
{
    [CacheComponent]
    [SerializeField] private TMP_Text text;
    [CacheComponent]
    [SerializeField] private FitToText fitToText;
    [CacheComponent]
    [SerializeField] private UIDissolve dissolve;
    [CacheComponent]
    [SerializeField] private TMP_TextJuicer textJuicer;

    protected override void Awake()
    {
        base.Awake();

        dissolve.DissolveAmount = 1;
    }

    public static void Appear(string text, float delay = 0)
    {
        Instance.AppearLocal(text, delay);
    }

    public static void Disappear()
    {
        Instance.DisappearLocal();
    }

    private void AppearLocal(string text, float delay = 0)
    {
        dissolve.DOKill();
        dissolve.TweenDissolveAmount(0, 0.3f).SetDelay(delay);

        fitToText.enabled = true;

        this.text.text = text;

        textJuicer.SetDirty();
        textJuicer.enabled = true;

        SoundManager.PlaySound("big_text");
    }

    private void DisappearLocal()
    {
        dissolve.DOKill();
        dissolve.TweenDissolveAmount(1, 0.3f).onComplete = () =>
        {
            textJuicer.enabled = false;
            fitToText.enabled = false;
        };
    }
}
