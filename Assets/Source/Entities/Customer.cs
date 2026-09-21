using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Customer : Entity
{
    public enum CustomerStates
    {
        TakeVegetables = 0,
        PayForVegetables = 1,
        Leave = 2,
        Idle = 3
    }

    [SerializeField] NavMeshAgent _meshAgent;
    [field: SerializeField] public CustomerStates CurrentState { get; private set; } = CustomerStates.Idle;
    [field: SerializeField] public Box Box { get; private set; }
    [SerializeField] Transform _boxParent;

    public UnityEvent<int, VegetableSettings> OnTaskReceived;
    public UnityEvent OnRequiredQuantityReached;
    public event Action<Customer> OnControlTransferred;
    public UnityEvent OnTaskCompleted;

    public int RequiredQuantity { get; private set; }
    public VegetableSettings TargetVegetable { get; private set; }

    Vector3 _exitPosition;

    private void Awake()
    {
        if (!_meshAgent)
            _meshAgent = GetComponent<NavMeshAgent>();
    }

    public void ReturnBoxToParent()
    {
        if (Box && Box.ChangeableParent)
            Box.ChangeableParent.SetParent(_boxParent);
    }

    public void SetDestination(Vector3 destination)
    {
        if (_meshAgent && _meshAgent.isOnNavMesh)
            _meshAgent.SetDestination(destination);
    }

    public float GetRemainingDistance()
    {
        if (!_meshAgent || !_meshAgent.isOnNavMesh || _meshAgent.pathPending)
            return float.PositiveInfinity;

        return _meshAgent.remainingDistance;
    }

    public void SetTask(
        Vector3 shelfPosition,
        Vector3 exitPosition,
        VegetableSettings targetVegetable,
        int requiredQuantity)
    {
        TargetVegetable = targetVegetable;
        _exitPosition = exitPosition;

        if (!Inventory || !TargetVegetable || Inventory.GetCapacity() <= 0)
        {
            CurrentState = CustomerStates.Leave;
            SetDestination(_exitPosition);
            return;
        }

        Inventory.SetTargetVegetableSettings(TargetVegetable);
        RequiredQuantity = Mathf.Clamp(requiredQuantity, 1, Inventory.GetCapacity());
        Inventory.AllowTakingVegetables();

        CurrentState = CustomerStates.TakeVegetables;
        SetDestination(shelfPosition);
        OnTaskReceived?.Invoke(RequiredQuantity, TargetVegetable);
    }

    public void OnInventoryCountChanged(int vegetablesCount)
    {
        if (CurrentState != CustomerStates.TakeVegetables || vegetablesCount < RequiredQuantity)
            return;

        Inventory.ForbidTakingVegetables();
        CurrentState = CustomerStates.PayForVegetables;
        OnControlTransferred?.Invoke(this);
        OnRequiredQuantityReached?.Invoke();
    }

    public void OnMoneyPaid()
    {
        if (CurrentState != CustomerStates.PayForVegetables)
            return;

        CurrentState = CustomerStates.Leave;
        SetDestination(_exitPosition);
        OnTaskCompleted?.Invoke();
    }

    public void ResetForPool()
    {
        if (_meshAgent && _meshAgent.isOnNavMesh)
            _meshAgent.ResetPath();

        RequiredQuantity = 0;
        TargetVegetable = null;
        CurrentState = CustomerStates.Idle;
    }
}
