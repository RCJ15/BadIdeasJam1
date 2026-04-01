using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A <see cref="ScriptableObject"/> that contains an array of every single <see cref="Sound"/> in the entire project.
/// <summary>
public class SoundList : ScriptableObject
{
    public static SoundList Instance
    {
        get
        {
            if (_instanceCache == null)
            {
                _instanceCache = Resources.Load<SoundList>(nameof(SoundList));
            }

            return _instanceCache;
        }
    }
    private static SoundList _instanceCache;

    [SerializeField] private Sound[] sounds;

    /// <summary>
    /// An array of every single <see cref="Sound"/> in the entire project.
    /// </summary>
    public Sound[] Sounds => sounds;

    /// <summary>
    /// Returns a <see cref="Sound"/> with the given <paramref name="index"/> from the <see cref="Sounds"/> array.
    /// </summary>
    public Sound this[int index] => sounds[index];

    /// <summary>
    /// A dictionary where the key is the name of a <see cref="Sound"/> whilst the value is the <see cref="Sound"/> itself. <para/>
    /// Use <see cref="GetSoundFromName(string)"/> instead of using this dictionary directly.
    /// </summary>
    public Dictionary<string, Sound> NameToSoundDictionary
    {
        get
        {
            // Create the dictionary if it's null
            if (_nameToSoundDictionary == null)
            {
                _nameToSoundDictionary = new Dictionary<string, Sound>();

                // Populate dictionary
                foreach (Sound sound in sounds)
                {
                    if (sound == null)
                    {
                        continue;
                    }

                    _nameToSoundDictionary.Add(sound.name.ToLowerInvariant(), sound);
                }
            }

            return _nameToSoundDictionary;
        }
    }
    private Dictionary<string, Sound> _nameToSoundDictionary = null;

    /// <summary>
    /// Returns a <see cref="Sound"/> from it's <paramref name="name"/>.
    /// </summary>
    public Sound GetSoundFromName(string name)
    {
        // The SoundList hasn't loaded yet
        if (Instance == null)
        {
            return null;
        }

        // Name is null or empty
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        name = name.ToLowerInvariant();

        // Name is not present in the dictionary
        if (!NameToSoundDictionary.ContainsKey(name))
        {
            return null;
        }

        return NameToSoundDictionary[name];
    }
}