using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RangedEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float keepDistance = 8f;   
    public float retreatDistance = 4f; 
    public float detectionRadius = 30f;

    [Header("Detection Settings")]
    public float viewAngle = 140f;    
    public LayerMask targetMask;      
    public LayerMask obstructionMask; 

    [Header("Memory Settings")]
    public float searchWaitTime = 2f;
    private Vector3 lastKnownPosition;
    private bool isSearching = false;

    [Header("Combat Settings")]
    public string bulletPoolTag = "Bullet";
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
        if (rb != null) rb.linearVelocity = Vector3.zero;
        target = null;
        isSearching = false;
        canDodge = true;
        StopAllCoroutines();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        InvokeRepeating("FindVisibleTarget", 0f, 0.2f);
    }

    void FixedUpdate()
    {
        // STATE 1: ACTIVE TARGET IN SIGHT
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            HandleMovement(distance, target.position);
            RotateTowards(target.position);

            if (distance <= detectionRadius && Time.time >= nextFireTime)
            {
                Shoot();
            }

            if (distance < 7f && canDodge)
            {
                StartCoroutine(DodgeRoutine());
            }
        }
        // STATE 2: SEARCHING LAST KNOWN POSITION
        else if (isSearching)
        {
            float distToMemory = Vector3.Distance(transform.position, lastKnownPosition);
            
            if (distToMemory > 1.5f)
            {
                MoveDirectlyTo(lastKnownPosition);
                RotateTowards(lastKnownPosition);
            }
            else
            {
                // Reached the spot, wait and then give up
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
                StartCoroutine(SearchExhaustion());
            }
        }
        // STATE 3: IDLE
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void FindVisibleTarget()
    {
        Collider[] targetsInRadius = Physics.OverlapSphere(transform.position, detectionRadius, targetMask);
        float closestDistance = Mathf.Infinity;
        Transform bestTarget = null;

        foreach (Collider t in targetsInRadius)
        {
            Transform potentialTarget = t.transform;
            Vector3 dirToTarget = (potentialTarget.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(transform.position, potentialTarget.position);

                if (!Physics.Raycast(transform.position + Vector3.up, dirToTarget, distToTarget, obstructionMask))
                {
                    if (distToTarget < closestDistance)
                    {
                        closestDistance = distToTarget;
                        bestTarget = potentialTarget;
                    }
                }
            }
        }

        if (bestTarget != null)
        {
            target = bestTarget;
            lastKnownPosition = target.position; // Keep memory fresh
            isSearching = false;
        }
        else if (target != null)
        {
            // Just lost sight
            target = null;
            isSearching = true;
        }
    }

    void HandleMovement(float distance, Vector3 targetPos)
    {
        Vector3 direction = Vector3.zero;

        if (distance > keepDistance)
            direction = (targetPos - transform.position).normalized;
        else if (distance < retreatDistance)
            direction = (transform.position - targetPos).normalized;
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        ApplyVelocity(direction);
    }

    void MoveDirectlyTo(Vector3 position)
    {
        Vector3 direction = (position - transform.position).normalized;
        ApplyVelocity(direction);
    }

    void ApplyVelocity(Vector3 direction)
    {
        direction.y = 0;
        Vector3 moveVel = direction * moveSpeed;
        moveVel.y = rb.linearVelocity.y;
        rb.linearVelocity = moveVel;
    }

    void RotateTowards(Vector3 position)
    {
        Vector3 lookDir = (position - transform.position);
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8f);
        }
    }

    void Shoot()
    {
        Vector3 dirToTarget = (target.position - firePoint.position).normalized;
        if (!Physics.Raycast(firePoint.position, dirToTarget, Vector3.Distance(firePoint.position, target.position), obstructionMask))
        {
            nextFireTime = Time.time + fireRate;
            ObjectPooler.Instance.SpawnFromPool(bulletPoolTag, firePoint.position, firePoint.rotation);
        }
    }

    IEnumerator SearchExhaustion()
    {
        yield return new WaitForSeconds(searchWaitTime);
        if (target == null) isSearching = false;
    }

    IEnumerator DodgeRoutine()
    {
        if (target == null) yield break;
        canDodge = false;
        Vector3 sideDir = Vector3.Cross((target.position - transform.position).normalized, Vector3.up);
        if (Random.value > 0.5f) sideDir *= -1;
        rb.AddForce(sideDir * dodgeForce, ForceMode.Impulse);
        yield return new WaitForSeconds(dodgeCooldown);
        canDodge = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        if (isSearching)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, lastKnownPosition);
            Gizmos.DrawWireCube(lastKnownPosition, Vector3.one);
        }
    }
}