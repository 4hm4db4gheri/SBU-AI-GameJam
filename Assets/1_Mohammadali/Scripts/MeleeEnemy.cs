using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeleeEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float stopDistance = 2f;
    public float detectionRadius = 25f;

    [Header("Detection Settings")]
    public float viewAngle = 120f;
    public LayerMask targetMask;      
    public LayerMask obstructionMask; 

    [Header("Memory Settings")]
    public float searchWaitTime = 2f;
    private Vector3 lastKnownPosition;
    private bool isSearching = false;

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
        if(rb != null) rb.linearVelocity = Vector3.zero;
        target = null; 
        isSearching = false;
        canDodge = true;
        StopAllCoroutines();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
        // Scan for visible targets 5 times a second
        InvokeRepeating("FindVisibleTarget", 0f, 0.2f);
    }

    void FixedUpdate()
    {
        // STATE 1: TARGET IN SIGHT
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);

            if (distance > stopDistance)
            {
                MoveTo(target.position);
            }
            else
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); 
                if (Time.time >= nextAttackTime) Attack();
            }

            if (distance < 6f && canDodge) StartCoroutine(DodgeRoutine());
        }
        // STATE 2: SEARCHING LAST KNOWN POSITION
        else if (isSearching)
        {
            float distToMemory = Vector3.Distance(transform.position, lastKnownPosition);
            
            if (distToMemory > 1.2f) // Arrival threshold
            {
                MoveTo(lastKnownPosition);
            }
            else
            {
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
            lastKnownPosition = target.position; // Refresh memory
            isSearching = false;
        }
        else if (target != null)
        {
            // Lost sight, start searching
            target = null;
            isSearching = true;
        }
    }

    void MoveTo(Vector3 position)
    {
        Vector3 direction = (position - transform.position);
        direction.y = 0;
        direction.Normalize();

        Vector3 moveVelocity = direction * moveSpeed;
        moveVelocity.y = rb.linearVelocity.y;
        rb.linearVelocity = moveVelocity;

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
        // target.GetComponent<Health>().TakeDamage(attackDamage);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        if (isSearching)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, lastKnownPosition);
            Gizmos.DrawWireCube(lastKnownPosition, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}