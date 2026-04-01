using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    [CacheComponent]
    [SerializeField] private LineRenderer line;

    [Space]
    [SerializeField] private Transform[] transforms;

    private PointData[] _points;
    private int _length;

    private void Awake()
    {
        _length = transforms.Length;

        _points = new PointData[_length];
        for (int i = 0; i < _length; i++)
        {
            _points[i] = new(!line.useWorldSpace, transforms[i]);
        }

        line.positionCount = _length;

        UpdatePoints();
    }

    private void LateUpdate()
    {
        foreach (PointData data in _points)
        {
            if (data.Changed())
            {
                UpdatePoints();
                break;
            }
        }
    }

    private void UpdatePoints()
    {
        for (int i = 0; i < _length; i++)
        {
            PointData point = _points[i];
            point.Set();
            line.SetPosition(i, point.Position);
        }
    }

    public class PointData
    {
        public bool IsLocal { get; private set; }
        public Transform Transform { get; private set; }

        public Vector3 Position => IsLocal ? Transform.localPosition : Transform.position;
        //public Quaternion Rotation => IsLocal ? Transform.localRotation : Transform.rotation;

        private Vector3 _oldPosition;
        //private Quaternion _oldRotation;

        public bool Changed()
        {
            return Position != _oldPosition; // || Rotation != _oldRotation;
        }

        public void Set()
        {
            _oldPosition = Position;
            //_oldRotation = Rotation;
        }

        public PointData(bool isLocal, Transform transform)
        {
            IsLocal = isLocal;
            Transform = transform;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;

        if (transforms == null) return;

        Transform previousTransform = null;

        foreach (Transform transform in transforms)
        {
            if (previousTransform != null)
            {
                Gizmos.DrawLine(previousTransform.position, transform.position);
            }
            previousTransform = transform;
        }
    }
#endif
}
