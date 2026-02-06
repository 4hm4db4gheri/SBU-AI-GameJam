using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int _currentHealth;
    
    void Start()
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
