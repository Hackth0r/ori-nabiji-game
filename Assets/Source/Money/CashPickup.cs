using System;
using TMPro;
using UnityEngine;

public class CashPickup : MonoBehaviour
{
    [SerializeField] TMP_Text _valueText;
    [SerializeField, Min(0f)] float _floatHeight = 0.12f;
    [SerializeField, Min(0f)] float _floatSpeed = 2.5f;

    MoneyAccount _targetAccount;
    Action<int> _onCollected;
    int _amount;
    Vector3 _startPosition;

    public int Amount => _amount;

    private void Awake()
    {
        Collider trigger = GetComponent<Collider>();
        if (!trigger)
            trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        if (!body)
            body = gameObject.AddComponent<Rigidbody>();

        body.isKinematic = true;
        body.useGravity = false;
    }

    public void Initialize(MoneyAccount targetAccount, int amount, Action<int> onCollected = null)
    {
        _targetAccount = targetAccount;
        _onCollected = onCollected;
        _startPosition = transform.position;
        SetAmount(amount);
    }

    public void SetAmount(int amount)
    {
        _amount = Mathf.Max(0, amount);
        if (_valueText)
            _valueText.text = $"₾{_amount}";
    }

    private void Update()
    {
        if (_floatHeight <= 0f)
            return;

        transform.position = _startPosition +
            Vector3.up * (Mathf.Sin(Time.unscaledTime * _floatSpeed) * _floatHeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_targetAccount || _amount <= 0)
            return;

        Player player = other.GetComponentInParent<Player>();
        if (!player || player.Account != _targetAccount)
            return;

        int collected = _amount;
        _amount = 0;
        _targetAccount.AddMoney(collected);
        _onCollected?.Invoke(collected);
        Destroy(gameObject);
    }

    public static CashPickup CreateRuntime(Vector3 position)
    {
        GameObject instance = GameObject.CreatePrimitive(PrimitiveType.Cube);
        instance.name = "Cash Pickup";
        instance.transform.position = position;
        instance.transform.localScale = new Vector3(0.55f, 0.12f, 0.36f);

        Renderer renderer = instance.GetComponent<Renderer>();
        if (renderer)
        {
            Shader shader = Shader.Find("Standard");
            if (shader)
            {
                Material material = new Material(shader);
                material.color = new Color(0.28f, 0.67f, 0.32f);
                renderer.material = material;
            }
        }

        Collider collider = instance.GetComponent<Collider>();
        collider.isTrigger = true;
        return instance.AddComponent<CashPickup>();
    }
}
