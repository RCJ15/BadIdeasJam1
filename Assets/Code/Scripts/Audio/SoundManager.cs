using System;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.Audio;
using UnityEngine.Pool;

/// <summary>
/// This script is responsible for spawning <see cref="SoundObject"/>s that play <see cref="Sound"/>s courtesy of the <see cref="Audio.SoundList"/>. <para/>
/// Use any of the <see cref="PlaySound(Sound)"/> methods to play a sound!
/// </summary>
[SingletonMode(true)]
public class SoundManager : Singleton<SoundManager>
{
    /// <summary>
    /// The default <see cref="AudioMixerGroup"/> that every sound effect is played in. <para/>
    /// Some <see cref="Sound"/>s can have a <see cref="Sound.MixerGroupOverride"/> which would override this default.
    /// </summary>
    public AudioMixerGroup DefaultMixerGroup { get => defaultMixerGroup; set => defaultMixerGroup = value; }
    [SerializeField] private AudioMixerGroup defaultMixerGroup;

    /// <summary>
    /// This is simply just a reference to <see cref="SoundList.Instance"/>.
    /// </summary>
    public static SoundList SoundList => SoundList.Instance;

    private static ObjectPool<SoundObject> _pool = new ObjectPool<SoundObject>(PoolCreateFunc);

    private static SoundObject _template = null;

    private static SoundObject PoolCreateFunc()
    {
        if (_template == null)
        {
            _template = Resources.Load<SoundObject>(nameof(SoundObject));
        }

        SoundObject soundObj = Instantiate(_template);

        soundObj.gameObject.SetActive(true);

        soundObj.Pool = _pool;

        soundObj.transform.SetParent(Instance.transform);

        return soundObj;
    }

    /// <summary>
    /// The random pitch that every sound uses. Returns a random float between 0.8 and 1.2.
    /// </summary>
    public static float GetRandomPitch()
    {
        return Random.Range(0.8f, 1.2f);
    }

    /// <summary>
    /// Creates a gameobject that will play the given clip with the options inserted. The new object will then self destruct when the sound is finished playing.
    /// </summary>
    /// <param name="clip">The audio clip to play.</param>
    /// <param name="volume">The volume of the sound.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="mixerGroup">The audio mixer group that the sound will be played to.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="is3D">The spacial blend of the sound.<para/>
    /// True = is only heard in a certain area around <paramref name="position"/>. The radius of the area is controlled by the <paramref name="range"/> parameter.<para/>
    /// False = is heard anywhere regardless of <paramref name="position"/>.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.<para/>
    /// Only works if <paramref name="is3D"/> is true.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.<para/>
    /// Only works if <paramref name="is3D"/> is true.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    private static SoundObject PlaySoundClip(AudioClip clip, float volume, float pitch, AudioMixerGroup mixerGroup, Vector3 position, bool is3D, Vector2 range, bool playInBothEars = false, Action<SoundObject> beforePlayCallback = null)
    {
        // Return if the clip is null as we can't play something that's null
        if (clip == null)
        {
            return null;
        }

        // Get sound from the pool
        SoundObject obj = _pool.Get();

        // Setup sound
        obj.TargetVolume = volume;
        obj.Volume = 1;
        obj.TargetPitch = pitch;
        obj.Pitch = 1;

        obj.transform.position = position;

        AudioSource audio = obj.Source;

        audio.playOnAwake = false;
        audio.loop = false;

        audio.ignoreListenerPause = false;
        audio.ignoreListenerVolume = false;

        audio.bypassEffects = false;
        audio.bypassListenerEffects = false;
        audio.bypassReverbZones = false;

        audio.mute = false;
        audio.panStereo = 0;

        audio.dopplerLevel = 0;
        audio.clip = clip;
        audio.volume = volume;
        audio.pitch = pitch;
        audio.outputAudioMixerGroup = mixerGroup;
        audio.ignoreListenerPause = false;
        audio.spatialBlend = is3D ? 1 : 0;

        if (is3D)
        {
            audio.spread = playInBothEars ? 60 : 180;
            audio.minDistance = range.x;
            audio.maxDistance = range.y;

            //audio.SetupRealisticRolloff();
        }

        beforePlayCallback?.Invoke(obj);

        // Play the sound
        audio.time = 0;
        obj.Play();

        return obj;
    }

    /// <summary>
    /// Will play a <see cref="Sound"/> with customizable options.
    /// </summary>
    /// <param name="sound">The sound that is going to be played.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="is3D">The spacial blend of the sound.<para/>
    /// True = is only heard in a certain area around <paramref name="position"/>. The radius of the area is controlled by the <paramref name="range"/> parameter.<para/>
    /// False = is heard anywhere regardless of <paramref name="position"/>.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.<para/>
    /// Only works if <paramref name="is3D"/> is true.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.<para/>
    /// Only works if <paramref name="is3D"/> is true.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(Sound sound, Vector3 position, float pitch, float volume, bool is3D, Vector2 range, bool playInBothEars, Action<SoundObject> beforePlayCallback = null)
    {
        // If the sound == null then just don't play anything
        if (sound == null)
        {
            return null;
        }

        // If the SoundManager doesn't exist then return
        if (Instance == null || !Instance.gameObject.activeInHierarchy)
        {
            return null;
        }

        // Find the right audio clip
        AudioClip clip = sound.GetClip();

        // Setup clip
        volume *= sound.Volume;
        pitch += sound.Pitch - 1;

        AudioMixerGroup mixerGroup = sound.MixerGroupOverride == null ? Instance.defaultMixerGroup : sound.MixerGroupOverride;

        // Play the sound
        return PlaySoundClip(clip, volume, pitch, mixerGroup, position, is3D, range, playInBothEars, beforePlayCallback);
    }

    /// <summary>
    /// Will play a <see cref="Sound"/> with customizable options.
    /// </summary>
    /// <param name="name">The name of the sound.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="is3D">The spacial blend of the sound.<para/>
    /// True = is only heard in a certain area. The radius of the area is controlled by the <paramref name="range"/> parameter.<para/>
    /// False = is heard anywhere regardless of <paramref name="position"/>.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.<para/>
    /// Only works if <paramref name="is3D"/> is true.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.<para/>
    /// Only works if <paramref name="is3D"/> is true.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(string name, Vector3 position, float pitch, float volume, bool is3D, Vector2 range, bool playInBothEars, Action<SoundObject> beforePlayCallback = null)
    {
        // If the name == null or empty then just don't play anything
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        // Can't play sound if the Sound List hasn't been loaded yet
        if (SoundList == null)
        {
            return null;
        }

        // Get the sound from name
        Sound sound = SoundList.GetSoundFromName(name);

        // Play the sound
        return PlaySound(sound, position, pitch, volume, is3D, range, playInBothEars, beforePlayCallback);
    }

    #region 2D Sounds
    #region 2D Sound - Random Pitch
    /// <summary>
    /// Plays a 2D <see cref="Sound"/> with randomized pitch. The random pitch is between 0.8 and 1.2.
    /// </summary>
    /// <param name="name">The name of the sound.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(string name, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(name, Vector3.zero, GetRandomPitch(), 1, false, Vector2.zero, false, beforePlayCallback);
    }

    /// <summary>
    /// Plays a 2D <see cref="Sound"/> with randomized pitch. The random pitch is between 0.8 and 1.2.
    /// </summary>
    /// <param name="sound">The sound that is going to be played.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(Sound sound, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(sound, Vector3.zero, GetRandomPitch(), 1, false, Vector2.zero, false, beforePlayCallback);
    }
    #endregion

    #region 2D Sound - Customizable Pitch
    /// <summary>
    /// Plays a 2D <see cref="Sound"/> with customizable pitch.
    /// </summary>
    /// <param name="name">The name of the sound.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(string name, float pitch, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(name, Vector3.zero, pitch, 1, false, Vector2.zero, false, beforePlayCallback);
    }
    /// <summary>
    /// Plays a 2D <see cref="Sound"/> with customizable pitch.
    /// </summary>
    /// <param name="sound">The sound that is going to be played.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(Sound sound, float pitch, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(sound, Vector3.zero, pitch, 1, false, Vector2.zero, false, beforePlayCallback);
    }
    #endregion

    #region 2D Sound - Customizable Pitch & Customizable Volume
    /// <summary>
    /// Plays a 2D <see cref="Sound"/> with customizable pitch and customizable volume.
    /// </summary>
    /// <param name="name">The name of the sound.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="volume">The volume of the sound.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(string name, float pitch, float volume, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(name, Vector3.zero, pitch, volume, false, Vector2.zero, false, beforePlayCallback);
    }

    /// <summary>
    /// Plays a 2D <see cref="Sound"/> with customizable pitch and customizable volume.
    /// </summary>
    /// <param name="sound">The sound that is going to be played.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="volume">The volume of the sound.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(Sound sound, float pitch, float volume, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(sound, Vector3.zero, pitch, volume, false, Vector2.zero, false, beforePlayCallback);
    }
    #endregion
    #endregion

    #region 3D Sounds
    public const bool PLAY_IN_BOTH_EARS_DEFAULT = true;

    #region 3D Sound - Random Pitch
    /// <summary>
    /// Plays a 3D <see cref="Sound"/> with randomized pitch. The random pitch is between 0.8 and 1.2.
    /// </summary>
    /// <param name="name">The name of the sound.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(string name, Vector3 position, Vector2 range, bool playInBothEars = PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(name, position, GetRandomPitch(), 1, true, range, playInBothEars, beforePlayCallback);
    }
    /// <summary>
    /// Plays a 3D <see cref="Sound"/> with randomized pitch. The random pitch is between 0.8 and 1.2.
    /// </summary>
    /// <param name="sound">The sound that is going to be played.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(Sound sound, Vector3 position, Vector2 range, bool playInBothEars = PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(sound, position, GetRandomPitch(), 1, true, range, playInBothEars, beforePlayCallback);
    }
    #endregion

    #region 3D Sound - Customizable Pitch
    /// <summary>
    /// Plays a 3D sound with customizable pitch.
    /// </summary>
    /// <param name="name">The name of the sound.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(string name, Vector3 position, float pitch, Vector2 range, bool playInBothEars = PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(name, position, pitch, 1, true, range, playInBothEars, beforePlayCallback);
    }

    /// <summary>
    /// Plays a 3D sound with customizable pitch.
    /// </summary>
    /// <param name="sound">The sound that is going to be played.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(Sound sound, Vector3 position, float pitch, Vector2 range, bool playInBothEars = PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(sound, position, pitch, 1, true, range, playInBothEars, beforePlayCallback);
    }
    #endregion

    #region 3D Sound - Customizable Pitch & Customizable Volume
    /// <summary>
    /// Plays a 3D sound with customizable pitch and customizable volume.
    /// </summary>
    /// <param name="name">The name of the sound.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="volume">The volume of the sound.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(string name, Vector3 position, float pitch, float volume, Vector2 range, bool playInBothEars = PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(name, position, pitch, volume, true, range, playInBothEars, beforePlayCallback);
    }

    /// <summary>
    /// Plays a 3D sound with customizable pitch and customizable volume.
    /// </summary>
    /// <param name="sound">The sound that is going to be played.</param>
    /// <param name="position">Where the sound will be played.</param>
    /// <param name="pitch">The pitch of the sound.</param>
    /// <param name="volume">The volume of the sound.</param>
    /// <param name="range">How far the sound can be heard from.<para/>
    /// X = min distance. Y = max distance.</param>
    /// <param name="playInBothEars">If the sound should be able to play in both ears.</param>
    /// <param name="beforePlayCallback">A callback that is called just before Play() is called on the resulting <see cref="SoundObject"/>. Will not be called if the sound was played unsuccessfully.</param>
    /// <returns>A <see cref="SoundObject"/> that contain info about the sound. Will be null if the sound was played unsuccessfully.</returns>
    public static SoundObject PlaySound(Sound sound, Vector3 position, float pitch, float volume, Vector2 range, bool playInBothEars = PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        return PlaySound(sound, position, pitch, volume, true, range, playInBothEars, beforePlayCallback);
    }
    #endregion
    #endregion
}