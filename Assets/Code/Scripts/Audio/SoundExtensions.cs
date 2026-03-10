using System;
using UnityEngine;

public static class SoundExtensions
{
    public static void LoadAll(this Sound[] sounds)
    {
        foreach (Sound sound in sounds)
        {
            sound.LoadAudioClips();
        }
    }
    public static void UnloadAll(this Sound[] sounds)
    {
        foreach (Sound sound in sounds)
        {
            sound.UnloadAudioClips();
        }
    }

    public static void PlayAll(this Sound[] sounds)
    {
        PlayAll(sounds, null);
    }

    public static void PlayAll(this Sound[] sounds, float pitch)
    {
        PlayAll(sounds, pitch, null);
    }

    public static void PlayAll(this Sound[] sounds, Action<SoundObject> beforePlayCallback)
    {
        foreach (Sound sound in sounds)
        {
            SoundManager.PlaySound(sound, beforePlayCallback);
        }
    }

    public static void PlayAll(this Sound[] sounds, float pitch, Action<SoundObject> beforePlayCallback = null)
    {
        foreach (Sound sound in sounds)
        {
            SoundManager.PlaySound(sound, pitch, beforePlayCallback);
        }
    }

    public static void PlayAll(this Sound[] sounds, float pitch, float volume, Action<SoundObject> beforePlayCallback = null)
    {
        foreach (Sound sound in sounds)
        {
            SoundManager.PlaySound(sound, pitch, volume, beforePlayCallback);
        }
    }

    public static void PlayAll(this Sound[] sounds, Vector3 position, Vector2 range, bool playInBothEars = SoundManager.PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        foreach (Sound sound in sounds)
        {
            SoundManager.PlaySound(sound, position, range, playInBothEars, beforePlayCallback);
        }
    }

    public static void PlayAll(this Sound[] sounds, Vector3 position, float pitch, Vector2 range, bool playInBothEars = SoundManager.PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        foreach (Sound sound in sounds)
        {
            SoundManager.PlaySound(sound, position, pitch, range, playInBothEars, beforePlayCallback);
        }
    }

    public static void PlayAll(this Sound[] sounds, Vector3 position, float pitch, float volume, Vector2 range, bool playInBothEars = SoundManager.PLAY_IN_BOTH_EARS_DEFAULT, Action<SoundObject> beforePlayCallback = null)
    {
        foreach (Sound sound in sounds)
        {
            SoundManager.PlaySound(sound, position, pitch, volume, range, playInBothEars, beforePlayCallback);
        }
    }
}