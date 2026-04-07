using DG.Tweening;
using Input;
using UnityEngine;

public class PointerOverworld : Singleton<PointerOverworld>
{
    public static bool HasControl { get; set; }

    public static bool Hit { get; set; }
    public static Vector3 Point { get; set; }
    public static Vector3 Normal { get; set; }
    public static Quaternion Rotation { get; set; } = Quaternion.identity;

    [SerializeField] private LayerMask layer;
    [SerializeField] private Transform visualTemplate;
    [SerializeField] private float visualTweenDuration = 0.5f;

    private Transform _currentVisual;

    private Camera _camera;

    protected override void Awake()
    {
        base.Awake();

        Hit = false;
    }

    private void Start()
    {
        _camera = GameCamera.Instance.Main;
    }

    private void OnDestroy()
    {
        Hit = false;
    }

    private void Update()
    {
        if (_camera == null)
        {
            _camera = GameCamera.Instance.Main;
        }

        if (_camera == null) return;

        Ray mouseRay = _camera.ScreenPointToRay(GameInput.UI.Point.Value);

        if (Physics.Raycast(mouseRay, out RaycastHit hit, 10000f, layer))
        {
            Hit = true;
            Point = hit.point;
            Normal = hit.normal;
            Rotation = Quaternion.LookRotation(Normal, Vector3.up);
        }
        else
        {
            Hit = false;
        }

        if (!HasControl)
        {
            Hit = false;
        }

        if (_currentVisual == null)
        {
            if (GameInput.UI.Click.Pressed && Hit)
            {
                _currentVisual = Instantiate(visualTemplate, Point, Rotation);
                _currentVisual.gameObject.SetActive(true);

                _currentVisual.localScale = Vector3.zero;
                _currentVisual.DOScale(Vector3.one, visualTweenDuration).SetEase(Ease.OutExpo);
            }
        }
        else
        {
            _currentVisual.position = Point;
            _currentVisual.rotation = Rotation;

            if (!GameInput.UI.Click.Pressed || !Hit)
            {
                _currentVisual.DOKill();
                _currentVisual.DOScale(Vector3.zero, visualTweenDuration).SetEase(Ease.InExpo);

                Destroy(_currentVisual.gameObject, visualTweenDuration);

                _currentVisual = null;
            }
        }
    }
}
