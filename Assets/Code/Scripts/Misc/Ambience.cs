using DG.Tweening;
using UnityEngine;

[SingletonMode(true)]
public class Ambience : Singleton<Ambience>
{
    [CacheComponent]
    [SerializeField] private AudioSource source;

    [Space]
    [SerializeField] private float volume;

    protected override void Awake()
    {
        base.Awake();

        source.volume = 0;

        source.DOFade(volume, 10f);
    }
}
