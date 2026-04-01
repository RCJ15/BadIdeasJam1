using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameCamera : Singleton<GameCamera>
{
    public Camera Main => mainCamera;
    public Camera Game => gameCamera;
    public Camera Background => backgroundCamera;
    public Transform CameraTransform => cameraTransform;

    public float OrthoSize
    {
        get => Main.orthographicSize;
        set
        {
            Game.orthographicSize = value;
            Main.orthographicSize = value;
        }
    }
    public float Fov
    {
        get => Main.fieldOfView;
        set
        {
            Game.fieldOfView = value;
            Main.fieldOfView = value;
        }
    }

    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera gameCamera;
    [SerializeField] private Camera backgroundCamera;

    [Space]
    [SerializeField] private Transform cameraTransform;

    [Header("Render Texture (RT)")]
    [SerializeField] private RenderTexture gameRt;
    [SerializeField] private RenderTexture bgRt;
    [SerializeField] private int desiredBgHeight = 180;
    [SerializeField] private RawImage[] rawImages;
    private int _oldScreenWidth, _oldScreenHeight;

    protected override void Awake()
    {
        base.Awake();

        ResizeRT();

        mainCamera.orthographic = gameCamera.orthographic;
        mainCamera.orthographicSize = gameCamera.orthographicSize;
        mainCamera.fieldOfView = gameCamera.fieldOfView;
    }

    private void ResizeRT()
    {
        _oldScreenWidth = Screen.width;
        _oldScreenHeight = Screen.height;

        gameRt.Release();
        bgRt.Release();

        /*
        gameRt.width = _oldScreenWidth;
        gameRt.height = _oldScreenHeight;
        */

        // Pixelated RT
        float aspectRatio = (float)_oldScreenWidth / (float)_oldScreenHeight;

        bgRt.height = desiredBgHeight;
        bgRt.width = Mathf.RoundToInt((float)desiredBgHeight * aspectRatio);

        if (bgRt.width % 2 != 0) // Odd number
        {
            bgRt.width += 1;
        }

        gameRt.width = bgRt.width * 2;
        gameRt.height = bgRt.height * 2;

        gameRt.Create();
        bgRt.Create();

        // Cuz it doesn't wanna update
        mainCamera.ResetAspect();
        gameCamera.ResetAspect();
        backgroundCamera.ResetAspect();

        foreach (RawImage rawImage in rawImages)
        {
            rawImage.enabled = false;
            rawImage.enabled = true;
        }
    }

    private void LateUpdate()
    {
        if (Screen.width != _oldScreenWidth || Screen.height != _oldScreenHeight)
        {
            ResizeRT();
        }
    }

    public void ZoomCameraFov(float fov, float duration, Ease ease)
    {
        Main.DOKill();
        Game.DOKill();

        Main.DOFieldOfView(fov, duration).SetEase(ease);
        Game.DOFieldOfView(fov, duration).SetEase(ease);
    }

    public void ZoomCameraSize(float size, float duration, Ease ease)
    {
        Main.DOKill();
        Game.DOKill();

        Main.DOOrthoSize(size, duration).SetEase(ease);
        Game.DOOrthoSize(size, duration).SetEase(ease);
    }
}
