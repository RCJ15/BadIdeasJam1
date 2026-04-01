using DG.Tweening;
using UnityEngine;

public class CommandDragVisual : Singleton<CommandDragVisual>
{
    [CacheComponent]
    [SerializeField] private CommandVisuals visuals;
    [CacheComponent]
    [SerializeField] private UIDissolve dissolve;

    [Space]
    [SerializeField] private RectTransform visualsRect;

    public static Vector2 Position
    {
        get
        {
            TrySetTextOffset();
            return _position + _textOffset.Value;
        }
        set
        {
            TrySetTextOffset();

            _position = value + _textOffset.Value;
            _t = 1;

            Instance.DOKill();
            DOTween.To(() => _t, (v) => _t = v, 0, 0.5f).SetEase(Ease.OutExpo).SetTarget(Instance);
        }
    }
    private static Vector2 _position;
    private static Vector2? _textOffset = null;
    private static float _t;

    private Vector2 _visualsPosition;
    private Quaternion _visualsRotation;

    private bool _visible;

    private static void TrySetTextOffset()
    {
        Transform transform = Instance.transform;

        if (!_textOffset.HasValue)
        {
            Transform text = Instance.visuals.Text.rectTransform;
            _textOffset = transform.position - text.position;
        }
    }

    public static Command Command => _command;
    private static Command _command;

    private void Start()
    {
        dissolve.DissolveAmount = 1;
    }

    private void LateUpdate()
    {
        if (_visible)
        {
            _visualsPosition = visualsRect.position;
            _visualsRotation = visualsRect.rotation;
        }

        transform.position = Vector2.Lerp(_visualsPosition, _position, _t);
        transform.rotation = Quaternion.Slerp(_visualsRotation, Quaternion.identity, _t);
    }

    public static void SetCommand(Command command, bool instant = false)
    {
        Instance.SetCommandLocal(command, instant);
    }

    private void SetCommandLocal(Command command, bool instant = false)
    {
        _visible = command != null;

        dissolve.DOKill();

        if (_visible)
        {
            if (instant)
            {
                dissolve.DissolveAmount = 0;
            }
            else
            {
                dissolve.TweenDissolveAmount(0, 0.25f);
            }

            visuals.Command = command;
        }
        else
        {
            if (instant)
            {
                dissolve.DissolveAmount = 1;
            }
            else
            {
                dissolve.TweenDissolveAmount(1, 0.25f);
            }
        }
    }
}
