using UnityEngine;
using UnityEngine.Events;

public class ChangeableParent : MonoBehaviour
{
    [SerializeField] Transform _transform;
    public UnityEvent OnParentChanged;

    private void Awake()
    {
        if (!_transform)
            _transform = transform;
    }

    public Transform GetTransform() => _transform ? _transform : transform;
    public Vector3 GetPosition() => GetTransform().position;

    public void SetParent(Transform targetTransform, bool sendCall = true)
    {
        GetTransform().SetParent(targetTransform, true);
        if (sendCall)
            OnParentChanged?.Invoke();
    }

    public void SetLocalPosition(Vector3 localPosition)
    {
        GetTransform().localPosition = localPosition;
    }
}
