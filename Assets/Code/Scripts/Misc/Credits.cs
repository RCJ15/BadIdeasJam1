using UnityEngine;

public class Credits : MonoBehaviour
{
    private void Start()
    {
        MusicPlayer.StopAll();
        MusicPlayer.Play("credits");
    }
}
