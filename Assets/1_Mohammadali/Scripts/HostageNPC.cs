using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HostageNPC : MonoBehaviour
{
    [Header("Rescue Settings")]
    public float rescueTime = 2f;
    public float detectionRadius = 3f;
    private float currentRescueTimer = 0f;
    private bool isRescued = false;

    [Header("Follow Settings")]
    public float followRadius = 4f; // Radius around player to stay in
    public float moveSpeed = 5f;
    
    [Header("Combat")]
    public float fireRate = 1f;
    private float nextFireTime;
    public GameObject bulletPrefab;
    public Transform firePoint;

    private Transform player;
    private Rigidbody rb;
    public Image bar;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        bar.type = Image.Type.Filled;
        bar.fillMethod = Image.FillMethod.Horizontal;
    }

    void Update()
    {
        if (!isRescued)
        {
            CheckForPlayer();
        }
        else
        {
            FollowPlayer();
            ShootAtEnemies();
        }
    }

    void CheckForPlayer()
    {
        // Find player if not found yet
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detectionRadius)
        {
            currentRescueTimer += Time.deltaTime;
            bar.fillAmount = currentRescueTimer / rescueTime;
            if (currentRescueTimer >= rescueTime)
            {
                Rescue();
            }
        }
        else
        {
            currentRescueTimer = 0f; // Reset if player leaves radius
        }
    }

    void Rescue()
    {
        isRescued = true;
        this.tag = "Ally"; // Change tag so enemies target them and bullets recognize them
        Debug.Log("Hostage Rescued!");
        // Add visual feedback here (e.g., change color or play sound)
    }

    void FollowPlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        
        if (dist > followRadius)
        {
            Vector3 moveDir = (player.position - transform.position).normalized;
            moveDir.y = 0;
            rb.linearVelocity = moveDir * moveSpeed;
            
            // Rotate towards movement
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDir), Time.deltaTime * 10f);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    void ShootAtEnemies()
    {
        // Find nearest enemy
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closestEnemy = null;
        float minDist = 15f; // Max shooting range

        foreach (GameObject e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < minDist)
            {
                minDist = d;
                closestEnemy = e.transform;
            }
        }

        if (closestEnemy != null)
        {
            // Aim at enemy
            Vector3 lookDir = (closestEnemy.position - transform.position).normalized;
            lookDir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 5f);

            if (Time.time >= nextFireTime)
            {
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                nextFireTime = Time.time + fireRate;
            }
        }
    }
}