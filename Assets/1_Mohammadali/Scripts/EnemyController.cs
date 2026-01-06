using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth = 100;
    
    void OnEnable()
    {
        _currentHealth = maxHealth;
    }

    public void Damage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        gameObject.SetActive(false);
    }

}
