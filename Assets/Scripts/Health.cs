using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public float maxHealth = 100f;
    public float currentHealth;

    // اگر این تیک خورده باشد، یعنی این آبجکت دشمن است و باید یاد بگیرد
    public bool isAI = false;

    private EnemyBrain brain; // برای یادگیری (اگر دشمن بود)

    void Start()
    {
        currentHealth = maxHealth;
        if (isAI)
        {
            brain = GetComponent<EnemyBrain>();
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        // اگر دشمن ضربه خورد، کمی تنبیه شود (پاداش منفی کوچک)
        // تا یاد بگیرد از ضربه خوردن اجتناب کند (در آینده)
        if (isAI && brain != null)
        {
            // فعلا فقط لاگ می‌گیریم، بعدا در سیستم Flee استفاده می‌شود
            // brain.Learn(..., -1f); 
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void TakeDamage(float amount, Vector3 hitPoint, Vector3 hitNormal, bool isCritical)
    {
        TakeDamage(amount);
        PlayHitEffect(amount, isCritical);
    }

    void PlayHitEffect(float amount, bool isCritical)
    {
        if (isCritical) { }
        else { }
    }

    void Die()
    {
        if (isAI && brain != null)
        {
            // مرگ = تنبیه بزرگ!
            // این باعث می‌شود دشمن بفهمد کاری که منجر به مرگش شد اشتباه بوده
            // اما چون الان مرده و آبجکتش حذف می‌شود، این یادگیری باید در متغیرهای استاتیک brain ذخیره شود
            Debug.Log(gameObject.name + " Died and learned a hard lesson!");
        }

        ExperienceManager.Instance.AddExperience(10);

        // اگر پلیر یا یار مرد هم لاگ بده
        Debug.Log(gameObject.name + " was destroyed.");

        Destroy(gameObject);
    }

}