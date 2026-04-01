using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BattleZone4 : BattleZone
{
    public override int BattleIndex => 4;

    [Header("Roomba")]
    [SerializeField] private Transform middleModel;
    [SerializeField] private Model[] models;

    protected override IEnumerator Coroutine()
    {
        _player.CameraAttached = false;
        _player.gameObject.SetActive(false);

        MusicPlayer.Stop("overworld", 3f);

        _cameraTransform.position = middleModel.position + new Vector3(0, 4f, -7f);

        _camera.ZoomCameraFov(25, 1, Ease.OutSine);

        yield return CoroutineUtility.GetWait(1f);

        foreach (var model in models)
        {
            model.transform.DORotate(new(0, 180, 0), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack);
        }

        SoundManager.PlaySound("whirr");

        yield return CoroutineUtility.GetWait(0.5f);

        foreach (var model in models)
        {
            model.SetFloat("AttackSpeed", 1.5f);
            model.SetTrigger("Attack");
        }
    }
}
