using DebugTools;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FitToText : MonoBehaviour, IComponentDebugGUI
{
    [CacheComponent(DisableField = false)]
    [SerializeField] private TMP_Text text;

    [Space]
    [SerializeField] private Vector2 extraSize;
    [SerializeField] private bool updateEveryFrame;

    private RectTransform _textRect;
    private RectTransform _rect;

    private Canvas _canvas;

    private void Awake()
    {
        _textRect = text.rectTransform;
        _rect = transform as RectTransform;

        _canvas = GetComponentInParent<Canvas>(true);
    }

    private void Start()
    {
        Fit();
    }

    private void OnEnable()
    {
        Fit();
    }

    private void LateUpdate()
    {
        if (updateEveryFrame)
        {
            Fit();
        }
    }

    public void Fit()
    {
        if (_canvas == null)
        {
            _canvas = GetComponentInParent<Canvas>(true);
        }

        _rect.position = _textRect.position + text.TextBoundCenterCanvasSpace(_canvas);

        Vector2 size = text.textBounds.size;

        if (size.x <= 0) size.x = 0;
        if (size.y <= 0) size.y = 0;
        _rect.sizeDelta = size + extraSize;
    }

    public void OnDebugGUI()
    {
        DebugGUI.Property("Center", text.textBounds.center);
        DebugGUI.Property("Size", text.textBounds.size);
    }

    private void OnDrawGizmosSelected()
    {
        if (text == null) return;

        _canvas = GetComponentInParent<Canvas>(true);

        Vector2 scale = _canvas == null ? Vector2.one : _canvas.transform.localScale;

        Bounds bounds = text.textBounds;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(text.rectTransform.position + Vector3.Scale(bounds.center, scale), bounds.size * scale);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(FitToText))]
    public class FitToTextEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            if (GUILayout.Button("Apply In Editor"))
            {
                FitToText text = (FitToText)target;
                if (text == null) return;

                text._rect = text.transform as RectTransform;
                text._textRect = text.text.rectTransform;

                Undo.RecordObject(text.transform, "FitToText");
                text.Fit();
            }
        }
    }
#endif
}

public static class TMP_TextExtensions
{
    public static Vector3 TextBoundCenterCanvasSpace(this TMP_Text text, Canvas canvas = null)
    {
        Vector2 scale = canvas == null ? Vector2.one : canvas.transform.localScale;

        return Vector3.Scale(text.textBounds.center, scale); 
    }
}