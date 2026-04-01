using UnityEngine;

public class WinZone : MonoBehaviour
{
    [SerializeField] private Vector3 size;

    private PlayerOverworld _player;
    private bool _inZone;

    private void Start()
    {
        _player = PlayerOverworld.Instance;
    }
    private void Update()
    {
        if (_inZone) return;

        Vector3 playerPos = _player.transform.position;

        Vector3 min = transform.position - size / 2f;
        Vector3 max = transform.position + size / 2f;

        if (playerPos.x >= min.x && playerPos.y >= min.y && playerPos.z >= min.z
            &&
            playerPos.x <= max.x && playerPos.y <= max.y && playerPos.z <= max.z)
        {
            _inZone = true;
            _player.CanMove = false;

            // WIN!!!
            Debug.Log("PLAYER WINS!!");

            MusicPlayer.Stop("overworld", 1);

            SceneTransition.Goto(5);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(transform.position, size);
    }
}
