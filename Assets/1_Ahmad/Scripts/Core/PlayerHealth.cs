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

    public float currentHealth { get; private set; }
    public float MaxHealth => _statsComponent != null ? _statsComponent.Stats.GetValue(_maxHealthStat, 100f) : 100f;
    public bool IsDead => currentHealth <= 0f;

    public event Action<PlayerHealth> Died;
    public event Action<float> Healed;

    private bool _isInvulnerable = false;

    private void Awake()
    {
        _statsComponent = GetComponent<StatsComponent>();
    }

    private void Start()
    {
        currentHealth = MaxHealth;
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
            currentHealth = Mathf.Clamp(currentHealth, 0f, newValue);
    }

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, bool isCritical)
    {
        if (IsDead || _isInvulnerable)
            return;

        float dmg = Mathf.Max(0f, amount);
        if (dmg <= 0f)
            return;

        currentHealth = Mathf.Max(0f, currentHealth - dmg);

        if (currentHealth <= 0f)
            Die();
    }
    public void TakeDamage(float amount)
    {
        TakeDamage(amount, Vector3.zero, Vector3.zero, false);
    }

    public void Heal(float amount)
    {
        if (IsDead)
            return;

        float heal = Mathf.Max(0f, amount);
        if (heal <= 0f)
            return;

        float before = currentHealth;
        currentHealth = Mathf.Min(MaxHealth, currentHealth + heal);
        Healed?.Invoke(currentHealth - before);
    }

    private void Die()
    {
        if (IsDead == false)
            currentHealth = 0f;

        Died?.Invoke(this);

        if (_destroyOnDeath)
            Destroy(gameObject);
    }

    public void Invulnerable(float duration)
    {
        StartCoroutine(InvulnerableCoroutine(duration));
    }

    private IEnumerator InvulnerableCoroutine(float duration)
    {
        _isInvulnerable = true;
        yield return new WaitForSeconds(duration);
        _isInvulnerable = false;
    }
}