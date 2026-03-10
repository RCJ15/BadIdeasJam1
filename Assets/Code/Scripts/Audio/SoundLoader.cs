using UnityEngine;

/// <summary>
/// A simple script that will simply use <see cref="Sound.LoadAudioClips"/> on Awake and <see cref="Sound.UnloadAudioClips"/> OnDestroy on an array of <see cref="Sound"/>.
/// </summary>
public class SoundLoader : MonoBehaviour
{
    [SerializeField] private Sound[] sounds;

    [SerializeField] private bool unloadSounds = true;

    private void Awake()
    {
        foreach (Sound sound in sounds)
        {
            sound.LoadAudioClips();
        }
    }

    private void OnDestroy()
    {
        if (!unloadSounds)
        {
            return;
        }

        foreach (Sound sound in sounds)
        {
            sound.UnloadAudioClips();
        }
    }
}