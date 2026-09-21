using UnityEngine;
using UnityEngine.Events;

public class BuySpot : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField, Min(0)] int _cost;
    [SerializeField, Min(1f)] float _spendPerSecond = 35f;
    [SerializeField] string _saveId;

    public UnityEvent<int> OnCostChanged;
    public UnityEvent OnBought;
    public UnityEvent OnNotEnoughMoney;

    int _remainingCost;
    float _spendAccumulator;
    int _playerContacts;
    bool _bought;
    bool _notEnoughRaised;
    string _saveKey;

    private void Awake()
    {
        if (!_player)
            _player = FindObjectOfType<Player>();

        Collider trigger = GetComponent<Collider>();
        if (!trigger)
            trigger = gameObject.AddComponent<BoxCollider>();
        trigger.isTrigger = true;

        _saveKey = SaveKeyUtility.ForComponent(this, "buyspot", _saveId);
        int paid = Mathf.Clamp(SaveGameStore.GetInt(_saveKey + "/paid", 0), 0, _cost);

        _bought = SaveGameStore.GetBool(_saveKey + "/bought", false) || paid >= _cost;
        _remainingCost = _bought ? 0 : _cost - paid;
        OnCostChanged?.Invoke(_remainingCost);
    }

    private void Start()
    {
        if (!_bought)
            return;

        OnBought?.Invoke();
        gameObject.SetActive(false);
    }

    public void SetCost(int newCost)
    {
        _cost = Mathf.Max(0, newCost);
        _remainingCost = _cost;
        _bought = _cost == 0;
        _spendAccumulator = 0f;

        SaveGameStore.SetInt(_saveKey + "/paid", _bought ? _cost : 0);
        SaveGameStore.SetBool(_saveKey + "/bought", _bought);
        OnCostChanged?.Invoke(_remainingCost);
    }

    private void FixedUpdate()
    {
        if (_bought || _playerContacts <= 0 || !_player || !_player.Account)
            return;

        _spendAccumulator += _spendPerSecond * Time.fixedDeltaTime;
        int requested = Mathf.Min(Mathf.FloorToInt(_spendAccumulator), _remainingCost);
        if (requested <= 0)
            return;

        int paid = _player.Account.SpendUpTo(requested);
        _spendAccumulator -= paid;

        if (paid <= 0)
        {
            if (!_notEnoughRaised)
            {
                _notEnoughRaised = true;
                OnNotEnoughMoney?.Invoke();
            }
            return;
        }

        _notEnoughRaised = false;
        _remainingCost -= paid;

        SaveGameStore.SetInt(_saveKey + "/paid", _cost - _remainingCost);
        OnCostChanged?.Invoke(_remainingCost);

        if (_remainingCost > 0)
            return;

        _bought = true;
        SaveGameStore.SetBool(_saveKey + "/bought", true);
        SaveGameStore.Flush();
        OnBought?.Invoke();
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player && player == _player)
            _playerContacts++;
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponentInParent<Player>();
        if (player && player == _player)
            _playerContacts = Mathf.Max(0, _playerContacts - 1);
    }

    private void OnDisable()
    {
        _playerContacts = 0;
        _notEnoughRaised = false;
    }

    private void OnValidate()
    {
        _cost = Mathf.Max(0, _cost);
        _spendPerSecond = Mathf.Max(1f, _spendPerSecond);
    }
}
