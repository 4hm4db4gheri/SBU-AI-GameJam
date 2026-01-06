using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class RangedEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float keepDistance = 8f;   // Ideal distance from target
    public float retreatDistance = 4f; // Back away if target gets this close
    public float detectionRadius = 30f;

    [Header("Combat Settings")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    private float nextFireTime;

    [Header("Dodge Settings")]
    public float dodgeForce = 10f;
    public float dodgeCooldown = 5f;
    private bool canDodge = true;

    private Transform target;
    private Rigidbody rb;

    private void OnEnable()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = Vector3.zero;
        StopAllCoroutines();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        InvokeRepeating("FindClosestTarget", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        HandleMovement(distance);
        RotateTowardsTarget();

        // Fire if within range and not retreating too frantically
        if (distance <= keepDistance + 2f && Time.time >= nextFireTime)
        {
            Shoot();
        }

        // Dodge if target is close or randomly while kiting
        if (distance < 7f && canDodge)
        {
            StartCoroutine(DodgeRoutine());
        }
    }

    void HandleMovement(float distance)
    {
        Vector3 direction = Vector3.zero;

        if (distance > keepDistance)
        {
            // Move Closer
            direction = (target.position - transform.position).normalized;
        }
        else if (distance < retreatDistance)
        {
            // Move Away (Retreat)
            direction = (transform.position - target.position).normalized;
        }
        else
        {
            // Stay still or "Strafing" could be added here
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        direction.y = 0;
        Vector3 moveVel = direction * moveSpeed;
        moveVel.y = rb.linearVelocity.y;
        rb.linearVelocity = moveVel;
    }

    void RotateTowardsTarget()
    {
        Vector3 lookDir = (target.position - transform.position);
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }

    void Shoot()
    {
        nextFireTime = Time.time + fireRate;
        if (projectilePrefab && firePoint)
        {
            ObjectPooler.Instance.SpawnFromPool("Bullet", firePoint.position, firePoint.rotation);
        }
    }

    void FindClosestTarget()
    {
        List<GameObject> potentialTargets = new List<GameObject>();
        potentialTargets.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        // potentialTargets.AddRange(GameObject.FindGameObjectsWithTag("Ally"));

        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (GameObject obj in potentialTargets)
        {
            float dist = Vector3.Distance(transform.position, obj.transform.position);
            if (dist < closestDistance && dist <= detectionRadius)
            {
                closestDistance = dist;
                bestTarget = obj.transform;
            }
        }

        target = bestTarget;
    }

    IEnumerator DodgeRoutine()
    {
        canDodge = false;
        
        // Lateral dodge (Side-stepping)
        Vector3 sideDir = Vector3.Cross((target.position - transform.position).normalized, Vector3.up);
        if (Random.value > 0.5f) sideDir *= -1;

        rb.AddForce(sideDir * dodgeForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }
}