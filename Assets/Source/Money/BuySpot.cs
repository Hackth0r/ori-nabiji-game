using UnityEngine;
using UnityEngine.Events;

public class BuySpot : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField, Min(0)] int _cost;
    [SerializeField, Min(1f)] float _spendPerSecond = 35f;

    public UnityEvent<int> OnCostChanged;
    public UnityEvent OnBought;
    public UnityEvent OnNotEnoughMoney;

    int _remainingCost;
    float _spendAccumulator;
    bool _bought;

    private void Awake()
    {
        _remainingCost = _cost;
        OnCostChanged.Invoke(_remainingCost);
    }

    public void SetCost(int newCost)
    {
        _cost = Mathf.Max(0, newCost);
        _remainingCost = _cost;
        _bought = false;
        _spendAccumulator = 0f;
        OnCostChanged.Invoke(_remainingCost);
    }

    private void _SpendWhileStanding()
    {
        if (_bought || _remainingCost <= 0) return;

        _spendAccumulator += _spendPerSecond * Time.fixedDeltaTime;
        int requested = Mathf.FloorToInt(_spendAccumulator);
        if (requested <= 0) return;

        requested = Mathf.Min(requested, _remainingCost);
        int paid = 0;

        while (paid < requested && _player.Account.TryToTakeMoney(1))
            paid++;

        _spendAccumulator -= paid;

        if (paid == 0)
        {
            OnNotEnoughMoney.Invoke();
            _spendAccumulator = 0f;
            return;
        }

        _remainingCost -= paid;
        OnCostChanged.Invoke(_remainingCost);

        if (_remainingCost <= 0)
        {
            _bought = true;
            OnBought.Invoke();
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_bought) return;
        if (other.TryGetComponent(out Player player) && player == _player)
            _SpendWhileStanding();
    }

    private void OnValidate()
    {
        if (OnCostChanged != null)
            OnCostChanged.Invoke(Application.isPlaying ? _remainingCost : _cost);
    }
}
