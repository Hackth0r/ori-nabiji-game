using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(NavMeshAgent))]
public class MeshAgentController : MonoBehaviour
{
    [SerializeField] Camera _camera;
    [SerializeField] LayerMask _groundMask = ~0;
    [SerializeField] bool _allowDesktopClickToMove = true;
    [SerializeField] bool _allowKeyboard = true;
    [Header("Mobile drag joystick")]
    [SerializeField] bool _allowTouchDrag = true;
    [SerializeField, Min(20f)] float _touchRadius = 120f;
    [SerializeField, Range(0f, 0.9f)] float _touchDeadZone = 0.12f;
    [SerializeField, Min(0.5f)] float _lookAheadDistance = 2f;

    NavMeshAgent _meshAgent;
    int _activeFingerId = -1;
    Vector2 _touchOrigin;
    bool _touchStartedOverUi;

    private void Awake()
    {
        _meshAgent = GetComponent<NavMeshAgent>();
        if (!_camera)
            _camera = Camera.main;
    }

    private void Update()
    {
        if (!_camera)
            _camera = Camera.main;

        bool directionalInput = false;

        if (_allowTouchDrag && Input.touchCount > 0)
            directionalInput = _HandleTouch();

        if (!directionalInput && _allowKeyboard)
            directionalInput = _HandleKeyboard();

        if (!directionalInput && _allowDesktopClickToMove)
            _HandleDesktopPointer();
    }

    private bool _HandleKeyboard()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude < 0.01f)
            return false;

        _MoveInDirection(input.normalized);
        return true;
    }

    private bool _HandleTouch()
    {
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if (_activeFingerId < 0 && touch.phase == TouchPhase.Began)
            {
                _activeFingerId = touch.fingerId;
                _touchOrigin = touch.position;
                _touchStartedOverUi = EventSystem.current &&
                    EventSystem.current.IsPointerOverGameObject(touch.fingerId);
            }

            if (touch.fingerId != _activeFingerId)
                continue;

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                _activeFingerId = -1;
                _touchStartedOverUi = false;
                if (_meshAgent.isOnNavMesh)
                    _meshAgent.ResetPath();
                return false;
            }

            if (_touchStartedOverUi)
                return false;

            Vector2 delta = Vector2.ClampMagnitude(touch.position - _touchOrigin, _touchRadius) / _touchRadius;
            if (delta.magnitude <= _touchDeadZone)
                return false;

            _MoveInDirection(delta.normalized);
            return true;
        }

        return false;
    }

    private void _MoveInDirection(Vector2 input)
    {
        if (!_meshAgent.isOnNavMesh)
            return;

        Transform cameraTransform = _camera ? _camera.transform : transform;
        Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 direction = (right * input.x + forward * input.y).normalized;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Vector3 target = transform.position + direction * _lookAheadDistance;
        if (NavMesh.SamplePosition(target, out NavMeshHit navHit, _lookAheadDistance, _meshAgent.areaMask))
            _meshAgent.SetDestination(navHit.position);
    }

    private void _HandleDesktopPointer()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (!Input.GetMouseButton(0) || !_camera)
            return;

        if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, 250f, _groundMask, QueryTriggerInteraction.Ignore))
            return;

        if (NavMesh.SamplePosition(hit.point, out NavMeshHit navHit, 2f, _meshAgent.areaMask))
            _meshAgent.SetDestination(navHit.position);
#endif
    }
}
