using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private float _health = 100f;

    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, bool isCritical)
    {
        _health -= amount;
        if (_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}