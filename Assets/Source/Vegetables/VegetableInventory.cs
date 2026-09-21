using System;
using UnityEngine;
using UnityEngine.Events;

public class VegetableInventory : MonoBehaviour
{
    [field: SerializeField] public VegetableSettings TargetVegetableSettings { get; private set; }
    [field: SerializeField] public Transform[] SlotPositions { get; private set; }
    [field: SerializeField] public bool CanTakeVegetables { get; private set; }
    [field: SerializeField, Min(0f)] public float AddingCooldown { get; private set; }

    Vegetable[] _vegetables = Array.Empty<Vegetable>();
    int _addedCount;
    float _remainingTime;

    public UnityEvent OnMaxQuantityReached;
    public UnityEvent OnVegetableAdded;
    public UnityEvent<int> OnVegetablesCountChanged;
    public UnityEvent<Vegetable> OnVegetableTaken;
    public UnityEvent OnInventoryEmptied;

    public int Count => _addedCount;

    public void AllowTakingVegetables() => CanTakeVegetables = true;
    public void ForbidTakingVegetables() => CanTakeVegetables = false;

    public int GetCapacity() => _vegetables.Length;

    public void SetTargetVegetableSettings(VegetableSettings vegetableSettings)
    {
        TargetVegetableSettings = vegetableSettings;
    }

    public bool HasItems() => _addedCount > 0;

    public bool HasItemsOfType(VegetableSettings vegetableSettings)
    {
        return vegetableSettings && _FindVegetableOfType(vegetableSettings) >= 0;
    }

    public bool HasFreeSpace() => _addedCount < _vegetables.Length;
    public bool HasNoCooldown() => _remainingTime <= 0f;

    public bool CanAddItem(Vegetable vegetable)
    {
        return CanTakeVegetables &&
               vegetable &&
               HasFreeSpace() &&
               HasNoCooldown() &&
               (!TargetVegetableSettings || vegetable.VegetableSettings == TargetVegetableSettings);
    }

    private void Awake()
    {
        _Prepare();
    }

    private void _Prepare()
    {
        SlotPositions ??= Array.Empty<Transform>();
        _vegetables = new Vegetable[SlotPositions.Length];
        _addedCount = 0;
    }

    public void AddVegetable(Vegetable vegetable)
    {
        TryAddVegetable(vegetable);
    }

    public bool TryAddVegetable(Vegetable vegetable)
    {
        if (!CanAddItem(vegetable))
            return false;

        int freeSlot = _FindFreeSlot();
        if (freeSlot < 0)
            return false;

        return _AddVegetableToSlot(vegetable, freeSlot);
    }

    private int _FindFreeSlot()
    {
        for (int i = 0; i < _vegetables.Length; i++)
        {
            if (!_vegetables[i] && SlotPositions[i])
                return i;
        }

        return -1;
    }

    private bool _AddVegetableToSlot(Vegetable vegetable, int freeSlot)
    {
        if (!vegetable || freeSlot < 0 || freeSlot >= _vegetables.Length || !SlotPositions[freeSlot])
            return false;

        vegetable.ChangeableParent.SetParent(SlotPositions[freeSlot]);
        _vegetables[freeSlot] = vegetable;
        _addedCount++;

        OnVegetableAdded?.Invoke();
        OnVegetablesCountChanged?.Invoke(_addedCount);

        _remainingTime = AddingCooldown;
        enabled = _remainingTime > 0f;

        if (!HasFreeSpace())
            OnMaxQuantityReached?.Invoke();

        return true;
    }

    public void AddVegetableToSlot(Vegetable vegetable, Transform slot)
    {
        if (!CanAddItem(vegetable) || !slot)
            return;

        int index = Array.IndexOf(SlotPositions, slot);
        if (index >= 0)
            _AddVegetableToSlot(vegetable, index);
    }

    private int _FindAnyVegetable()
    {
        for (int i = _vegetables.Length - 1; i >= 0; i--)
        {
            if (_vegetables[i])
                return i;
        }

        return -1;
    }

    private int _FindVegetableOfType(VegetableSettings vegetableSettings)
    {
        if (!vegetableSettings)
            return -1;

        for (int i = _vegetables.Length - 1; i >= 0; i--)
        {
            if (_vegetables[i] && _vegetables[i].VegetableSettings == vegetableSettings)
                return i;
        }

        return -1;
    }

    private Vegetable _TakeVegetableFromSlot(int slot)
    {
        if (slot < 0 || slot >= _vegetables.Length || !_vegetables[slot])
            return null;

        Vegetable result = _vegetables[slot];
        result.ChangeableParent.SetParent(null, false);
        _vegetables[slot] = null;
        _addedCount = Mathf.Max(0, _addedCount - 1);

        OnVegetableTaken?.Invoke(result);
        OnVegetablesCountChanged?.Invoke(_addedCount);

        if (!HasItems())
            OnInventoryEmptied?.Invoke();

        return result;
    }

    public Vegetable TakeVegetable()
    {
        return _TakeVegetableFromSlot(_FindAnyVegetable());
    }

    public Vegetable TakeVegetableOfType(VegetableSettings vegetableSettings)
    {
        if (!vegetableSettings || !HasItems())
            return null;

        if (TargetVegetableSettings)
        {
            return TargetVegetableSettings == vegetableSettings
                ? _TakeVegetableFromSlot(_FindAnyVegetable())
                : null;
        }

        return _TakeVegetableFromSlot(_FindVegetableOfType(vegetableSettings));
    }

    private void Update()
    {
        _remainingTime -= Time.deltaTime;
        if (_remainingTime <= 0f)
        {
            _remainingTime = 0f;
            enabled = false;
        }
    }

    public static bool TryTransferVegetable(
        VegetableInventory fromInventory,
        VegetableInventory toInventory)
    {
        if (!fromInventory || !toInventory ||
            !toInventory.CanTakeVegetables ||
            !toInventory.HasNoCooldown() ||
            !toInventory.HasFreeSpace())
            return false;

        Vegetable vegetable = toInventory.TargetVegetableSettings
            ? fromInventory.TakeVegetableOfType(toInventory.TargetVegetableSettings)
            : fromInventory.TakeVegetable();

        if (!vegetable)
            return false;

        if (toInventory.TryAddVegetable(vegetable))
            return true;

        // The destination changed between checks. Put the item back when possible.
        fromInventory.TryAddVegetable(vegetable);
        return false;
    }

    public static void TransferVegetables(
        VegetableInventory fromInventory,
        VegetableInventory toInventory)
    {
        TryTransferVegetable(fromInventory, toInventory);
    }
}
