using DG.Tweening;
using Input;
using UnityEngine;

public class PlayerOverworld : Singleton<PlayerOverworld>
{
    public static int Checkpoint { get; set; } = 0;
    public bool CanMove { get; set; } = true;
    public bool CameraAttached { get; set; } = true;

    private Vector3 _cameraPos => transform.position + _cameraStartOffset + _cameraOffset;

    [CacheComponent]
    [SerializeField] private CharacterController controller;
    [CacheComponent]
    [SerializeField] private Model model;

    [Space]
    [SerializeField] private float cameraFollowDelta;
    private Transform _cameraTransform;
    private Vector3 _cameraStartOffset;
    private Vector3 _cameraOffset;

    [Space]
    [SerializeField] private float lookDelta;
    [SerializeField] private float moveSpeed;
    private float _currentSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float deceleration;
    [SerializeField] private float closeDistance;
    [SerializeField] private float gravity;

    private Vector3 _targetPoint;
    private Vector3 _velocity;
    private bool _isGrounded;

    private Vector3 _currentDirection;
    private bool _triggeredFailsafe;

    protected override void Awake()
    {
        base.Awake();

        _currentDirection = model.transform.forward;
    }

    private void Start()
    {
        _cameraTransform = GameCamera.Instance.CameraTransform;

        _cameraStartOffset = _cameraTransform.position;

        if (OverworldCheckpoint.Checkpoints.TryGetValue(Checkpoint, out OverworldCheckpoint checkpoint))
        {
            transform.position = checkpoint.transform.position;
        }

        _targetPoint = transform.position;
        _cameraTransform.position = _cameraPos;

        controller.enabled = true;
    }

    public void BeginIntroCutscene()
    {
        _cameraOffset = new(0, 10);
    }

    public void MoveCameraDown()
    {
        DOTween.To(() => _cameraOffset, (v) => _cameraOffset = v, Vector3.zero, 5).SetEase(Ease.Linear);
    }

    private void Update()
    {
        // Determine target
        if (PointerOverworld.Hit && GameInput.UI.Click.Pressed)
        {
            _targetPoint = PointerOverworld.Point;
        }

        _isGrounded = controller.isGrounded;

        if (_isGrounded)
        {
            if (_velocity.y < -2f)
            {
                _velocity.y = -2f;
            }
        }

        // Move towards target
        Vector3 pos = transform.position;
        Vector3 posForDistance = pos;
        posForDistance.y = _targetPoint.y;
        float dist = Vector3.Distance(_targetPoint, posForDistance);

        Vector3 direction;

        if (dist <= closeDistance || !CanMove || SceneTransition.IsTransitioning)
        {
            direction = Vector3.zero;
        }
        else
        {
            direction = _targetPoint - pos;
            direction.y = 0;
            direction.Normalize();
        }

        if (direction != Vector3.zero)
        {
            _currentDirection = direction;

            _currentSpeed = Mathf.MoveTowards(_currentSpeed, moveSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0, deceleration * Time.deltaTime);
        }

        model.transform.rotation = Quaternion.Slerp(model.transform.rotation, Quaternion.LookRotation(_currentDirection, Vector3.up), lookDelta * Time.deltaTime);

        // Apply gravity
        _velocity.y += -gravity * Time.deltaTime;

        // Move
        Vector3 finalMove = _currentDirection * _currentSpeed + Vector3.up * _velocity.y;
        controller.Move(finalMove * Time.deltaTime);

        Vector3 velocity = controller.velocity;

        float xSpeed = new Vector2(velocity.x, velocity.z).magnitude;
        model.SetFloat("WalkSpeed", xSpeed);
        model.SetBool("Walking", xSpeed > 0.5f);

        if (CameraAttached)
        {
            _cameraTransform.position = Vector3.Lerp(_cameraTransform.position, _cameraPos, cameraFollowDelta * Time.deltaTime);
        }

        // Failsafe
        if (!_triggeredFailsafe && transform.position.y <= -100)
        {
            _triggeredFailsafe = true;
            SceneTransition.Goto(SceneTransition.CurrentSceneIndex);
        }
    }
}
