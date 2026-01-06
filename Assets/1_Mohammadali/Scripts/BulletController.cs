using System;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float speed = 10.0f;
    public int damage;
    
    void Update()
    {
        transform.position += transform.forward * (Time.deltaTime * speed);
    }

    private void OnCollisionEnter(Collision other)
    {
        Rigidbody rb = other.collider.attachedRigidbody;
    
        GameObject hitObject = (rb != null) ? rb.gameObject : other.gameObject;
        
        if (hitObject.gameObject.CompareTag("Player"))
        {
            hitObject.gameObject.GetComponent<PlayerController>().Damage(damage);
            gameObject.SetActive(false);
        }
        else if(hitObject.gameObject.CompareTag("Enemy"))
        {
            hitObject.gameObject.GetComponent<EnemyController>().Damage(damage);
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
