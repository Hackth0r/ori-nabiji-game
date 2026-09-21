using UnityEngine;
using UnityEngine.Pool;

public class CustomerSpawner : MonoBehaviour
{
    [SerializeField] GameObject _customerPrefab;
    [SerializeField, Min(1)] int _maxCustomers = 6;
    [SerializeField, Min(0.1f)] float _spawnCooldown = 2f;
    [Space(10)]
    [SerializeField] CashRegister _cashRegister;
    [SerializeField] ShelfHandler _shelfHandler;
    [SerializeField] VegetableRegistry _vegetableRegistry;
    [SerializeField] VegetablePool _vegetablePool;
    [SerializeField] Transform _entryPoint;
    [SerializeField] Transform _customersParent;

    ObjectPool<Customer> _customersPool;
    float _remainingTime;

    private void Awake()
    {
        _Prepare();
    }

    private void _Prepare()
    {
        if (!_customerPrefab || !_customerPrefab.GetComponent<Customer>() ||
            !_cashRegister || !_shelfHandler || !_vegetablePool || !_entryPoint)
        {
            Debug.LogError("CustomerSpawner is missing required references.", this);
            enabled = false;
            return;
        }

        _customersPool = new ObjectPool<Customer>(
            createFunc: _Create,
            actionOnGet: _Show,
            actionOnRelease: _Hide,
            actionOnDestroy: _Destroy,
            collectionCheck: false,
            defaultCapacity: _maxCustomers,
            maxSize: _maxCustomers);
    }

    private Customer _Create()
    {
        GameObject newInstance = Instantiate(_customerPrefab, _customersParent);
        Customer customer = newInstance.GetComponent<Customer>();
        _cashRegister.SubscribeOnCustomer(customer);
        return customer;
    }

    private void _Destroy(Customer customer)
    {
        if (customer)
            Destroy(customer.gameObject);
    }

    public void Release(Customer customer)
    {
        if (_customersPool == null || !customer || customer.CurrentState != Customer.CustomerStates.Leave)
            return;

        _customersPool.Release(customer);
        enabled = true;
    }

    private void _Show(Customer customer)
    {
        customer.gameObject.SetActive(true);
        customer.transform.position = _entryPoint.position;

        if (!_SetTaskForCustomer(customer))
        {
            customer.gameObject.SetActive(false);
            _customersPool.Release(customer);
        }
    }

    private void _Hide(Customer customer)
    {
        _ReleaseInventory(customer.Inventory);

        if (customer.Box)
        {
            _ReleaseInventory(customer.Box.VegetableInventory);
            customer.Box.SetActive(false);
        }

        customer.ResetForPool();
        customer.gameObject.SetActive(false);
    }

    private void _ReleaseInventory(VegetableInventory inventory)
    {
        if (!inventory)
            return;

        while (inventory.HasItems())
        {
            Vegetable vegetable = inventory.TakeVegetable();
            if (vegetable)
                _vegetablePool.ReleaseVegetable(vegetable);
        }
    }

    private bool _SetTaskForCustomer(Customer customer)
    {
        Shelf shelf = _shelfHandler.GetRandomShelf();
        if (!shelf || !shelf.Inventory || !shelf.Inventory.TargetVegetableSettings)
            return false;

        int capacity = customer.Inventory ? customer.Inventory.GetCapacity() : 0;
        if (capacity <= 0)
            return false;

        int requiredQuantity = Random.Range(1, capacity + 1);
        Vector3 shelfPosition = shelf.GetRandomPositionNearShelf();

        customer.SetTask(
            shelfPosition,
            _entryPoint.position,
            shelf.Inventory.TargetVegetableSettings,
            requiredQuantity);

        return true;
    }

    private void _TryToSpawnCustomer(float timestep)
    {
        if (_customersPool == null)
            return;

        _remainingTime -= timestep;
        if (_remainingTime > 0f)
            return;

        if (_customersPool.CountActive >= _maxCustomers)
        {
            enabled = false;
            return;
        }

        if (!_shelfHandler.HasConfiguredShelves)
        {
            _remainingTime = 1f;
            return;
        }

        _customersPool.Get();
        _remainingTime = _spawnCooldown;
    }

    private void Update()
    {
        _TryToSpawnCustomer(Time.deltaTime);
    }
}
