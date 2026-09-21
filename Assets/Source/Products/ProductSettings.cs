using UnityEngine;

public enum ProductCategory
{
    Produce = 0,
    Bakery = 1,
    Dairy = 2,
    Drinks = 3,
    Snacks = 4,
    Household = 5,
    Frozen = 6,
    Other = 99
}

[CreateAssetMenu(
    fileName = "Product",
    menuName = "Ori Nabiji/Product",
    order = 1)]
public class ProductSettings : VegetableSettings
{
    [SerializeField] string _productId;
    [SerializeField] string _displayName;
    [SerializeField] ProductCategory _category = ProductCategory.Other;

    public string ProductId => string.IsNullOrWhiteSpace(_productId) ? name : _productId;
    public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
    public ProductCategory Category => _category;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(_productId))
            _productId = System.Guid.NewGuid().ToString("N");
    }
#endif
}
