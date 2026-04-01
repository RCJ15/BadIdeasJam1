using System;
using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    private AnimEvents _animEvents;

    private void Awake()
    {
        _animEvents = GetComponent<AnimEvents>();
        _animEvents.OnAnimEvent += OnAnimEvent;
    }

    private void OnDestroy()
    {
        if (_animEvents != null)
        {
            _animEvents.OnAnimEvent -= OnAnimEvent;
        }
    }

    private void OnAnimEvent(string obj)
    {
        if (obj == "Die")
        {
            Destroy(gameObject);
        }
    }
}
