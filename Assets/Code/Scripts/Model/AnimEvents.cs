using System;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    public Action<string> OnAnimEvent { get; set; }

    public void AnimEvent(string eventName)
    {
        if (eventName.StartsWith("SFX:"))
        {
            string[] split = eventName.Split(':');

            SoundManager.PlaySound(split[1]);
        }

        OnAnimEvent?.Invoke(eventName);
    }
}
