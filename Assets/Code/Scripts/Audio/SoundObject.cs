using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;

/// <summary>
/// Handles playing <see cref="AudioClip"/>s sent from the <see cref="SoundManager"/>.
/// </summary>
public class SoundObject : MonoBehaviour
{
    /// <summary>
    /// The pool that this <see cref="SoundObject"/> originates from.
    /// </summary>
    public ObjectPool<SoundObject> Pool { get; set; }

    /// <summary>
    /// The <see cref="AudioSource"/> that's playing the <see cref="AudioClip"/> on this <see cref="SoundObject"/>.
    /// </summary>
    public AudioSource Source => source;
    [CacheComponent][SerializeField] private AudioSource source;

    /// <summary>
    /// The volume this <see cref="SoundObject"/> was assigned to have by the <see cref="SoundManager"/> when <see cref="SoundManager.PlaySound(Sound)"/> is used.
    /// </summary>
    public float TargetVolume { get; set; }

    /// <summary>
    /// The pitch this <see cref="SoundObject"/> was assigned to have by the <see cref="SoundManager"/> when <see cref="SoundManager.PlaySound(Sound)"/> is used.
    /// </summary>
    public float TargetPitch { get; set; }

    /// <summary>
    /// Is true if the <see cref="Source"/> is playing currently.
    /// </summary>
    public bool IsPlaying => source.isPlaying;

    /// <summary>
    /// Gets and sets the <see cref="AudioSource.ignoreListenerPause"/> from the <see cref="Source"/>.
    /// </summary>
    public bool IgnoreAudioListenerPause { get => Source.ignoreListenerPause; set => Source.ignoreListenerPause = value; }

    /// <summary>
    /// Gets and sets the <see cref="AudioSource.loop"/> from the <see cref="Source"/>.
    /// </summary>
    public bool Loop { get => source.loop; set => source.loop = value; }

    /// <summary>
    /// Gets and sets the <see cref="AudioSource.volume"/> from the <see cref="Source"/>. <para/>
    /// Note that settings this value will have the given value always be multiplied by <see cref="TargetVolume"/>.
    /// Setting this value will interrupt any volume <see cref="Tween"/> on this object. Use <see cref="IsTweeningVolume"/> to know if this will be the case.
    /// </summary>
    public float Volume
    {
        get => source.volume;
        set
        {
            StopVolumeTween();

            source.volume = value * TargetVolume;
        }
    }

    /// <summary>
    /// Gets and sets the <see cref="AudioSource.pitch"/> from the <see cref="Source"/>. <para/>
    /// Note that settings this value will have the given value always be multiplied by <see cref="TargetPitch"/>.
    /// Setting this value will interrupt any pitch <see cref="Tween"/> on this object. Use <see cref="IsTweeningPitch"/> to know if this will be the case.
    /// </summary>
    public float Pitch
    {
        get => source.pitch;
        set
        {
            StopPitchTween();

            source.pitch = value * TargetPitch;
        }
    }

    /// <summary>
    /// Returns true if the volume on this <see cref="SoundObject"/> is being tweened. <para/>
    /// Use any of the <see cref="TweenVolume"/> methods to start tweening the volume, making this set to true. <para/>
    /// Use <see cref="StopVolumeTween"/> method to stop any volume <see cref="Tween"/>, making this set to false.
    /// </summary>
    public bool IsTweeningVolume => _currentVolumeTween != null;

    /// <summary>
    /// The volume <see cref="Tween"/> that modifies the volume of this <see cref="SoundObject"/>. <para/>
    /// Will be null if <see cref="IsTweeningVolume"/> is false.
    /// </summary>
    public Tween CurrentVolumeTween => _currentPitchTween;

    /// <summary>
    /// Returns true if the pitch on this <see cref="SoundObject"/> is being tweened. <para/>
    /// Use the <see cref="TweenPitch"/> method to start tweening the pitch, making this set to true. <para/>
    /// Use <see cref="StopPitchTween"/> method to stop any pitch <see cref="Tween"/>, making this set to false.
    /// </summary>
    public bool IsTweeningPitch => _currentVolumeTween != null;

    /// <summary>
    /// The pitch <see cref="Tween"/> that modifies the pitch of this <see cref="SoundObject"/>. <para/>
    /// Will be null if <see cref="IsTweeningPitch"/> is false.
    /// </summary>
    public Tween CurrentPitchTween => _currentPitchTween;

    /// <summary>
    /// An action that is triggered when this <see cref="SoundObject"/> is released back into the pool. <para/>
    /// The <see cref="bool"/> value will be true if the <see cref="Stop(float, bool)"/> method was used with "doCallback" set to true.
    /// </summary>
    public Action<bool> OnRelease { get; set; }

    private Coroutine _coroutine;

    private WaitWhile _waitPredicate = null;

    private Tween _currentVolumeTween = null;
    private Tween _currentPitchTween = null;

    private bool _isReleased;

    /// <summary>
    /// Makes this <see cref="SoundObject"/> play its <see cref="AudioSource"/>. Do NOT use this method, use <see cref="SoundManager.PlaySound(Sound)"/> or any variation instead.
    /// </summary>
    public void Play()
    {
        StopVolumeTween();

        source.Play();

        OnRelease = null;

        _isReleased = false;

        StopCoroutine();
        _coroutine = StartCoroutine(Coroutine());
    }

    private void StopCoroutine()
    {
        if (_coroutine == null)
        {
            return;
        }

        StopCoroutine(_coroutine);

        _coroutine = null;
    }

    private bool WaitPredicate() => IsPlaying || (!Source.ignoreListenerPause && AudioListener.pause);

    private IEnumerator Coroutine()
    {
        // Wait while the source is playing
        if (_waitPredicate == null)
        {
            _waitPredicate = new WaitWhile(WaitPredicate);
        }

        yield return _waitPredicate;

        // Only kill once the source is finished and do the callback
        source.DOKill();
        Release(false);
    }

    #region Tweening Volume
    /// <summary>
    /// Stops and kills any active volume <see cref="Tween"/> from being finished.
    /// </summary>
    /// <returns>The <see cref="Tween"/> that was stopped and killed. Will be null if no <see cref="Tween"/> was sucessfully stopped.</returns>
    public Tween StopVolumeTween()
    {
        if (!IsTweeningVolume)
        {
            return null;
        }

        Tween oldTween = _currentVolumeTween;

        _currentVolumeTween.Kill();
        _currentVolumeTween = null;

        return oldTween;
    }

    /// <summary>
    /// Starts a Fade In <see cref="Tween"/> and calls the <paramref name="onComplete"/> <see cref="Action"/> when done.  <para/>
    /// Note that the volume is being tweened to the <see cref="TargetVolume"/>, not 1.
    /// </summary>
    /// <param name="fadeTime">The time it takes for this <see cref="SoundObject"/> to Fade In. Set to 0 or below for the volume to instantly be set.</param>
    /// <param name="onComplete">A callback that is called when the Fade In is done.</param>
    /// <returns>The <see cref="Tween"/> that controls the Fade In. Will be null if <paramref name="fadeTime"/> is less or equal to 0.</returns>
    public Tween FadeIn(float fadeTime, Action onComplete = null)
    {
        return TweenVolume(1, fadeTime, onComplete);
    }

    /// <summary>
    /// Starts a Fade Out <see cref="Tween"/> and calls the <paramref name="onComplete"/> <see cref="Action"/> when done. 
    /// </summary>
    /// <param name="fadeTime">The time it takes for this <see cref="SoundObject"/> to Fade Out. Set to 0 or below for the volume to instantly be set.</param>
    /// <param name="onComplete">A callback that is called when the Fade Out is done.</param>
    /// <returns>The <see cref="Tween"/> that controls the Fade Out. Will be null if <paramref name="fadeTime"/> is less or equal to 0.</returns>
    public Tween FadeOut(float fadeTime, Action onComplete = null)
    {
        return TweenVolume(0, fadeTime, onComplete);
    }

    /// <summary>
    /// Starts a volume <see cref="Tween"/> to the specified <paramref name="endVolume"/> and calls the <paramref name="onComplete"/> <see cref="Action"/> when done. <para/>
    /// Note that <paramref name="endVolume"/> will always be multiplied by <see cref="TargetVolume"/> if <paramref name="multiplyByTargetVolume"/> is left true.
    /// </summary>
    /// <param name="endVolume">The volume that this <see cref="SoundObject"/> will end on after the <see cref="Tween"/> is finished. This value will always be multiplied by <see cref="TargetPitch"/> if <paramref name="multiplyByTargetVolume"/> is left true.</param>
    /// <param name="fadeTime">The time it takes for this <see cref="SoundObject"/> to reach <paramref name="endVolume"/>. Set to 0 or below for the volume to instantly be set.</param>
    /// <param name="onComplete">A callback that is called when the <see cref="Tween"/> is done.</param>
    /// <param name="multiplyByTargetVolume">Wether or not <paramref name="endVolume"/> should be multiplied by <see cref="TargetVolume"/>.</param>
    /// <returns>The <see cref="Tween"/> that controls the volume. Will be null if <paramref name="fadeTime"/> is less or equal to 0.</returns>
    public Tween TweenVolume(float endVolume, float fadeTime, Action onComplete = null, bool multiplyByTargetVolume = true)
    {
        if (multiplyByTargetVolume)
        {
            endVolume *= TargetVolume;
        }

        StopVolumeTween();

        // Instant completion
        if (fadeTime <= 0)
        {
            source.volume = endVolume;
            onComplete?.Invoke();
            return null;
        }

        // Gradual fade
        _currentVolumeTween = source.DOFade(endVolume, fadeTime);

        _currentVolumeTween.SetUpdate(source.ignoreListenerPause);

        _currentVolumeTween.onComplete = () =>
        {
            onComplete?.Invoke();
            _currentVolumeTween = null;
        };

        return _currentVolumeTween;
    }

    /// <summary>
    /// Starts a Fade Out <see cref="Tween"/> and stops this <see cref="SoundObject"/> from playing when the <see cref="Tween"/> is done. 
    /// </summary>
    /// <param name="fadeTime">The time it takes for this <see cref="SoundObject"/> to stop playing. Set to 0 or below for the sound to instantly stop.</param>
    /// <param name="doCallback">Wether or not this <see cref="SoundObject"/> should call <see cref="OnRelease"/> when stopped.</param>
    /// <returns>The <see cref="Tween"/> that controls the Fade Out. Will be null if <paramref name="fadeTime"/> is less or equal to 0.</returns>
    public Tween Stop(float fadeTime = 0, bool doCallback = true)
    {
        StopCoroutine();

        return FadeOut(fadeTime, () => InstantStop(doCallback));
    }
    #endregion

    #region Tweening Pitch
    /// <summary>
    /// Stops and kills any active pitch <see cref="Tween"/> from being finished.
    /// </summary>
    /// <returns>The <see cref="Tween"/> that was stopped and killed. Will be null if no <see cref="Tween"/> was sucessfully stopped.</returns>
    public Tween StopPitchTween()
    {
        if (!IsTweeningPitch)
        {
            return null;
        }

        Tween oldTween = _currentPitchTween;

        _currentPitchTween.Kill();
        _currentPitchTween = null;

        return oldTween;
    }

    /// <summary>
    /// Starts a pitch <see cref="Tween"/> to the specified <paramref name="endPitch"/> and calls the <paramref name="onComplete"/> <see cref="Action"/> when done. <para/>
    /// Note that <paramref name="endPitch"/> will always be multiplied by <see cref="TargetPitch"/> if <paramref name="multiplyByTargetPitch"/> is left true.
    /// </summary>
    /// <param name="endPitch">The pitch that this <see cref="SoundObject"/> will end on after the <see cref="Tween"/> is finished. This value will always be multiplied by <see cref="TargetPitch"/> if <paramref name="multiplyByTargetPitch"/> is left true.</param>
    /// <param name="fadeTime">The time it takes for this <see cref="SoundObject"/> to reach <paramref name="endVolume"/>. Set to 0 or below for the volume to instantly be set.</param>
    /// <param name="onComplete">A callback that is called when the <see cref="Tween"/> is done.</param>
    /// <param name="multiplyByTargetPitch">Wether or not <paramref name="endPitch"/> should be multiplied by <see cref="TargetPitch"/>.</param>
    /// <returns>The <see cref="Tween"/> that controls the pitch. Will be null if <paramref name="fadeTime"/> is less or equal to 0.</returns>
    public Tween TweenPitch(float endPitch, float fadeTime, Action onComplete = null, bool multiplyByTargetPitch = true)
    {
        if (multiplyByTargetPitch)
        {
            endPitch *= TargetPitch;
        }

        StopPitchTween();

        // Instant completion
        if (fadeTime <= 0)
        {
            source.pitch = endPitch;
            onComplete?.Invoke();
            return null;
        }

        // Gradual fade
        _currentPitchTween = source.DOPitch(endPitch, fadeTime);

        _currentPitchTween.SetUpdate(source.ignoreListenerPause);

        _currentPitchTween.onComplete = () =>
        {
            onComplete?.Invoke();
            _currentPitchTween = null;
        };

        return _currentPitchTween;
    }
    #endregion

    private void InstantStop(bool doCallback = true)
    {
        StopVolumeTween();

        source.Stop();

        Release(true, doCallback);
    }

    private void Release(bool isStopMethod, bool doCallback = true)
    {
        if (_isReleased)
        {
            return;
        }

        _isReleased = true;

        StopCoroutine();

        Pool.Release(this);

        if (doCallback)
        {
            OnRelease?.Invoke(isStopMethod);
        }
    }
}