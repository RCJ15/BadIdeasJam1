using DG.Tweening;
using Input;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[SingletonMode(true)]
public class TooltipManager : Singleton<TooltipManager>
{
    private static readonly List<TooltipProvider> _activeProviders = new();
    private static int _activeCount;

    [CacheComponent]
    [SerializeField] private Canvas canvas;
    private RectTransform _canvasRect;
    [CacheComponent]
    [SerializeField] private FitToText fitToText;
    [CacheComponent]
    [SerializeField] private UIDissolve dissolve;

    [Space]
    [SerializeField] private RectTransform mouse;
    [SerializeField] private float hoverTime;
    private float _hoverTimer;

    [Space]
    [SerializeField] private RectTransform tooltipTransform;
    [SerializeField] private TMP_Text text;
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform debug;
    [Space]
    [SerializeField] private float extraYOffset;
    [SerializeField] private Vector2 edgesOffset;
    [Space]
    [SerializeField] private float titleSize;
    [SerializeField] private float descriptionSize;

    private string _title = "Title";
    private string _description = "Description";

    private TooltipProvider _oldProvider;
    private TooltipProvider _provider;
    private bool _active;

    public static void AddActive(TooltipProvider provider)
    {
        _activeProviders.Add(provider);
        _activeCount++;

        Instance.UpdateProvider();
    }
    public static void RemoveActive(TooltipProvider provider)
    {
        if (_activeProviders.Remove(provider))
        {
            _activeCount--;

            Instance.UpdateProvider();
        }
    }

    private void Start()
    {
        _canvasRect = canvas.transform as RectTransform;
        UpdateProvider();

        dissolve.DissolveAmount = 1;
    }

    private void Update()
    {
        if (_active) return;
        if (_provider == null) return;

        _hoverTimer += Time.deltaTime;

        if (_hoverTimer >= hoverTime)
        {
            Set(_provider);
            _active = true;

            dissolve.DOKill();
            dissolve.TweenDissolveAmount(0, 0.2f);
        }
    }

    private void UpdateProvider()
    {
        _provider = _activeCount <= 0 ? null : _activeProviders[_activeCount - 1];

        if (_oldProvider == _provider)
        {
            return;
        }

        _oldProvider = _provider;

        _hoverTimer = 0;

        if (_active)
        {
            dissolve.DOKill();
            dissolve.TweenDissolveAmount(1, 0.2f);
        }

        _active = false;
    }

    private string ProcessString(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;

        s = s.Replace("#ENERGY#", "<sprite name=\"Energy\" tint=1>");
        s = s.Replace("#HEALTH#", "<sprite name=\"Heart\" tint=1>");
        s = s.Replace("#SWORD#", "<sprite name=\"Sword\" tint=1>");
        s = s.Replace("#SPEED#", "<sprite name=\"Speed\" tint=1>");

        return s;
    } 

    private void Set(TooltipProvider provider)
    {
        _title = ProcessString(provider.Title);
        _description = ProcessString(provider.Description);

        text.text = "";

        if (!string.IsNullOrEmpty(_title))
        {
            text.text += $"<u><size={titleSize}>{_title}</size></u>\n";
        }

        if (!string.IsNullOrEmpty(_description))
        {
            text.text += $"<size={descriptionSize}>{_description}</size>";
        }

        text.rectTransform.anchoredPosition = Vector2.zero;
        text.ForceMeshUpdate(true);

        fitToText.Fit();

        text.rectTransform.anchoredPosition = -background.anchoredPosition;
        background.anchoredPosition = Vector2.zero;

        Rect rect = provider.ScreenRect();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, rect.center, null, out Vector2 pos);
        Vector2 size = rect.size / canvas.scaleFactor;
        Vector2 halfSize = size / 2f;

        if (debug.gameObject.activeSelf)
        {
            debug.anchoredPosition = pos;
            debug.sizeDelta = size;
        }

        Rect canvasRect = _canvasRect.rect;
        Vector2 halfCanvasSize = (canvasRect.size / 2f) - edgesOffset;
        Rect backgroundRect = background.rect;

        float xOffset = 0;
        float yOffset = (size.y + backgroundRect.height) / 2f + extraYOffset;

        bool top = false;

        if (pos.y - halfSize.y - backgroundRect.height - extraYOffset < -halfCanvasSize.y)
        {
            top = true;
        }

        if (pos.x + backgroundRect.width / 2f > halfCanvasSize.x)
        {
            xOffset = halfCanvasSize.x - pos.x - backgroundRect.width / 2f;
        }
        else if (pos.x - backgroundRect.width / 2f < -halfCanvasSize.x)
        {
            xOffset = -(pos.x - backgroundRect.width / 2f + halfCanvasSize.x);
        }

        tooltipTransform.anchoredPosition = pos + new Vector2(xOffset, yOffset * (top ? 1 : -1f));
    }
}
