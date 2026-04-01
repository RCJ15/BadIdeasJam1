using System;
using System.Collections;
using UnityEngine;

public class CombatManager : Singleton<CombatManager>
{
    private PlayerCombat _player;
    private GameCamera _camera;

    [SerializeField] private bool isBoss;

    protected override void Awake()
    {
        base.Awake();

        PlayerCombat.OnBeginPlayersTurn += OnBeginPlayersTurn;
        PlayerCombat.OnEndPlayersTurn += OnEndPlayersTurn;
    }

    private IEnumerator Start()
    {
        MusicPlayer.Play(isBoss ? "boss" : "battle");

        _player = PlayerCombat.Instance;
        _camera = GameCamera.Instance;

        float startOrthoSize = _camera.OrthoSize;
        _camera.OrthoSize /= 2f;
        _camera.ZoomCameraSize(startOrthoSize, 1.5f, DG.Tweening.Ease.OutExpo);

        yield return CoroutineUtility.GetWait(1f);

        if (isBoss)
        {
            BigText.Appear("BOSS LEVEL!");
            yield return CoroutineUtility.GetWait(1.25f);
            BigText.Disappear();
            yield return CoroutineUtility.GetWait(0.5f);
        }

        PlayerCombat.IsPlayersTurn = true;

        BigText.Appear("Your turn!");

        yield return CoroutineUtility.GetWait(1f);

        BigText.Disappear();
    }

    private void OnDestroy()
    {
        PlayerCombat.OnBeginPlayersTurn -= OnBeginPlayersTurn;
        PlayerCombat.OnEndPlayersTurn -= OnEndPlayersTurn;
    }

    private void OnBeginPlayersTurn()
    {

    }

    private void OnEndPlayersTurn()
    {
        StopAllCoroutines();

        // START ENEMY TURN
        StartCoroutine(EnemyTurn());
    }

    private IEnumerator EnemyTurn()
    {
        int enemyCount = EnemyCombat.AllEnemies.Count;

        if (enemyCount <= 0)
        {
            yield return CoroutineUtility.GetWait(1f);

            // Player won!
            BigText.Appear("Victory!");

            MusicPlayer.Stop("boss");
            MusicPlayer.Outro("battle");

            yield return CoroutineUtility.GetWait(3f);

            SceneTransition.Goto(0);

            yield break;
        }

        BigText.Appear("Enemies turn");

        yield return CoroutineUtility.GetWait(1f);

        BigText.Disappear();

        yield return CoroutineUtility.GetWait(0.5f);

        foreach (EnemyCombat enemy in EnemyCombat.AllEnemies)
        {
            enemy.PerformTurn();

            yield return null;
            yield return new WaitWhile(() => enemy.ExecutingCommands);

            yield return CoroutineUtility.GetWait(1f);

            if (PlayerCombat.Dead)
            {
                yield break;
            }
        }

        if (PlayerCombat.Dead)
        {
            yield break;
        }

        // Players turn!
        PlayerCombat.IsPlayersTurn = true;

        BigText.Appear("Your turn!");

        yield return CoroutineUtility.GetWait(1f);

        BigText.Disappear();
    }
}
