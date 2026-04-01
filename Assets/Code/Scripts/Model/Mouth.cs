using UnityEngine;

public class Mouth : MonoBehaviour
{
    private static readonly float FULL_ROTATION_RADIANS = Mathf.PI * 2f;

    [SerializeField] private Transform start;
    [SerializeField] private Transform end;

    [Space]
    [SerializeField] private Transform[] middlePoints;
    private int _middlePointsLength;
    private float[] _middlePointsStartY;

    [Space]
    [SerializeField] private float speed;
    [SerializeField] private float intensity = 0.3f;
    private float _time;
    // X is Upper limit
    // Y is Lower limit

    private void Start()
    {
        _middlePointsLength = middlePoints.Length;
        _middlePointsStartY = new float[_middlePointsLength];

        Vector2 startPos = start.localPosition;
        Vector2 endPos = end.localPosition;

        for (int i = 0; i < _middlePointsLength; i++)
        {
            Transform point = middlePoints[i];
            Vector2 pos = point.localPosition;

            _middlePointsStartY[i] = pos.y;
        }

        _time = Mathf.PI / 2f;
    }

    private void Update()
    {
        float sin = (Mathf.Sin(_time) + 1) / 2f;

        _time += speed * Time.deltaTime;
        _time %= FULL_ROTATION_RADIANS;

        for (int i = 0; i < _middlePointsLength; i++)
        {
            float startY = _middlePointsStartY[i];

            Vector3 pos = middlePoints[i].localPosition;
            pos.y = Mathf.Lerp(startY, Mathf.Lerp(-startY, startY, sin), intensity);
            middlePoints[i].localPosition = pos;
        }
    }
}
