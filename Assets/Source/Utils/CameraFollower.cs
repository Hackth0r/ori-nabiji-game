using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    [SerializeField] Camera _camera;

    private void Awake()
    {
        if (!_camera)
            _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (!_camera)
            _camera = Camera.main;

        if (_camera)
            transform.forward = _camera.transform.forward;
    }
}
