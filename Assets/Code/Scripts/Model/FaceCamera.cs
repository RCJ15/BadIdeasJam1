using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] private bool onX = true;
    [SerializeField] private bool onY = true;
    [SerializeField] private bool onZ = true;

    private Transform _cameraTransform;

    private Quaternion _oldRotation;
    private Quaternion _oldCameraRotation;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        if (_oldRotation == transform.rotation && _oldCameraRotation == _cameraTransform.rotation)
        {
            return;
        }

        _oldRotation = transform.rotation;
        _oldCameraRotation = _cameraTransform.rotation;
        Vector3 euler = _cameraTransform.rotation.eulerAngles;
        if (!onX) euler.x = 0;
        if (!onY) euler.y = 0;
        if (!onZ) euler.z = 0;
        transform.rotation = Quaternion.Euler(euler);
    }
}
