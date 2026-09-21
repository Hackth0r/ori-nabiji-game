using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class StockWorker : MonoBehaviour
{
    [Serializable]
    public class RestockRoute
    {
        public VegetableInventory source;
        public Transform sourcePoint;
        public VegetableInventory destination;
        public Transform destinationPoint;
    }

    enum WorkerState
    {
        Idle,
        MovingToSource,
        Loading,
        MovingToDestination,
        Unloading
    }

    [SerializeField] VegetableInventory _workerInventory;
    [SerializeField] List<RestockRoute> _routes = new();
    [SerializeField, Min(0.05f)] float _interactionDistance = 0.35f;
    [SerializeField, Min(0.1f)] float _idleRetryDelay = 0.5f;

    NavMeshAgent _agent;
    RestockRoute _route;
    WorkerState _state;
    float _retryTimer;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        if (!_workerInventory)
            _workerInventory = GetComponent<VegetableInventory>();

        if (_workerInventory)
            _workerInventory.AllowTakingVegetables();
    }

    private void Update()
    {
        if (!_agent.isOnNavMesh || !_workerInventory)
            return;

        switch (_state)
        {
            case WorkerState.Idle:
                _UpdateIdle();
                break;
            case WorkerState.MovingToSource:
                if (_Arrived()) _state = WorkerState.Loading;
                break;
            case WorkerState.Loading:
                _Load();
                break;
            case WorkerState.MovingToDestination:
                if (_Arrived()) _state = WorkerState.Unloading;
                break;
            case WorkerState.Unloading:
                _Unload();
                break;
        }
    }

    private void _UpdateIdle()
    {
        _retryTimer -= Time.deltaTime;
        if (_retryTimer > 0f)
            return;

        _route = _FindRoute();
        if (_route == null)
        {
            _retryTimer = _idleRetryDelay;
            return;
        }

        if (_workerInventory.HasItems())
        {
            _MoveTo(_route.destinationPoint, WorkerState.MovingToDestination);
            return;
        }

        _MoveTo(_route.sourcePoint, WorkerState.MovingToSource);
    }

    private RestockRoute _FindRoute()
    {
        foreach (RestockRoute route in _routes)
        {
            if (route == null || !route.source || !route.destination ||
                !route.sourcePoint || !route.destinationPoint)
                continue;

            if (route.source.HasItems() && route.destination.HasFreeSpace())
                return route;
        }

        return null;
    }

    private void _Load()
    {
        if (!_route.source.HasItems() || !_workerInventory.HasFreeSpace())
        {
            _MoveTo(_route.destinationPoint, WorkerState.MovingToDestination);
            return;
        }

        VegetableInventory.TryTransferVegetable(_route.source, _workerInventory);
    }

    private void _Unload()
    {
        if (!_workerInventory.HasItems() || !_route.destination.HasFreeSpace())
        {
            _route = null;
            _state = WorkerState.Idle;
            _retryTimer = 0f;
            return;
        }

        VegetableInventory.TryTransferVegetable(_workerInventory, _route.destination);
    }

    private void _MoveTo(Transform point, WorkerState nextState)
    {
        if (!point)
        {
            _state = WorkerState.Idle;
            return;
        }

        _agent.SetDestination(point.position);
        _state = nextState;
    }

    private bool _Arrived()
    {
        if (_agent.pathPending)
            return false;

        return _agent.remainingDistance <= Mathf.Max(_interactionDistance, _agent.stoppingDistance);
    }
}
