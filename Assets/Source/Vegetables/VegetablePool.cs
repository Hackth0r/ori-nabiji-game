using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class VegetablePool : MonoBehaviour
{
    [Serializable]
    public class PoolData
    {
        public VegetableSettings vegetable;
        public GameObject prefab;
        [Min(0)] public int startCapacity = 4;
        [Min(1)] public int maxCapacity = 64;
    }

    [SerializeField] PoolData[] _vegetablePrefabs;
    [SerializeField] Transform _vegetablesParent;

    readonly Dictionary<VegetableSettings, ObjectPool<Vegetable>> _pools = new();

    private void Awake()
    {
        _CreatePools();
    }

    private void _CreatePools()
    {
        if (_vegetablePrefabs == null)
            return;

        foreach (PoolData pool in _vegetablePrefabs)
        {
            if (pool == null || !pool.vegetable || !pool.prefab ||
                !pool.prefab.GetComponent<Vegetable>())
            {
                Debug.LogWarning("Ignoring invalid product pool entry.", this);
                continue;
            }

            if (_pools.ContainsKey(pool.vegetable))
            {
                Debug.LogWarning($"Duplicate pool entry for {pool.vegetable.name}.", this);
                continue;
            }

            int defaultCapacity = Mathf.Max(1, pool.startCapacity);
            int maxCapacity = Mathf.Max(defaultCapacity, pool.maxCapacity);

            _pools[pool.vegetable] = new ObjectPool<Vegetable>(
                createFunc: () => _Create(pool),
                actionOnGet: _Show,
                actionOnRelease: _Hide,
                actionOnDestroy: _Destroy,
                collectionCheck: false,
                defaultCapacity: defaultCapacity,
                maxSize: maxCapacity);
        }
    }

    public Vegetable GetVegetable(VegetableSettings vegetableSettings)
    {
        if (!vegetableSettings || !_pools.TryGetValue(vegetableSettings, out ObjectPool<Vegetable> pool))
        {
            Debug.LogError($"No product pool configured for {(vegetableSettings ? vegetableSettings.name : "null")}.", this);
            return null;
        }

        return pool.Get();
    }

    public void ReleaseVegetable(Vegetable vegetable)
    {
        if (!vegetable)
            return;

        if (!_pools.TryGetValue(vegetable.VegetableSettings, out ObjectPool<Vegetable> pool))
        {
            Destroy(vegetable.gameObject);
            return;
        }

        pool.Release(vegetable);
    }

    private Vegetable _Create(PoolData pool)
    {
        GameObject newInstance = Instantiate(pool.prefab, _vegetablesParent);
        return newInstance.GetComponent<Vegetable>();
    }

    private void _Show(Vegetable vegetable)
    {
        vegetable.SetRipeness(0f);
        vegetable.gameObject.SetActive(true);
    }

    private void _Hide(Vegetable vegetable)
    {
        vegetable.gameObject.SetActive(false);

        if (vegetable.ChangeableParent)
            vegetable.ChangeableParent.SetParent(_vegetablesParent, false);
        else
            vegetable.transform.SetParent(_vegetablesParent, false);
    }

    private void _Destroy(Vegetable vegetable)
    {
        if (vegetable)
            Destroy(vegetable.gameObject);
    }
}
