using TMPro;
using UnityEngine;

public class CashPickup : MonoBehaviour
{
    [SerializeField] TMP_Text _valueText;
    [SerializeField, Min(0f)] float _floatHeight = 0.18f;
    [SerializeField, Min(0f)] float _floatSpeed = 2.5f;

    MoneyAccount _targetAccount;
    int _amount;
    Vector3 _startPosition;

    public void Initialize(MoneyAccount targetAccount, int amount)
    {
        _targetAccount = targetAccount;
        _amount = Mathf.Max(0, amount);
        _startPosition = transform.position;

        if (_valueText)
            _valueText.text = _amount.ToString();
    }

    private void Update()
    {
        if (_floatHeight <= 0f) return;

        transform.position = _startPosition +
            Vector3.up * (Mathf.Sin(Time.time * _floatSpeed) * _floatHeight);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_targetAccount || _amount <= 0) return;

        if (other.TryGetComponent(out Player player) &&
            player.Account == _targetAccount)
        {
            _targetAccount.AddMoney(_amount);
            _amount = 0;
            Destroy(gameObject);
        }
    }
}
