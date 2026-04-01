using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening.Core;
using DG.Tweening;
using DG.Tweening.Plugins.Options;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Dissolve effect for UI. I hate how I made this code, but it works so where is the harm? <para/>
/// Source: https://github.com/mob-sakai/UIEffect <br/>
/// License (MIT): https://github.com/mob-sakai/UIEffect/blob/main/LICENSE.md
/// </summary>
[RequireComponent(typeof(Graphic))]
[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
public class UIDissolve : BaseMeshEffect, IMaterialModifier
{
    public static Shader Shader
    {
        get
        {
            if (_shader == null)
            {
                _shader = Shader.Find("Custom/UI/Dissolve");
            }

            return _shader;
        }
    }
    private static Shader _shader = null;

    private const uint _shaderId = 0 << 3;
    private static readonly Hash128 _invalidHash = new Hash128();

    private static readonly Dictionary<Hash128, (Material, int)> _materialMap = new Dictionary<Hash128, (Material, int)>();

    private static readonly int _transitionTextureID = Shader.PropertyToID("_TransitionTex");

    private const int PARAMETER_CHANNELS = 2 * 4;
    private const int PARAMETER_INSTANCE_LIMIT = 128;
    private static Texture2D ParameterTexture
    {
        get
        {
            TryCreateParameterTexture();

            return _parameterTexture;
        }
    }
    private static Texture2D _parameterTexture;
    private static bool _updateParameterTexture;
    private static readonly int _parameterPropertyId = Shader.PropertyToID("_ParamTex");
    private static readonly byte[] _parameterData = new byte[PARAMETER_CHANNELS * PARAMETER_INSTANCE_LIMIT];
    private static readonly Stack<int> _parameterIndexStack = new Stack<int>(PARAMETER_INSTANCE_LIMIT);

    private int _parameterIndex;
    private Hash128 _effectMaterialHash;

    [Range(0, 1)]
    [SerializeField] private float dissolveAmount = 0.5f;

    [SerializeField] private bool invert = false;
    [SerializeField] private bool pixelPerfect = false;

    [Range(0, 1)]
    [SerializeField] private float width = 0.5f;

    [Range(0, 1)]
    [SerializeField] private float softness = 0.5f;

    [SerializeField] private Color edgeColor = Color.white;
    [SerializeField] private ColorMode edgeColorMode = ColorMode.Multiply;
    [SerializeField] private Texture transitionTexture;

    [Header("Rect")]
    [SerializeField] private EffectArea effectArea;

    [SerializeField] private float scale = 1;

    [SerializeField] private bool keepAspectRatio = true;

    private bool _lastKeepAspectRatio;
    private float _lastScale;
    private EffectArea _lastEffectArea;

    private RectTransform _rectTransform;

    /// <summary>
    /// How dissolved the graphic is. <para/>
    /// 0 = not dissolved at all, which means it's fully <b>VISIBLE</b>. <para/>
    /// 1 = fully dissolved, which means it's <b>INVISIBLE</b>.
    /// </summary>
    public float DissolveAmount
    {
        get => dissolveAmount;
        set
        {
            this.DOKill();
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(dissolveAmount, value))
            {
                return;
            }

            dissolveAmount = value;
            SetEffectParamsDirty();
        }
    }

    /// <summary>
    /// If true, then the <see cref="TransitionTexture"/>s alpha pixels will be inverted.
    /// </summary>
    public bool Invert
    {
        get => invert;
        set
        {
            if (invert == value)
            {
                return;
            }

            invert = value;
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// Pixelation... TODO: Proper description for this
    /// </summary>
    public bool PixelPerfect
    {
        get => pixelPerfect;
        set
        {
            if (pixelPerfect == value)
            {
                return;
            }

            pixelPerfect = value;
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// How large the edge color is on the edges of the dissolve effect, on a scale of 0 to 1.
    /// </summary>
    public float Width
    {
        get => width;
        set
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(width, value))
            {
                return;
            }

            width = value;
            SetEffectParamsDirty();
        }
    }

    /// <summary>
    /// The amount of softness on the edges of the dissolve effect, on a scale of 0 to 1.
    /// </summary>
    public float Softness
    {
        get => softness;
        set
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(softness, value))
            {
                return;
            }

            softness = value;
            SetEffectParamsDirty();
        }
    }

    /// <summary>
    /// The color around the edges of the dissolve effect.
    /// </summary>
    public Color EdgeColor
    {
        get => edgeColor;
        set
        {
            if (edgeColor == value)
            {
                return;
            }

            edgeColor = value;
            SetEffectParamsDirty();
        }
    }

    /// <summary>
    /// How the <see cref="EdgeColor"/> will interact with the edges of the dissolve effect.
    /// </summary>
    public ColorMode EdgeColorMode
    {
        get => edgeColorMode;
        set
        {
            if (edgeColorMode == value)
            {
                return;
            }

            edgeColorMode = value;
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// The main texture that determines the shape of the dissolve effect.
    /// </summary>
    public Texture TransitionTexture
    {
        get
        {
            return transitionTexture == null ? Texture2D.whiteTexture : transitionTexture;
        }
        set
        {
            if (transitionTexture == value)
            {
                return;
            }

            transitionTexture = value;
            SetMaterialDirty();
        }
    }

    /// <summary>
    /// How the <see cref="TransitionTexture"/> will cover the area of the graphic.
    /// </summary>
    public EffectArea DissolveEffectArea
    {
        get => effectArea;
        set
        {
            if (effectArea == value)
            {
                return;
            }

            effectArea = value;
            SetVerticesDirty();
        }
    }

    /// <summary>
    /// How much larger/smaller the <see cref="TransitionTexture"/> should be on the graphic. <para/>
    /// 1 is the default scale.
    /// </summary>
    public float Scale
    {
        get => scale;
        set
        {
            if (scale == value)
            {
                return;
            }

            scale = value;
            SetVerticesDirty();
        }
    }

    /// <summary>
    /// If the <see cref="TransitionTexture"/> should keep its original aspect ratio or be stretched to fit the current graphics rect (if it doesn't already match the textures aspect ratio).
    /// </summary>
    public bool KeepAspectRatio
    {
        get => keepAspectRatio;
        set
        {
            if (keepAspectRatio == value)
            {
                return;
            }

            keepAspectRatio = value;
            SetVerticesDirty();
        }
    }

    protected override void OnEnable()
    {
        _rectTransform = transform as RectTransform;

        if (_parameterIndex <= 0 && _parameterIndexStack.Count > 0)
        {
            _parameterIndex = _parameterIndexStack.Pop();
        }

        SetMaterialDirty();
        SetVerticesDirty();
        SetEffectParamsDirty();
    }

    protected override void OnDisable()
    {
        SetMaterialDirty();

        _parameterIndexStack.Push(_parameterIndex);
        _parameterIndex = 0;
    }

    public Hash128 GetMaterialHash(Material material)
    {
        if (!isActiveAndEnabled || !material || !material.shader)
        {
            return _invalidHash;
        }

        uint shaderVariantId = (uint)((int)edgeColorMode << 6);
        uint resourceId = (uint)TransitionTexture.GetInstanceID();

        bool[] booleans = new[] { invert, pixelPerfect };
        int combinedBooleans = 0;
        for (int i = 0; i < booleans.Length; i++)
        {
            combinedBooleans |= Convert.ToInt16(booleans[i]) << i;
        }

        return new Hash128(
            (uint)material.GetInstanceID(),
            _shaderId + shaderVariantId,
            (uint)combinedBooleans,
            resourceId
        );
    }

    public Material GetModifiedMaterial(Material baseMaterial)
    {
        return GetModifiedMaterial(baseMaterial, graphic);
    }

    public virtual Material GetModifiedMaterial(Material baseMaterial, Graphic graphic)
    {
        if (!isActiveAndEnabled)
        {
            return baseMaterial;
        }

        Hash128 oldHash = _effectMaterialHash;

        _effectMaterialHash = GetMaterialHash(baseMaterial);

        Material modifiedMaterial = baseMaterial;

        if (_effectMaterialHash.isValid)
        {
            if (!_materialMap.TryGetValue(_effectMaterialHash, out (Material, int) entry))
            {
                Material newMat = new Material(baseMaterial) { hideFlags = HideFlags.HideAndDontSave };
                entry = (newMat, 0);

                ModifyMaterial(newMat, graphic);
                _materialMap.Add(_effectMaterialHash, entry);
            }

            modifiedMaterial = entry.Item1;

            entry.Item2++;
        }

        if (oldHash.isValid)
        {
            if (_materialMap.TryGetValue(_effectMaterialHash, out (Material, int) entry) && --entry.Item2 > 0)
            {
                DestroyImmediate(entry.Item1);
                _materialMap.Remove(_effectMaterialHash);
            }
        }

        return modifiedMaterial;
    }

    public void ModifyMaterial(Material baseMaterial, Graphic graphic)
    {
        baseMaterial.shader = Shader;

        // Set keywords
        int length = 1;

        if (invert) length++;
        if (pixelPerfect) length++;

        string[] keywords = new string[length];

        int i = 0;
        keywords[i] = edgeColorMode.ToString().ToUpper();

        if (invert)
        {
            i++;
            keywords[i] = "INVERT";
        }

        if (pixelPerfect)
        {
            i++;
            keywords[i] = "PIXEL_PERFECT";
        }

        baseMaterial.shaderKeywords = keywords;

        // Set name
        baseMaterial.name = Path.GetFileName(baseMaterial.shader.name);

        // Set textures
        baseMaterial.SetTexture(_transitionTextureID, TransitionTexture);
        baseMaterial.SetTexture(_parameterPropertyId, ParameterTexture);
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        // bool isText = isTMPro || graphic is Text;
        float normalizedIndex = ((float)_parameterIndex - 0.5f) / PARAMETER_INSTANCE_LIMIT;

        UIVertex vertex = default(UIVertex);
        Texture tex = TransitionTexture;
        Rect rect;

        switch (effectArea)
        {
            //case EffectArea.RectTransform:
            default:
                rect = _rectTransform.rect;
                break;

            case EffectArea.Fit:
                // Fit to contents.
                float xMin = float.MaxValue;
                float yMin = float.MaxValue;
                float xMax = float.MinValue;
                float yMax = float.MinValue;
                for (int i = 0; i < vh.currentVertCount; i++)
                {
                    vh.PopulateUIVertex(ref vertex, i);
                    float x = vertex.position.x;
                    float y = vertex.position.y;
                    xMin = Mathf.Min(xMin, x);
                    yMin = Mathf.Min(yMin, y);
                    xMax = Mathf.Max(xMax, x);
                    yMax = Mathf.Max(yMax, y);
                }

                rect = new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
                break;
        }

        if (keepAspectRatio && tex)
        {
            float aspectRatio = (float)tex.width / (float)tex.height;

            if (rect.width < rect.height)
            {
                rect.width = rect.height * aspectRatio;
            }
            else
            {
                rect.height = rect.width / aspectRatio;
            }
        }

        rect.width *= scale;
        rect.height *= scale;

        // Calculate vertex position.
        vertex = default(UIVertex);
        int count = vh.currentVertCount;

        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            float x;
            float y;

            if (effectArea == EffectArea.Fit)
            {
                x = (vertex.position.x - rect.xMin) / rect.width;
                y = (vertex.position.y - rect.yMin) / rect.height;
            }
            else
            {
                x = vertex.position.x / rect.width + 0.5f;
                y = vertex.position.y / rect.height + 0.5f;
            }

            vertex.uv1.x = x;
            vertex.uv1.y = y;
            vertex.uv1.z = normalizedIndex;

            vh.SetUIVertex(vertex, i);
        }
    }

    private void SetEffectParamsDirty()
    {
        if (_parameterIndex <= 0)
        {
            return;
        }

        void SetData(int channelId, float floatValue)
        {
            byte value;

            if (floatValue >= 1)
            {
                value = 255;
            }
            else if (floatValue <= 0)
            {
                value = 0;
            }
            else
            {
                value = (byte)(Mathf.Clamp01(floatValue) * 255);
            }

            int index = (_parameterIndex - 1) * PARAMETER_CHANNELS + channelId;

            _parameterData[index] = value;
        }

        // Channel 1: Main data
        SetData(0, dissolveAmount);
        SetData(1, width);
        SetData(2, softness);
        // 3

        // Channel 2: Edge color
        SetData(4, edgeColor.r);
        SetData(5, edgeColor.g);
        SetData(6, edgeColor.b);
        SetData(7, edgeColor.a);

        _updateParameterTexture = true;
    }

    protected void SetVerticesDirty()
    {
        graphic.SetVerticesDirty();

        _lastKeepAspectRatio = keepAspectRatio;
        _lastScale = scale;
        _lastEffectArea = effectArea;
    }

    public void SetMaterialDirty()
    {
        graphic.SetMaterialDirty();
    }

    protected override void OnDidApplyAnimationProperties()
    {
        base.OnDidApplyAnimationProperties();

        if (_lastKeepAspectRatio != keepAspectRatio || _lastScale != scale || _lastEffectArea != effectArea)
        {
            SetVerticesDirty();
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
    private static void Initialize()
    {
#if UNITY_EDITOR
        foreach (var pair in _materialMap)
        {
            if (pair.Value.Item1 == null)
            {
                continue;
            }

            DestroyImmediate(pair.Value.Item1);
        }

        _materialMap.Clear();
#endif

        TryCreateParameterTexture();
    }

    private static void TryCreateParameterTexture()
    {
        if (_parameterTexture != null)
        {
            return;
        }

        bool isLinear = QualitySettings.activeColorSpace == ColorSpace.Linear;

        _parameterTexture = new Texture2D(PARAMETER_CHANNELS / 4, PARAMETER_INSTANCE_LIMIT, TextureFormat.RGBA32, false, isLinear)
        {
            hideFlags = HideFlags.HideAndDontSave,
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
        };

        _parameterIndexStack.Clear();

        for (int i = 1; i < PARAMETER_INSTANCE_LIMIT + 1; i++)
        {
            _parameterIndexStack.Push(i);
        }

        _updateParameterTexture = true;

        TryUpdateParameterTexture();

        Canvas.willRenderCanvases += TryUpdateParameterTexture;
    }

    private static void TryUpdateParameterTexture()
    {
        if (!_updateParameterTexture || ParameterTexture == null)
        {
            return;
        }

        _updateParameterTexture = false;
        ParameterTexture.LoadRawTextureData(_parameterData);
        ParameterTexture.Apply(false, false);
    }

    /// <summary>
    /// Color mode enum for <see cref="EdgeColorMode"/>.
    /// </summary>
    public enum ColorMode
    {
        /// <summary>
        /// Color is multiplied by the original color.
        /// </summary>
        Multiply,
        /// <summary>
        /// Color will replace the old original color.
        /// </summary>
        Fill,
        /// <summary>
        /// Color is added together with the original color.
        /// </summary>
        Add,
        /// <summary>
        /// Color is subtracted from the original color.
        /// </summary>
        Subtract,
    }

    /// <summary>
    /// Effect area enum for <see cref="DissolveEffectArea"/>.
    /// </summary>
    public enum EffectArea
    {
        /// <summary>
        /// The area is determined by the rect transform.
        /// </summary>
        RectTransform,
        /// <summary>
        /// The area is scaled and transformed to fit within the bounds of the rect transform.
        /// </summary>
        Fit,
    }

#if UNITY_EDITOR
    protected override void Reset()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        SetMaterialDirty();
        SetVerticesDirty();
        SetEffectParamsDirty();
    }

    protected override void OnValidate()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        SetMaterialDirty();
        SetVerticesDirty();
        SetEffectParamsDirty();
    }
#endif

    #region TWEENING
    /// <summary>
    /// Tweens the <see cref="DissolveAmount"/> variable using DOTween.
    /// </summary>
    public TweenerCore<float, float, FloatOptions> TweenDissolveAmount(float endValue, float duration)
    {
        this.DOKill();
        TweenerCore<float, float, FloatOptions> t = DOTween.To(() => dissolveAmount, x =>
        {
            dissolveAmount = x;
            SetEffectParamsDirty();
        }, endValue, duration);
        /*
        t.onComplete += () =>
        {
            dissolveAmount = endValue;
            SetEffectParamsDirty();
        };
        */
        t.SetTarget(this);
        return t;
    }
    #endregion
}