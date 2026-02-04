using UnityEngine;

public class BulletMover : MonoBehaviour
{
    public float speed = 10f;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
        Destroy(gameObject, 2f); // بعد از 2 ثانیه حذف شود تا صحنه شلوغ نشود
    }
    void OnTriggerEnter(Collider other)
    {
        // سعی کن کامپوننت Health را از چیزی که بهش خوردی بگیری
        Health targetHealth = other.GetComponent<Health>();

        if (targetHealth != null)
        {
            // اگر جان داشت، ۱۰ تا کم کن
            targetHealth.TakeDamage(10f);
            Debug.Log("Bullet hit " + other.name);
            Destroy(gameObject); // گلوله نابود شود
        }
        else if (!other.CompareTag("Enemy")) // اگر به دیوار خورد (و دشمن نبود)
        {
            Destroy(gameObject);
        }
    }
}