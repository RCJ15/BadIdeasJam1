using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class OverworldManager : Singleton<OverworldManager>
{
    public static bool DoIntroCutscene { get; set; } = true;
    public static float OverworldTime { get; set; }

#if UNITY_EDITOR
    private static bool _doneCutsceneEditor = false;
#endif

#if UNITY_EDITOR
    [Header("Intro cutscene")]
    [SerializeField] private bool doIntroCutscene;
#endif
    [SerializeField] private TMP_Text text;

    private PlayerOverworld _player;
    private GameCamera _camera;

    protected override void Awake()
    {
        base.Awake();

#if UNITY_EDITOR
        if (!_doneCutsceneEditor)
        {
            DoIntroCutscene = doIntroCutscene;
            _doneCutsceneEditor = true;
        }
#endif
    }

    private IEnumerator Start()
    {
        _player = PlayerOverworld.Instance;
        _camera = GameCamera.Instance;

        text.color = new(1, 1, 1, 0);

        if (DoIntroCutscene)
        {
            DoIntroCutscene = false;

            _player.BeginIntroCutscene();

            yield return CoroutineUtility.GetWait(1f);

            _player.MoveCameraDown();

            MusicPlayer.Play("Overworld");

            yield return CoroutineUtility.GetWait(5f);

            text.DOFade(1, 1f).onComplete = () =>
            {
                text.DOFade(0, 1f).SetDelay(2f);
            };

            PointerOverworld.HasControl = true;
        }
        else
        {
            PointerOverworld.HasControl = true;
            MusicPlayer.Play("Overworld", 3f, OverworldTime + 0.1f);
        }
    }

    public void EnterBattle(int sceneIndex, bool isBoss = false)
    {
        SoundManager.PlaySound("energy", 1);
        OverworldTime = MusicManager.GetTime("overworld");

        _camera.ZoomCameraFov(20, 1, Ease.OutExpo);

        MusicPlayer.Stop("overworld");
        MusicPlayer.Play(isBoss ? "boss" : "battle");

        SceneTransition.Goto(sceneIndex);
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (UnityEngine.InputSystem.Keyboard.current.kKey.wasPressedThisFrame)
        {
            EnterBattle(1);
        }
#endif
    }
}
