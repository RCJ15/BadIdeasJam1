using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Has only 1 method that adds a <see cref="MenuItem"/> which creates a <see cref="Sound"/>.
/// </summary>
public static class SoundCreateMenuItem
{
    [MenuItem("Assets/Create/Audio/Sound", priority = 38)]
    public static void CreateSound()
    {
        Sound sound = ScriptableObject.CreateInstance<Sound>();

        string name = "New Sound";

        // Check if anything is selected
        if (Selection.objects.Length > 0)
        {
            // Create temp list
            List<AudioClip> clips = new List<AudioClip>();

            // Loop through all objects in the selection
            foreach (Object obj in Selection.objects)
            {
                // Ignore any objects that aren't of type audio clip
                AudioClip clip = obj as AudioClip;

                if (clip == null)
                {
                    continue;
                }

                // Add the clip to the list
                clips.Add(clip);
            }

            // Check if we got any audio clips selected
            int clipsCount = clips.Count;

            if (clipsCount > 0)
            {
                // More than one sound?
                if (clipsCount > 1)
                {
                    // Make it a sound group
                    sound.Type = Sound.SoundType.Group;
                }
                // Only one sound?
                else
                {
                    // Make it a single sound
                    sound.Type = Sound.SoundType.Single;
                }

                // Name is the first clip name but fancy
                name = FancyAutoNaming(clips[0].name);

                // Set sound clips
                sound.Clips = clips.ToArray();
            }
        }

        // Refleciton is used as the GetActiveFolderPath method on the ProjectWindowUtil type is private for some reason
        string activeFolderPath = typeof(ProjectWindowUtil).GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[0]).ToString();

        ProjectWindowUtil.CreateAsset(sound, activeFolderPath + "/" + name + ".asset");
    }


    private static string FancyAutoNaming(string name)
    {
        string newName = name;

        newName.Trim();

        newName.Replace('_', ' ');
        newName.Replace('-', ' ');
        newName.Replace('.', ' ');
        newName.Replace(':', ' ');
        newName.Replace(',', ' ');

        newName = AddSpacesToSentence(newName);

        newName = FirstLettersUpper(newName);

        // Remove numbers at the end
        int stepsGoneBack = 1;

        while (char.IsNumber(newName[newName.Length - stepsGoneBack]) && newName.Length - stepsGoneBack >= 0)
        {
            stepsGoneBack++;
        }

        string oldName = newName;
        newName = "";
        for (int i = 0; i < oldName.Length - stepsGoneBack + 1; i++)
        {
            newName += oldName[i];
        }

        return newName.Trim();
    }

    private static string AddSpacesToSentence(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        string newText = text[0].ToString();

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                newText += ' ';

            newText += text[i];
        }
        return newText;
    }

    /// <summary>
    /// Will return the given string except only the first letters are uppercase. Example: hEllO woRlD = Hello World.
    /// </summary>
    /// <param name="text">The text that will be converted</param>
    /// <param name="onlyFirstWord">If true, makes only the first words first letter uppercase and everything else lowercase</param>
    private static string FirstLettersUpper(string text, bool onlyFirstWord = false)
    {
        if (onlyFirstWord)
        {
            // Take the first letter and make it uppercase
            // Take the other letters and make them lowercase
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }
        else
        {
            string returnData = "";
            bool nextUpper = true;

            foreach (char c in text)
            {
                if (nextUpper)
                {
                    returnData += char.ToUpper(c);
                    nextUpper = false;
                }
                else
                {
                    returnData += char.ToLower(c);
                }

                // If the character was a space or period then the next character will be uppercase
                if (c.Equals(' ') || c.Equals('.'))
                {
                    nextUpper = true;
                }
            }

            return returnData;
        }
    }
}