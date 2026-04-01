using System;
using System.Collections.Generic;
using UnityEngine;

[SingletonMode(true)]
public class MusicManager : Singleton<MusicManager>
{
    private static Dictionary<string, MusicPlayer> _nameToMusics = new();

    private static MusicPlayer[] _musics;

    protected override void Awake()
    {
        base.Awake();

        _musics = GetComponentsInChildren<MusicPlayer>(true);

        foreach (MusicPlayer musicPlayer in _musics)
        {
            _nameToMusics.Add(musicPlayer.SongName.ToLowerInvariant(), musicPlayer);
        }
    }

    public static float GetTime(string songName)
    {
        if (_nameToMusics.TryGetValue(songName.ToLowerInvariant(), out MusicPlayer music))
        {
            return music.Time;
        }

        return 0;
    }

    public static void StopAll(float fadeTime = 0)
    {
        foreach (MusicPlayer musicPlayer in _musics)
        {
            musicPlayer.StopLocal(fadeTime);
        }
    }

    public static void Play(string songName, float fadeTime = 0, float startTime = 0)
    {
        if (_nameToMusics.TryGetValue(songName.ToLowerInvariant(), out MusicPlayer music))
        {
            music.PlayLocal(fadeTime, startTime);
        }
    }

    public static void Stop(string songName, float fadeTime = 0)
    {
        if (_nameToMusics.TryGetValue(songName.ToLowerInvariant(), out MusicPlayer music))
        {
            music.StopLocal(fadeTime);
        }
    }

    public static void Outro(string songName)
    {
        if (_nameToMusics.TryGetValue(songName.ToLowerInvariant(), out MusicPlayer music))
        {
            music.OutroLocal();
        }
    }

    public static void Load(string songName)
    {
        if (_nameToMusics.TryGetValue(songName.ToLowerInvariant(), out MusicPlayer music))
        {
            music.LoadLocal();
        }
    }
}
