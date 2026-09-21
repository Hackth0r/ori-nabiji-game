using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(VegetableInventory))]
public class VegetableCultivator : MonoBehaviour
{
    [SerializeField] VegetableSettings _targetVegetable;
    [SerializeField] VegetableInventory _vegetableInventory;
    [SerializeField] VegetablePool _vegetablePool;

    Queue<Transform> _freePositions;
    Vegetable _cultivatableVegetable;
    Transform _cultivatableTransform;
    Dictionary<Vegetable, Transform> _grownVegetables;
    float _timeSpent;

    public void Construct(VegetablePool vegetablePool)
    {
        _vegetablePool = vegetablePool;
    }

    private void Awake()
    {
        if (!_vegetableInventory)
            _vegetableInventory = GetComponent<VegetableInventory>();

        _Prepare();
    }

    private void Start()
    {
        _TryToStartGrowing();
    }

    private void _Prepare()
    {
        int maxVegetables = _vegetableInventory ? _vegetableInventory.SlotPositions.Length : 0;
        _freePositions = new Queue<Transform>(maxVegetables);

        if (_vegetableInventory && _vegetableInventory.SlotPositions != null)
        {
            foreach (Transform slot in _vegetableInventory.SlotPositions)
            {
                if (slot)
                    _freePositions.Enqueue(slot);
            }
        }

        _grownVegetables = new Dictionary<Vegetable, Transform>(maxVegetables);
    }

    public void GrowForTime(float timestep)
    {
        if (!_cultivatableVegetable || !_targetVegetable)
            return;

        _timeSpent += Mathf.Max(0f, timestep);
        float ripeness = _timeSpent / _targetVegetable.EffectiveGrowthTime;
        _cultivatableVegetable.SetRipeness(ripeness);

        if (ripeness >= 1f)
            _OnVegetableGrown();
    }

    private void _OnVegetableGrown()
    {
        if (!_cultivatableVegetable || !_cultivatableTransform)
            return;

        _grownVegetables[_cultivatableVegetable] = _cultivatableTransform;
        _vegetableInventory.AddVegetableToSlot(_cultivatableVegetable, _cultivatableTransform);
        _cultivatableVegetable = null;
        _cultivatableTransform = null;
        _TryToStartGrowing();
    }

    private void _TryToStartGrowing()
    {
        if (_cultivatableVegetable || !_targetVegetable || !_vegetablePool)
            return;

        if (!_freePositions.TryDequeue(out Transform newPosition))
            return;

        Vegetable vegetable = _vegetablePool.GetVegetable(_targetVegetable);
        if (!vegetable)
        {
            _freePositions.Enqueue(newPosition);
            return;
        }

        vegetable.ChangeableParent.SetParent(newPosition, false);
        vegetable.ChangeableParent.SetLocalPosition(Vector3.zero);
        _cultivatableVegetable = vegetable;
        _cultivatableTransform = newPosition;
        _timeSpent = 0f;
    }

    public void OnVegetableTaken(Vegetable vegetable)
    {
        if (!vegetable)
            return;

        if (_grownVegetables.TryGetValue(vegetable, out Transform position))
        {
            _grownVegetables.Remove(vegetable);
            if (position)
                _freePositions.Enqueue(position);
            _TryToStartGrowing();
        }
    }
}
