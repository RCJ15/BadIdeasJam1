using DG.Tweening;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;

public class CommandVisuals : MonoBehaviour
{
    public TMP_Text Text => text;
    public FitToText FitToText => fitToText;
    public RectTransform IconRect => iconRect;
    public SVGImage Icon => icon;

    public Color Color
    {
        get => _color;
        set
        {
            _color = value;
            UpdateColor();
        }
    }

    private Color _color = Color.white;

    [CacheComponent]
    [SerializeField] private TMP_Text text;
    [CacheComponent]
    [SerializeField] private FitToText fitToText;
    private RectTransform _fitToTextRect;

    [Space]
    [SerializeField] private RectTransform iconRect;
    [SerializeField] private SVGImage icon;

    public Command Command
    {
        get => _command;
        set
        {
            if (_command == value) return;

            _command = value;

            if (_command == null) return;

            text.text = _command.FormattedCommandName;
            text.ForceMeshUpdate(true);
            fitToText.Fit();

            icon.sprite = _command.Icon;
            icon.transform.localScale = new(_command.IconFlipX ? -1 : 1, _command.IconFlipY ? -1 : 1, 1);

            UpdateColor();

            UpdateIconRect();
        }
    }
    private Command _command;

    private void LateUpdate()
    {
        // For some godforsaken reason the icon rect keeps not being aligned even though nothing should modify it except this script so I have to do this
        if (_command != null)
        {
            UpdateIconRect();
        }
    }

    private void UpdateIconRect()
    {
        if (_fitToTextRect == null)
        {
            _fitToTextRect = fitToText.transform as RectTransform;
        }

        float size = _fitToTextRect.sizeDelta.y;
        iconRect.sizeDelta = new(size, size);
        iconRect.anchoredPosition = new(-(size / 2f), 0);
    }

    private void UpdateColor()
    {
        Color color = _command.Color * _color;
        text.color = color;
        icon.color = color;
    }

    public Tween TweenColor(Color to, float duration)
    {
        return DOTween.To(() => Color, (c) => Color = c, to, duration).SetTarget(this);
    }
}
