using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/*
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;
#endif
*/

public class CustomButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public UnityEvent OnClick => onClick;

    //[Header("Custom button properties")]
    [SerializeField] private UnityEvent onClick;
    [SerializeField] private RectTransform visualsRect;
    private Vector2 _visualsStartPos;

    private GlobalUISettings _settings;

    private float _currentScale = 1f;
    private float _shakeTimer;

    private Tween _scaleTween;
    private bool _select;
    private bool _pressed;

    private bool _oldSelect;
    private bool _oldPressed;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _select = true;
        UpdateState();

        _settings.HoverSfx?.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _select = false;
        UpdateState();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _pressed = true;
        UpdateState();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _pressed = false;
        UpdateState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
        _settings.ClickSfx?.Play();
    }

    private void Start()
    {
        _visualsStartPos = visualsRect.anchoredPosition;
        _settings = GlobalUISettings.Instance;
    }

    private void UpdateState(bool instant = false)
    {
        if (_oldSelect == _select && _oldPressed == _pressed) return;

        float scale;

        if (_pressed)
        {
            scale = _settings.PressedSize;
        }
        else if (_select)
        {
            scale = _settings.SelectedSize;
        }
        else
        {
            scale = 1f;
        }

        GlobalUISettings.TweenSettings tweenSettings = _settings.NormalSizeTween;

        if (_oldPressed)
        {
            tweenSettings = _settings.FromPressedSizeTween;
        }
        else if (_pressed)
        {
            tweenSettings = _settings.PressedSizeTween;
        }

        _oldSelect = _select;
        _oldPressed = _pressed;

        if (scale == _currentScale) return;
        _currentScale = scale;

        if (_scaleTween != null) _scaleTween.Kill();

        if (instant)
        {
            visualsRect.localScale = Vector3.one * scale;
        }
        else
        {
            _scaleTween = visualsRect.DOScale(scale, tweenSettings.Duration).SetEase(tweenSettings.Ease);
        }
    }

    /*
    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);

#if UNITY_EDITOR
        if (!Application.isPlaying) return;
#endif

        if (_oldState == state) return;

        float scale;
        GlobalUISettings.TweenSettings tweenSettings = _settings.NormalSizeTween;

        if (_oldState == SelectionState.Pressed) tweenSettings = _settings.FromPressedSizeTween;

        switch (state)
        {
            default:
            case SelectionState.Normal:
            case SelectionState.Disabled:
                scale = 1f;
                _shake = false;
                break;

            case SelectionState.Selected:
            case SelectionState.Highlighted:
                scale = _settings.SelectedSize;
                _shake = false;
                _settings.HoverSfx.Play();
                break;

            case SelectionState.Pressed:
                scale = _settings.PressedSize;
                tweenSettings = _settings.PressedSizeTween;
                _shake = true;
                _settings.ClickSfx.Play();
                break;
        }

        if (scale == _currentScale) return;
        _currentScale = scale;

        if (_scaleTween != null) _scaleTween.Kill();

        if (instant)
        {
            visualsRect.localScale = Vector3.one * scale;
        }
        else
        {
            _scaleTween = visualsRect.DOScale(scale, tweenSettings.Duration).SetEase(tweenSettings.Ease);
        }
    }
    */

    private void Update()
    {
        if (!_pressed)
        {
            visualsRect.anchoredPosition = _visualsStartPos;
            _shakeTimer = 0;
            return;
        }

        if (_shakeTimer <= 0)
        {
            visualsRect.anchoredPosition = _visualsStartPos + Random.insideUnitCircle * _settings.ShakeIntensity;

            _shakeTimer = _settings.TimeBtwShakes;
        }
        else
        {
            _shakeTimer -= Time.deltaTime;
        }
    }
}

/*
#if UNITY_EDITOR
[CustomEditor(typeof(CustomButton), true)]
public class CustomButtonEditor : ButtonEditor
{
    private static readonly HashSet<string> _blacklistedProps = new()
    {
        "m_Script",
        "m_Navigation",
        "m_Transition",
        "m_Colors",
        "m_SpriteState",
        "m_AnimationTriggers",
        "m_Interactable",
        "m_TargetGraphic",
        "m_OnClick",
    };

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.UpdateIfRequiredOrScript();

        SerializedProperty prop = serializedObject.GetIterator();
        bool enterChildren = true;

        while (prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            string propPath = prop.propertyPath;

            if (_blacklistedProps.Contains(propPath)) continue;

            EditorGUILayout.PropertyField(prop, true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
*/