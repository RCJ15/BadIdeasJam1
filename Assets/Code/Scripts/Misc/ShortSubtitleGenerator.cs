using TMPro;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using BrunoMikoski.TextJuicer;

// I AM TOO POOR FOR ANY GOOD EDITING SOFTWARE
public class ShortSubtitleGenerator : MonoBehaviour
{
    [CacheComponent]
    [SerializeField] private TMP_Text text;
    [CacheComponent]
    [SerializeField] private TMP_TextJuicer textJuicer;
    [CacheComponent]
    [SerializeField] private AudioSource source;

    [Space]
    [SerializeField] private TextAsset asset;

    private Subtitle[] _subtitles;
    private int _subtitlesLength;

    private int _index;

    private string _oldText;

    private void Awake()
    {
        string raw = asset.text;

        List<Subtitle> subtitles = new();

        string[] splits = raw.Split('\n');

        for (int i = 0; i < splits.Length; i++)
        {
            string split = splits[i];

            if (split.StartsWith("["))
            {
                string timeString = split.Substring(1, split.Length - 3);

                string[] colonSplit = timeString.Split(':');

                int hours = int.Parse(colonSplit[0]);
                int minutes = int.Parse(colonSplit[1]);
                float seconds = float.Parse(colonSplit[2], System.Globalization.NumberStyles.Any);

                float time = seconds + ((float)minutes * 60f) + ((float)hours * 60f * 60f);
                string text = splits[i + 1].Trim();

                subtitles.Add(new Subtitle(text, time));

                Debug.Log("Subtitle at " + time + " = " + text);
            }
        }

        _subtitles = subtitles.ToArray();
        _subtitlesLength = _subtitles.Length;
    }

    private IEnumerator Start()
    {
        source.clip.LoadAudioData();

        yield return new WaitUntil(() => source.clip.loadState == AudioDataLoadState.Loaded);

        yield return CoroutineUtility.GetWait(1);

        source.Play();
    }

    private void Update()
    {
        float time = source.time;

        string newText = "";

        foreach (Subtitle subtitle in _subtitles)
        {
            if (time < subtitle.Time) continue;

            newText = subtitle.Text;
        }

        if (_oldText != newText)
        {
            _oldText = newText;
            text.text = newText;
            text.ForceMeshUpdate(true);
            textJuicer.SetDirty();
        }
    }

    public class Subtitle
    {
        public string Text { get; private set; }
        public float Time { get; private set; }

        public Subtitle(string text, float time)
        {
            Text = text;
            Time = time;
        }
    }
}
