using UnityEngine;

public class Claw : MonoBehaviour
{
    public bool Rotate { get; set; } = true;

    [SerializeField] private Line clawArmTemplate;
    [SerializeField] private int armAmount;
    [SerializeField] private Vector3 armOffset;

    [Space]
    [SerializeField] private float width;
    [SerializeField] private float length;

    [Space]
    [SerializeField] private float closedAngle;
    [SerializeField] private float openAngle;

    [Header("Animation Values")]
    [SerializeField] private float spinSpeed = 15f;
    [SerializeField] private bool invertSpinDirection;
    [SerializeField] private float openAmount = 0;
    private float _oldOpenAmount;

    private Transform[] _arms;
    private float[] _armYRotation;

    public float OpenAmount
    {
        get => openAmount;
        set => openAmount = value;
    }

    private void Awake()
    {
        Vector2 size = new(width, length);

        _arms = new Transform[armAmount];
        _armYRotation = new float[armAmount];
        for (int i = 0; i < armAmount; i++)
        {
            Transform arm = Instantiate(clawArmTemplate, transform).transform;
            arm.name = $"Arm {i + 1}";

            arm.localPosition = armOffset;

            _armYRotation[i] =  360f / armAmount * i;
            arm.localScale = size;

            _arms[i] = arm;
        }

        _oldOpenAmount = openAmount;
        UpdateClaw();
    }

    private void LateUpdate()
    {
        if (_oldOpenAmount != openAmount)
        {
            _oldOpenAmount = openAmount;
            UpdateClaw();
        }
        
        if (Rotate)
        {
            transform.Rotate(Vector3.up, (invertSpinDirection ? -1 : 1) * spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void UpdateClaw()
    {
        float angle = openAmount >= 0 ? Mathf.Lerp(0, openAngle, openAmount) : Mathf.Lerp(0, closedAngle, -openAmount);

        for (int i = 0; i < armAmount; i++)
        {
            Transform arm = _arms[i];
            arm.localRotation = Quaternion.Euler(0, _armYRotation[i], angle);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;

        Gizmos.color = Color.yellow;

        Matrix4x4 startMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        for (int i = 0;i < armAmount; i++)
        {
            float angle = 360f / armAmount * i;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Vector3 forwardDir = rotation * Vector3.forward;
            Vector3 upDir = rotation * Vector3.up;
            Gizmos.DrawLine(armOffset, armOffset + (upDir * length + forwardDir * width));
        }

        Gizmos.matrix = startMatrix;
    }
#endif
}
