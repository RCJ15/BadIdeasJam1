using DG.Tweening;
using System.Collections;
using UnityEngine;

public class BattleZone2 : BattleZone
{
    public override int BattleIndex => 2;

    [Header("Roomba")]
    [SerializeField] private Model model;

    protected override IEnumerator Coroutine()
    {
        _player.CameraAttached = false;
        _player.gameObject.SetActive(false);

        MusicPlayer.Stop("overworld", 3f);

        _cameraTransform.position = model.transform.position + new Vector3(2.5f, 1.3f);
        _cameraTransform.rotation *= Quaternion.Euler(0, -90, 0);

        _camera.ZoomCameraFov(25, 1, Ease.OutSine);

        yield return CoroutineUtility.GetWait(1f);

        model.transform.DORotate(new(0, 180, 0), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.OutBack);
        SoundManager.PlaySound("whirr");

        yield return CoroutineUtility.GetWait(0.5f);

        model.SetFloat("AttackSpeed", 1.5f);
        model.SetTrigger("Attack");
    }
}
