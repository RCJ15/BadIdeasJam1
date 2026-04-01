using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CombatShadowTexture : MonoBehaviour
{
    private static readonly int _property = Shader.PropertyToID("_TexelSize");

    [CacheComponent]
    [SerializeField] private RawImage image;
    private Material _mat;

    [Space]
    [SerializeField] private float duration = 1f;
    [SerializeField] private float min = 1f;
    [SerializeField] private float max = 1f;

    private void Start()
    {
        _mat = image.material;

        Set(min);
        DOTween.To(Get, Set, max, duration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    private float Get()
    {
        Vector4 value = _mat.GetVector(_property);
        return value.x;
    }
    
    private void Set(float value) => _mat.SetVector(_property, new(value, value));
}
