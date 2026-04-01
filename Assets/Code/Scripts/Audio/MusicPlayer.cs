using DG.Tweening;
using System;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [CacheComponent]
    [SerializeField] private AudioSource source;

    [Space]
    [SerializeField] private string songName;
    [SerializeField] private AudioClip intro;
    [SerializeField] private AudioClip loop;
    [SerializeField] private AudioClip outro;

    public string SongName => songName;

    private bool _playing = false;

    public float Time
    {
        get => source.time;
        set => source.time = value;
    }

    private void Awake()
    {
        source.volume = 0;
    }

    public void PlayLocal(float fadeTime = 0, float startTime = 0)
    {
        if (_playing) return;
        _playing = true;

        source.DOKill();

        if (fadeTime <= 0)
        {
            source.volume = 1;
        }
        else
        {
            source.DOFade(1, fadeTime);
        }

        source.clip = loop;
        source.loop = true;

        if (startTime != 0)
        {
            source.time = startTime;
            source.Play();
            return;
        }

        source.PlayOneShot(intro);

        if (loop != null)
        {
            source.PlayScheduled(AudioSettings.dspTime + (double)intro.length);
        }
    }

    public void StopLocal(float fadeTime = 0)
    {
        if (!_playing) return;
        _playing = false;

        source.DOKill();

        if(fadeTime <= 0)
        {
            source.volume = 0;
            source.Stop();

            return;
        }

        source.DOFade(0, fadeTime).onComplete = source.Stop;
    }

    public void OutroLocal()
    {
        _playing = false;
        source.volume = 1;
        source.Stop();
        if (outro != null)
        {
            source.PlayOneShot(outro);
        }
    }


    public void LoadLocal()
    {
        if (intro != null) intro.LoadAudioData();
        if (loop != null) loop.LoadAudioData();
        if (outro != null) outro.LoadAudioData();
    }

    public static void Play(string songName, float fadeTime = 0, float startTime = 0) => MusicManager.Play(songName, fadeTime, startTime);
    public static void Stop(string songName, float fadeTime = 0) => MusicManager.Stop(songName, fadeTime);
    public static void Outro(string songName) => MusicManager.Outro(songName);
    public static void StopAll(float fadeTime = 0) => MusicManager.StopAll(fadeTime);

    public static void Load(string songName) => MusicManager.Load(songName);
}
