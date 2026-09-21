using System.Collections.Generic;
using UnityEngine;

public class ShelfHandler : MonoBehaviour
{
    [SerializeField] List<Shelf> _shelves = new();

    public bool HasConfiguredShelves
    {
        get
        {
            foreach (Shelf shelf in _shelves)
            {
                if (shelf && shelf.IsConfigured)
                    return true;
            }
            return false;
        }
    }

    public void AddShelf(Shelf shelf)
    {
        if (shelf && !_shelves.Contains(shelf))
            _shelves.Add(shelf);
    }

    public void RemoveShelf(Shelf shelf)
    {
        if (shelf)
            _shelves.Remove(shelf);
    }

    public Shelf GetShelfWithType(VegetableSettings vegetableSettings)
    {
        if (!vegetableSettings)
            return null;

        return _shelves.Find(x =>
            x &&
            x.Inventory &&
            x.Inventory.TargetVegetableSettings == vegetableSettings);
    }

    public Shelf GetRandomShelf()
    {
        int configuredCount = 0;

        foreach (Shelf shelf in _shelves)
        {
            if (shelf && shelf.IsConfigured)
                configuredCount++;
        }

        if (configuredCount == 0)
            return null;

        int targetIndex = Random.Range(0, configuredCount);
        foreach (Shelf shelf in _shelves)
        {
            if (!shelf || !shelf.IsConfigured)
                continue;

            if (targetIndex-- == 0)
                return shelf;
        }

        return null;
    }

    private void OnValidate()
    {
        _shelves.RemoveAll(x => !x);
    }
}
