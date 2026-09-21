using UnityEngine;

public class Shelf : MonoBehaviour
{
    [field: SerializeField] public VegetableInventory Inventory { get; private set; }
    [SerializeField] Transform[] _positionsNearShelf;

    public bool IsConfigured =>
        Inventory &&
        Inventory.TargetVegetableSettings &&
        _positionsNearShelf != null &&
        _positionsNearShelf.Length > 0;

    public Vector3 GetRandomPositionNearShelf()
    {
        if (_positionsNearShelf == null || _positionsNearShelf.Length == 0)
            return transform.position;

        int start = Random.Range(0, _positionsNearShelf.Length);
        for (int i = 0; i < _positionsNearShelf.Length; i++)
        {
            Transform candidate = _positionsNearShelf[(start + i) % _positionsNearShelf.Length];
            if (candidate)
                return candidate.position;
        }

        return transform.position;
    }
}
