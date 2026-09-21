using UnityEngine;

[CreateAssetMenu(
    fileName = "Legacy Vegetable",
    menuName = "Ori Nabiji/Legacy Vegetable",
    order = 50)]
public class VegetableSettings : ScriptableObject
{
    [field: SerializeField, Min(0)] public int PricePerUnit { get; private set; }
    [field: SerializeField, Min(0.01f)] public float GrowthTime { get; private set; } = 1f;
    [field: SerializeField] public Sprite Icon { get; private set; }

    public float EffectiveGrowthTime => Mathf.Max(0.01f, GrowthTime);
}
