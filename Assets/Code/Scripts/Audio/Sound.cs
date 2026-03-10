using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

/// <summary>
/// A <see cref="ScriptableObject"/> representing a sound to be used in the <see cref="SoundManager"/>.
/// </summary>
public class Sound : ScriptableObject
{
    public bool AudioDataLoaded
    {
        get
        {
            if (!_audioDataLoaded.HasValue)
            {
                bool allClipsLoaded = true;

                foreach (AudioClip clip in clips)
                {
                    if (clip.preloadAudioData)
                    {
                        continue;
                    }

                    allClipsLoaded = false;
                    break;
                }

                _audioDataLoaded = allClipsLoaded;
            }

            return _audioDataLoaded.Value;
        }
    }
    private bool? _audioDataLoaded = null;

    /// <summary>
    /// This <see cref="Sound"/>s <see cref="SoundType"/>.
    /// </summary>
    public SoundType Type { get => type; set => type = value; }
    [SerializeField] private SoundType type;

    /// <summary>
    /// The first <see cref="AudioClip"/> in the <see cref="Clips"/> array. This should be used if this <see cref="Sound"/>s <see cref="SoundType"/> is single.
    /// </summary>
    public AudioClip Clip => clips[0];
    /// <summary>
    /// An array of every <see cref="AudioClip"/> in this <see cref="Sound"/>. Will only contain 1 <see cref="AudioClip"/> if this <see cref="Sound"/>s <see cref="SoundType"/> is single.
    /// </summary>
    public AudioClip[] Clips { get => clips; set => clips = value; }
    [SerializeField] private AudioClip[] clips;

    /// <summary>
    /// How many <see cref="AudioClip"/>s that are in the <see cref="Clips"/> array.
    /// </summary>
    public int ClipsLength
    {
        get
        {
            if (!_clipsLength.HasValue)
            {
                _clipsLength = clips.Length;
            }

            return _clipsLength.Value;
        }
    }
    private int? _clipsLength = null;

    /// <summary>
    /// This value is multipled by the volume that is given when playing this <see cref="Sound"/>.
    /// </summary>
    public float Volume => volume;
    [Range(0, 1)][SerializeField] private float volume = 1;
    /// <summary>
    /// This value is added on top of the pitch that is added when playing this <see cref="Sound"/>.
    /// </summary>
    public float Pitch => pitch;
    [Range(0, 3)][SerializeField] private float pitch = 1;

    /// <summary>
    /// The <see cref="AudioMixerGroup"/> that this specific <see cref="Sound"/> should play on. <para/>
    /// Will be null if this <see cref="Sound"/> should play on the default sound effects mixer group
    /// </summary>
    public AudioMixerGroup MixerGroupOverride => mixerGroupOverride;
    [SerializeField] private AudioMixerGroup mixerGroupOverride;

    /// <summary>
    /// Returns a <see cref="AudioClip"/> from this <see cref="Sound"/>. <para/>
    /// The <see cref="AudioClip"/> that's returned is based on what <see cref="Type"/> of <see cref="Sound"/> this is. <para/>
    /// Will return null if this <see cref="Sound"/> has no clips at all.
    /// </summary>
    public AudioClip GetClip()
    {
        // No clips :(
        if (ClipsLength <= 0)
        {
            return null;
        }

        switch (type)
        {
            // Single - The first sound in the array
            case SoundType.Single:
                return Clip;

            // Group - Random sound in the array
            case SoundType.Group:
                return Clips[Random.Range(0, ClipsLength)];

            // No matching type
            default:
                return null;
        }
    }

    /// <summary>
    /// Uses <see cref="AudioClip.LoadAudioData"/> on all clips in the <see cref="Clips"/> array.
    /// </summary>
    public void LoadAudioClips()
    {
        if (AudioDataLoaded)
        {
            return;
        }

        foreach (AudioClip clip in clips)
        {
            if (clip == null || clip.preloadAudioData)
            {
                continue;
            }

            clip.LoadAudioData();
        }

        _audioDataLoaded = true;
    }

    /// <summary>
    /// Uses <see cref="AudioClip.UnloadAudioData"/> on all clips in the <see cref="Clips"/> array.
    /// </summary>
    public void UnloadAudioClips()
    {
        if (!AudioDataLoaded)
        {
            return;
        }

        foreach (AudioClip clip in clips)
        {
            if (clip == null || clip.preloadAudioData)
            {
                continue;
            }

            clip.UnloadAudioData();
        }

        _audioDataLoaded = false;
    }

    #region Play Methods
    /// <summary>
    /// Plays this sound using <see cref="SoundManager.PlaySound(Sound, Action{SoundObject})"/>. <para/>
    /// For more detailed documentation, look at summary of the above mentioned method.
    /// </summary>
    public SoundObject Play()
    {
        return Play(null);
    }

    /// <summary>
    /// Plays this sound using <see cref="SoundManager.PlaySound(Sound, float, Action{SoundObject})"/>. <para/>
    /// For more detailed documentation, look at summary of the above mentioned method.
    /// </summary>
    public SoundObject Play(float pitch)
    {
        return Play(pitch, null);
    }

    /// <summary>
    /// Plays this sound using <see cref="SoundManager.PlaySound(Sound, Action{SoundObject})"/>. <para/>
    /// For more detailed documentation, look at summary of the above mentioned method.
    /// </summary>
    public SoundObject Play(Action<SoundObject> beforePlayCallback)
    {
        return SoundManager.PlaySound(this, beforePlayCallback);
    }

    /// <summary>
    /// Plays this sound using <see cref="SoundManager.PlaySound(Sound, float, Action{SoundObject})"/>. <para/>
    /// For more detailed documentation, look at summary of the above mentioned method.
    /// </summary>
    public SoundObject Play(float pitch, Action<SoundObject> beforePlayCallback = null)
    {
        return SoundManager.PlaySound(this, pitch, beforePlayCallback);
    }

    /// <summary>
    /// Plays this sound using <see cref="SoundManager.PlaySound(Sound, float, float, Action{SoundObject})"/>. <para/>
    /// For more detailed documentation, look at summary of the above mentioned method.
    /// </summary>
    public SoundObject Play(float pitch, float volume, Action<SoundObject> beforePlayCallback = null)
    {
        return SoundManager.PlaySound(this, pitch, volume, beforePlayCallback);
    }

    /// <summary>
    /// Plays this sound using <see cref="SoundManager.PlaySound(Sound, Vector3, Vector2, bool, Action{SoundObject})"/>. <para/>
    /// For more detailed documentation, look at summary of the above mentioned method.
    /// </summary>
    public SoundObject Play(Vector3 position, Vector2 range, bool playInBothEars = SoundManager.PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return SoundManager.PlaySound(this, position, range, playInBothEars, beforePlayCallback);
    }

    /// <summary>
    /// Plays this sound using <see cref="SoundManager.PlaySound(Sound, Vector3, float, Vector2, bool, Action{SoundObject})"/>. <para/>
    /// For more detailed documentation, look at summary of the above mentioned method.
    /// </summary>
    public SoundObject Play(Vector3 position, float pitch, Vector2 range, bool playInBothEars = SoundManager.PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return SoundManager.PlaySound(this, position, pitch, range, playInBothEars, beforePlayCallback);
    }

    /// <summary>
    /// Plays this sound using <see cref="SoundManager.PlaySound(Sound, Vector3, float, float, Vector2, bool, Action{SoundObject})"/>. <para/>
    /// For more detailed documentation, look at summary of the above mentioned method.
    /// </summary>
    public SoundObject Play(Vector3 position, float pitch, float volume, Vector2 range, bool playInBothEars = SoundManager.PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return SoundManager.PlaySound(this, position, pitch, volume, range, playInBothEars, beforePlayCallback);
    }
    #endregion

    /// <summary>
    /// An enum that represents the different types a <see cref="Sound"/> can have.
    /// </summary>
    public enum SoundType
    {
        /// <summary>
        /// A <see cref="Sound"/> that only contains 1 <see cref="AudioClip"/>.
        /// </summary>
        Single,
        /// <summary>
        /// A <see cref="Sound"/> that contains multiple <see cref="AudioClip"/>s and plays them at random.
        /// </summary>
        Group,
    }
}