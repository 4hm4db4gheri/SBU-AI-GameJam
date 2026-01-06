using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float stopDistance = 2f;
    public float detectionRadius = 25f;

    [Header("Combat Settings")]
    public float attackDamage = 10f;
    public float attackRate = 1.5f;
    private float nextAttackTime;

    [Header("Dodge Settings")]
    public float dodgeForce = 12f;
    public float dodgeCooldown = 4f;
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
        // Freeze rotation so the physics engine doesn't tip the enemy over
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        InvokeRepeating("FindClosestTarget", 0f, 0.5f);
    }

    void FixedUpdate()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (distance > stopDistance)
        {
            MoveTowardsTarget();
        }
        else
        {
            // Stop moving when in range to attack
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
            
            if (Time.time >= nextAttackTime)
            {
                Attack();
            }
        }

        // Dodge logic
        if (distance < 6f && canDodge)
        {
            StartCoroutine(DodgeRoutine());
        }
    }

    void FindClosestTarget()
    {
        // Find all potential targets (Player and Allies)
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

    void MoveTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0; // Keep movement on the horizontal plane
        direction.Normalize();

        // Movement
        Vector3 moveVelocity = direction * moveSpeed;
        moveVelocity.y = rb.linearVelocity.y; // Preserve gravity
        rb.linearVelocity = moveVelocity;

        // Rotation: Look at target
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    void Attack()
    {
        nextAttackTime = Time.time + attackRate;
        Debug.Log("Melee Hit on: " + target.name);
        // Apply damage to target script here
    }

    IEnumerator DodgeRoutine()
    {
        canDodge = false;

        // Calculate a side-step (cross product of up and direction to target)
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        Vector3 sideDir = Vector3.Cross(directionToTarget, Vector3.up);

        // Randomly pick left or right
        if (Random.value > 0.5f) sideDir *= -1;

        // Apply impulse
        rb.AddForce(sideDir * dodgeForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }
}