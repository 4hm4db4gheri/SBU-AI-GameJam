using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Stats")]
    [SerializeField] private StatsComponent _statsComponent;
    [SerializeField] private StatDefinition _maxHealthStat;

    [Header("Options")]
    [SerializeField] private bool _destroyOnDeath = true;

    public float Current { get; private set; }
    public float Max => _statsComponent != null ? _statsComponent.Stats.GetValue(_maxHealthStat, 100f) : 100f;
    public bool IsDead => Current <= 0f;

    public event Action<PlayerHealth> Died;
    public event Action<float, bool> Damaged; // (amount, isCritical)
    public event Action<float> Healed;

    private bool _isInvulnerable = false;

    private void Awake()
    {
        _statsComponent = GetComponent<StatsComponent>();
    }

    private void Start()
    {
        Current = Max;
        if (_statsComponent != null)
            _statsComponent.Stats.StatValueChanged += OnStatValueChanged;
    }

    private void OnDestroy()
    {
        if (_statsComponent != null)
            _statsComponent.Stats.StatValueChanged -= OnStatValueChanged;
    }

    private void OnStatValueChanged(StatDefinition def, float newValue)
    {
        if (def == _maxHealthStat)
            Current = Mathf.Clamp(Current, 0f, newValue);
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, bool isCritical)
    {
        if (IsDead || _isInvulnerable)
            return;

        float dmg = Mathf.Max(0f, amount);
        if (dmg <= 0f)
            return;

        Current = Mathf.Max(0f, Current - dmg);
        Damaged?.Invoke(dmg, isCritical);

        if (Current <= 0f)
            Die();
    }

    public void Heal(float amount)
    {
        if (IsDead)
            return;

        float heal = Mathf.Max(0f, amount);
        if (heal <= 0f)
            return;

        float before = Current;
        Current = Mathf.Min(Max, Current + heal);
        Healed?.Invoke(Current - before);
    }

    private void Die()
    {
        if (IsDead == false)
            Current = 0f;

        Died?.Invoke(this);

        if (_destroyOnDeath)
            Destroy(gameObject);

        ExperienceManager.Instance.AddExperience(10);
    }

    public void Invulnerable(float duration)
    {
        StartCoroutine(InvulnerableCoroutine(duration));
    }

    private IEnumerator InvulnerableCoroutine(float duration)
    {
        _isInvulnerable = true;
        yield return new WaitForSeconds(duration);
    }
}