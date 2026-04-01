using DG.Tweening;
using System;
using UnityEngine;

[SingletonMode(true)]
public class GlobalUISettings : Singleton<GlobalUISettings>
{
    public float SelectedSize = 1f;
    public float PressedSize = 1f;
    public TweenSettings NormalSizeTween;
    public TweenSettings PressedSizeTween;
    public TweenSettings FromPressedSizeTween;

    [Space]
    public float ShakeIntensity;
    public float TimeBtwShakes;

    [Space]
    public Sound HoverSfx;
    public Sound ClickSfx;

    [Serializable]
    public class TweenSettings
    {
        public Ease Ease;
        public float Duration;
    }

    public enum SelectionState
    {
        Normal,
        Selected,
        Pressed,
    }
}