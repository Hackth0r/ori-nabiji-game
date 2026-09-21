using System;
using UnityEngine;
using UnityEngine.Events;

public class MoneyAccount : MonoBehaviour
{
    [field: SerializeField, Min(0)] public int Money { get; private set; }
    [SerializeField] bool _persist = true;
    [SerializeField] string _saveId = "player";

    public UnityEvent<int> OnValueChanged;

    string _saveKey;

    private void Awake()
    {
        _saveKey = SaveKeyUtility.ForComponent(this, "money", _saveId);

        if (_persist)
            Money = Mathf.Max(0, SaveGameStore.GetInt(_saveKey, Money));

        OnValueChanged?.Invoke(Money);
    }

    public void AddMoney(int value)
    {
        if (value <= 0)
            return;

        long nextValue = (long)Money + value;
        Money = (int)Math.Min(int.MaxValue, nextValue);
        _Commit();
    }

    public bool TryToTakeMoney(int value)
    {
        if (value < 0 || Money < value)
            return false;

        if (value == 0)
            return true;

        Money -= value;
        _Commit();
        return true;
    }

    public int SpendUpTo(int requestedAmount)
    {
        if (requestedAmount <= 0 || Money <= 0)
            return 0;

        int spent = Mathf.Min(requestedAmount, Money);
        Money -= spent;
        _Commit();
        return spent;
    }

    private void _Commit()
    {
        if (_persist)
            SaveGameStore.SetInt(_saveKey, Money);

        OnValueChanged?.Invoke(Money);
    }

    private void OnValidate()
    {
        Money = Mathf.Max(0, Money);
    }
}
