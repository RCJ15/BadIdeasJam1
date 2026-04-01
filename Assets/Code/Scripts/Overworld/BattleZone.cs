using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public abstract class BattleZone : MonoBehaviour
{
    [CacheComponent(DisableField = false)]
    [SerializeField] private OverworldCheckpoint checkpoint;

    [Space]
    [SerializeField] private bool isBoss;
    [SerializeField] private Vector3 size;
    [SerializeField] private float delay;

    public abstract int BattleIndex { get; }

    protected PlayerOverworld _player;
    protected GameCamera _camera;
    protected Transform _cameraTransform;

    private bool _inBattleZone;

    private void Awake()
    {
        if (PlayerOverworld.Checkpoint >= checkpoint.ID)
        {
            gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        _player = PlayerOverworld.Instance;
        _camera = GameCamera.Instance;
        _cameraTransform = _camera.CameraTransform;
    }

    private void Update()
    {
        if (_inBattleZone) return;

        Vector3 playerPos = _player.transform.position;

        Vector3 min = transform.position - size / 2f;
        Vector3 max = transform.position + size / 2f;

        if (playerPos.x >= min.x && playerPos.y >= min.y && playerPos.z >= min.z
            &&
            playerPos.x <= max.x && playerPos.y <= max.y && playerPos.z <= max.z)
        {
            _inBattleZone = true;
            _player.CanMove = false;

            StartCoroutine(Main());

            if (isBoss)
            {
                MusicPlayer.Load("boss");
            }
        }
    }

    protected IEnumerator Main()
    {
        yield return CoroutineUtility.GetWait(delay);

        yield return Coroutine();

        EnterBattle();
    }
    protected abstract IEnumerator Coroutine();

    protected void EnterBattle()
    {
        PlayerOverworld.Checkpoint = checkpoint.ID;
        OverworldManager.Instance.EnterBattle(BattleIndex, isBoss);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(transform.position, size);
    }
}
