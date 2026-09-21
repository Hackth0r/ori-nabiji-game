using System.Collections.Generic;
using UnityEngine;

public class VegetableRegistry : MonoBehaviour
{
    [SerializeField] List<VegetableSettings> _vegetableSettings = new();

    public void AddVegetableSettings(VegetableSettings vegetableSettings)
    {
        if (vegetableSettings && !_vegetableSettings.Contains(vegetableSettings))
            _vegetableSettings.Add(vegetableSettings);
    }

    public void RemoveVegetableSettings(VegetableSettings vegetableSettings)
    {
        if (vegetableSettings)
            _vegetableSettings.Remove(vegetableSettings);
    }

    public VegetableSettings GetRandomVegetableSettings()
    {
        _vegetableSettings.RemoveAll(x => !x);
        if (_vegetableSettings.Count == 0)
            return null;

        return _vegetableSettings[Random.Range(0, _vegetableSettings.Count)];
    }

    private void OnValidate()
    {
        _vegetableSettings.RemoveAll(x => !x);
    }
}
