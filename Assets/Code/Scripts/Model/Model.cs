using System;
using UnityEngine;

public class Model : MonoBehaviour
{
    [CacheComponent]
    [SerializeField] private Animator anim;
    private AnimEvents _animEvents;

    public bool HasAnim => anim != null;

    private void Start()
    {
        _animEvents = GetComponentInChildren<AnimEvents>(true);

        if (_animEvents != null)
        {
            _animEvents.OnAnimEvent += OnAnimEvent;
        }
    }

    private void OnDestroy()
    {
        if (_animEvents != null)
        {
            _animEvents.OnAnimEvent -= OnAnimEvent;
        }
    }

    private void OnAnimEvent(string evt)
    {
        if (evt == "UpdateRig")
        {
            if (TryGetComponent(out LowFPSRig rig))
            {
                rig.ForceUpdate();
            }
        }
    }

    public void PlayAnim(string name)
    {
        if (!HasAnim) return;

        anim.Play(name);
    }

    public void SetFloat(string name, float value)
    {
        if (!HasAnim) return;

        anim.SetFloat(name, value);
    }
    public float GetFloat(string name) => HasAnim ? anim.GetFloat(name) : 0;

    public void SetInt(string name, int value)
    {
        if (!HasAnim) return;

        anim?.SetInteger(name, value);
    }
    public int GetInt(string name) => HasAnim ? anim.GetInteger(name) : 0;

    public void SetBool(string name, bool value)
    {
        if (!HasAnim) return;

        anim?.SetBool(name, value);
    }
    public bool GetBool(string name) => HasAnim ? anim.GetBool(name) : false;

    public void SetTrigger(string name)
    {
        if (!HasAnim) return;

        anim?.SetTrigger(name);
    }
}
