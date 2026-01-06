using UnityEngine;

[CreateAssetMenu(menuName = "DOOGH/Stats/Stat Definition", fileName = "Stat_")]
public sealed class StatDefinition : ScriptableObject
{
    [Tooltip("Unique key used for lookup/serialization. Keep stable once shipped.")]
    public string Key;

    [Tooltip("Default base value when not explicitly set in a config.")]
    public float DefaultBaseValue;

    [Header("Optional Clamping")]
    public bool Clamp;
    public float MinValue;
    public float MaxValue = 999999f;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(Key))
            Key = name;

        if (MaxValue < MinValue)
            MaxValue = MinValue;
    }
}

