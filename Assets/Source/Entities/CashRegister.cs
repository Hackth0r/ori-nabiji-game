using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashRegister : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField, Min(0f)] float _distanceForService = 0.35f;
    [SerializeField] bool _automaticService;
    [SerializeField] string _saveId;
    [Space]
    [SerializeField] Transform _queueFirstPosition;
    [SerializeField] Transform _queueSecondPosition;
    [Space]
    [SerializeField] Transform _boxPosition;
    [Header("Cash pickup")]
    [SerializeField] CashPickup _cashPickupPrefab;
    [SerializeField] Transform _cashSpawnPosition;

    readonly Queue<Customer> _customersQueue = new();

    Vector3 _startPosition;
    Vector3 _offset;
    Customer _currentCustomer;
    CashPickup _cashPickup;
    int _pendingCash;
    string _saveKey;

    private void Awake()
    {
        if (!_player)
            _player = FindObjectOfType<Player>();

        _Prepare();
        _saveKey = SaveKeyUtility.ForComponent(this, "register", _saveId);
        _pendingCash = Mathf.Max(0, SaveGameStore.GetInt(_saveKey + "/cash", 0));
    }

    private void Start()
    {
        _EnsureCashPickup();
    }

    private void Update()
    {
        if (_automaticService && !_currentCustomer)
            _TryToServeCustomer();
    }

    private void _Prepare()
    {
        if (_queueFirstPosition && _queueSecondPosition)
        {
            _startPosition = _queueFirstPosition.position;
            _offset = _queueSecondPosition.position - _startPosition;
        }
        else
        {
            _startPosition = transform.position;
            _offset = transform.forward * 0.8f;
        }
    }

    public void SetAutomaticService(bool value)
    {
        _automaticService = value;
    }

    public void SubscribeOnCustomer(Customer customer)
    {
        if (customer)
            customer.OnControlTransferred += EnqueueCustomer;
    }

    public void EnqueueCustomer(Customer customer)
    {
        if (!customer || _customersQueue.Contains(customer))
            return;

        _customersQueue.Enqueue(customer);
        _MoveCustomers();
    }

    private void _MoveCustomers()
    {
        int i = 0;
        foreach (Customer customer in _customersQueue)
        {
            if (customer)
                customer.SetDestination(_startPosition + i * _offset);
            i++;
        }
    }

    private void _TryToServeCustomer()
    {
        if (_currentCustomer || !_boxPosition)
            return;

        while (_customersQueue.Count > 0 && !_customersQueue.Peek())
            _customersQueue.Dequeue();

        if (!_customersQueue.TryPeek(out Customer customer))
            return;

        float remainingDistance = customer.GetRemainingDistance();
        if (float.IsInfinity(remainingDistance) || remainingDistance > _distanceForService)
            return;

        _currentCustomer = customer;
        StartCoroutine(_Serve());
    }

    private IEnumerator _Serve()
    {
        Customer customer = _currentCustomer;
        if (!customer || !customer.Box)
        {
            _currentCustomer = null;
            yield break;
        }

        customer.Box.SetActive(true);
        customer.Box.ChangeableParent.SetParent(_boxPosition);

        float animationDuration = customer.Box.TranslationAnimator &&
                                  customer.Box.TranslationAnimator.DefaultAnimationSettings
            ? customer.Box.TranslationAnimator.DefaultAnimationSettings.AnimationDuration
            : 0.15f;

        yield return new WaitForSeconds(animationDuration);

        VegetableInventory checkoutInventory = customer.Box.VegetableInventory;
        float timeout = 5f;

        while (customer && checkoutInventory && customer.Inventory.HasItems() &&
               checkoutInventory.HasFreeSpace() && timeout > 0f)
        {
            VegetableInventory.TryTransferVegetable(customer.Inventory, checkoutInventory);
            float wait = Mathf.Max(0.02f, checkoutInventory.AddingCooldown);
            timeout -= wait;
            yield return new WaitForSeconds(wait);
        }

        if (!customer)
        {
            _currentCustomer = null;
            yield break;
        }

        customer.ReturnBoxToParent();
        yield return new WaitForSeconds(animationDuration);

        int unitPrice = customer.TargetVegetable ? customer.TargetVegetable.PricePerUnit : 0;
        int money = Mathf.Max(0, customer.RequiredQuantity * unitPrice);
        _AddPendingCash(money);

        customer.OnMoneyPaid();
        _currentCustomer = null;

        if (_customersQueue.Count > 0)
            _customersQueue.Dequeue();

        _MoveCustomers();
    }

    private void _AddPendingCash(int amount)
    {
        if (amount <= 0)
            return;

        long total = (long)_pendingCash + amount;
        _pendingCash = total > int.MaxValue ? int.MaxValue : (int)total;
        SaveGameStore.SetInt(_saveKey + "/cash", _pendingCash);
        _EnsureCashPickup();
    }

    private void _EnsureCashPickup()
    {
        if (_pendingCash <= 0 || !_player || !_player.Account)
            return;

        if (_cashPickup)
        {
            _cashPickup.SetAmount(_pendingCash);
            return;
        }

        Vector3 position = _cashSpawnPosition
            ? _cashSpawnPosition.position
            : transform.position + transform.right * 0.8f;

        _cashPickup = _cashPickupPrefab
            ? Instantiate(_cashPickupPrefab, position, Quaternion.identity)
            : CashPickup.CreateRuntime(position);

        _cashPickup.Initialize(_player.Account, _pendingCash, _OnCashCollected);
    }

    private void _OnCashCollected(int amount)
    {
        _pendingCash = Mathf.Max(0, _pendingCash - amount);
        SaveGameStore.SetInt(_saveKey + "/cash", _pendingCash);
        _cashPickup = null;
    }

    private void OnTriggerStay(Collider other)
    {
        if (_automaticService || _currentCustomer)
            return;

        Player player = other.GetComponentInParent<Player>();
        if (player && player == _player)
            _TryToServeCustomer();
    }
}
