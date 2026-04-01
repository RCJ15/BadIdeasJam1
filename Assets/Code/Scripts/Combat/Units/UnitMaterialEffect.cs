using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitMaterialEffect : MonoBehaviour
{
    private Renderer[] _renderers;
    private int _renderersLength;
    private Material[][] _originalMaterials;
    private Material[][] _hurtMaterials;
    private Material[][] _healMaterials;
    private Coroutine[] _hurtCoroutines;

    private GlobalUnitSettings unitSettings;

    private void Awake()
    {
        List<Renderer> renderers = new();
        foreach (var renderer in GetComponentsInChildren<Renderer>(true))
        {
            Type type = renderer.GetType();
            if (type != typeof(MeshRenderer) && type != typeof(SpriteRenderer) && type != typeof(LineRenderer) && type != typeof(SkinnedMeshRenderer))
            {
                continue;
            }

            renderers.Add(renderer);
        }

        _renderers = renderers.ToArray();
        _renderersLength = _renderers.Length;
    }

    private void Start()
    {
        unitSettings = GlobalUnitSettings.Instance;

        _originalMaterials = new Material[_renderersLength][];
        _hurtMaterials = new Material[_renderersLength][];
        _healMaterials = new Material[_renderersLength][];
        _hurtCoroutines = new Coroutine[_renderersLength];

        for (int i = 0; i < _renderersLength; i++)
        {
            Renderer renderers = _renderers[i];

            if (renderers == null) continue;

            int materialsLength = renderers.materials.Length;
            _originalMaterials[i] = new Material[materialsLength];

            for (int j = 0; j < materialsLength; j++)
            {
                _originalMaterials[i][j] = renderers.materials[j];
            }

            _hurtMaterials[i] = new Material[materialsLength];
            _healMaterials[i] = new Material[materialsLength];

            for (int j = 0; j < materialsLength; j++)
            {
                _hurtMaterials[i][j] = unitSettings.HurtMaterial;
                _healMaterials[i][j] = unitSettings.HealMaterial;
            }
        }
    }

    public void HurtEffect()
    {
        DoEffect(_hurtMaterials);
    }

    public void HealEffect()
    {
        DoEffect(_healMaterials);
    }

    private void DoEffect(Material[][] materials)
    {
        foreach (Coroutine coroutine in _hurtCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }

        for (int i = 0; i < _renderersLength; i++)
        {
            _hurtCoroutines[i] = StartCoroutine(DoAnim(i, materials));
        }
    }

    private IEnumerator DoAnim(int i, Material[][] materials)
    {
        Renderer mr = _renderers[i];

        if (mr == null) yield break;

        mr.materials = materials[i];

        yield return new WaitForSeconds(unitSettings.MaterialEffectDuration);

        mr.materials = _originalMaterials[i];
    }
}
