using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Unit User { get; set; }
    public Unit Target { get; set; }
    public float TravelTime { get; set; }

    [SerializeField] private float heightMultiplier = 0.5f;
    [SerializeField] private float minimumHeight;
    private float _speed;
    private float _t;

    private Vector3 _startPos;
    private Vector3 _targetPos;
    private Vector3 _oldPosition;
    private float _distance;

    private void Start()
    {
        _speed = 1 / TravelTime;

        _startPos = User.transform.position;
        _targetPos = Target.transform.position;

        _distance = Vector3.Distance(_startPos, _targetPos);

        _t = 0;
    }

    private void Update()
    {
        float x = Mathf.Lerp(_startPos.x, _targetPos.x, _t);
        float z = Mathf.Lerp(_startPos.z, _targetPos.z, _t);

        float y =
            Mathf.Lerp(_startPos.y, _targetPos.y, _t)
            +
            (Mathf.Sin(Mathf.Lerp(0, 180 * Mathf.Deg2Rad, _t)) * Mathf.Max(minimumHeight, _distance * heightMultiplier));

        Vector3 newPos = new(x, y, z);

        transform.forward = newPos - _oldPosition;
        _oldPosition = newPos;

        transform.position = newPos;

        _t += Time.deltaTime * _speed;

        if (_t >= 1)
        {
            User.AttackUnit();
            Destroy(gameObject);
            return;
        }
    }
}
