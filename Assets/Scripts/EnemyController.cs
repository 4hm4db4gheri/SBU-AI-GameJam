using UnityEngine;
using UnityEngine.AI;
using TMPro; // 1. اضافه شدن کتابخانه UI

[RequireComponent(typeof(EnemyBrain))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Health))]
public class EnemyController : MonoBehaviour
{
    // تنظیمات نوع دشمن
    public enum EnemyType { Melee, Ranged }
    public EnemyType type = EnemyType.Melee;

    [Header("Combat Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float meleeAttackRange = 2f;
    public float rangeAttackRange = 2f;
    public float attackCooldown = 1.5f;

    [Header("UI Settings")]
    public TMP_Text statusText; // 2. متغیر برای اتصال متن بالای سر

    // متغیرهای داخلی
    private EnemyBrain brain;
    private NavMeshAgent agent;
    private Health myHealth; // 3. دسترسی به جان خود دشمن

    private Transform currentTarget;
    private Transform playerTransform; // ذخیره موقعیت پلیر برای فرار

    private EnemyBrain.State currentState;
    private EnemyBrain.Action currentAction;

    private float lastAttackTime = 0;
    private float thinkTimer = 0;

    void Start()
    {
        brain = GetComponent<EnemyBrain>();
        agent = GetComponent<NavMeshAgent>();
        myHealth = GetComponent<Health>(); // گرفتن کامپوننت Health

        if (type == EnemyType.Ranged)
        {
            agent.stoppingDistance = rangeAttackRange;
        }
        else
        {
            agent.stoppingDistance = meleeAttackRange;
        }
    }

    void Update()
    {
        // تصمیم‌گیری هر 0.5 ثانیه
        thinkTimer += Time.deltaTime;
        if (thinkTimer > 0.5f)
        {
            DecideTarget();
            UpdateUI(); // آپدیت کردن متن بالای سر
            thinkTimer = 0;
        }

        // اجرای رفتارها (Action Execution)

        // حالت اول: فرار
        if (currentAction == EnemyBrain.Action.Flee)
        {
            if (playerTransform != null)
            {
                // محاسبه جهت مخالف پلیر
                Vector3 dirToPlayer = transform.position - playerTransform.position;
                Vector3 fleePos = transform.position + dirToPlayer.normalized * 5f; // 5 متر دور شو
                agent.SetDestination(fleePos);
            }
        }
        // حالت دوم: حمله (کد قبلی)
        else if (currentTarget != null)
        {
            float radius = (type == EnemyType.Ranged) ? rangeAttackRange : meleeAttackRange;

            // Sphere: center = currentTarget.position, radius = attack range
            // Destination = closest point on that sphere to the agent
            Vector3 direction = transform.position - currentTarget.position;
            direction.Normalize();
            Vector3 destination = new Vector3(currentTarget.position.x + direction.x * radius, currentTarget.position.y, currentTarget.position.z + direction.z * radius);

            float distance = Vector3.Distance(transform.position, destination);

            if (distance > 1)
            {
                // Use a small stopping distance so the agent actually reaches the blue sphere (attack position).
                // Otherwise with stoppingDistance = 2, the agent stops 2m short and never gets there.
                agent.stoppingDistance = 0.2f;
                agent.SetDestination(destination);
            }
            else
            {
                // Back at attack range; restore stopping distance for next approach if target moves
                if (type == EnemyType.Ranged)
                    agent.stoppingDistance = rangeAttackRange;
                else
                    agent.stoppingDistance = meleeAttackRange;

                // چرخش به سمت هدف
                Vector3 lookPos = currentTarget.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);

                if (Time.time > lastAttackTime + attackCooldown)
                {
                    PerformAttack();
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    void DecideTarget()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        GameObject a = GameObject.FindGameObjectWithTag("Ally");

        if (p) playerTransform = p.transform; // ذخیره پلیر برای محاسبات فرار

        float distP = p ? Vector3.Distance(transform.position, p.transform.position) : 999;
        float distA = a ? Vector3.Distance(transform.position, a.transform.position) : 999;

        // 4. منطق تشخیص وضعیت جدید (شامل چک کردن جان)
        currentState = EnemyBrain.State.Confused;

        if (myHealth != null && myHealth.currentHealth < 30f) // اگر جان زیر 30 بود
        {
            currentState = EnemyBrain.State.LowHealth;
        }
        else if (distP < distA)
        {
            currentState = EnemyBrain.State.NearPlayer;
        }
        else
        {
            currentState = EnemyBrain.State.NearAlly;
        }

        // تصمیم‌گیری مغز
        currentAction = brain.ChooseAction(currentState);

        // ست کردن هدف بر اساس تصمیم
        if (currentAction == EnemyBrain.Action.AttackPlayer) currentTarget = p ? p.transform : null;
        else if (currentAction == EnemyBrain.Action.AttackAlly) currentTarget = a ? a.transform : null;
        else currentTarget = null; // در حالت فرار هدف خاصی برای حمله نداریم
    }

    void PerformAttack()
    {
        float reward = 0;

        // اگر قرار بود حمله کنیم ولی داریم فرار میکنیم، نباید حمله انجام بشه
        if (currentAction == EnemyBrain.Action.Flee) return;

        if (type == EnemyType.Melee)
        {
            Debug.Log(gameObject.name + " Punched " + currentTarget.name);
            // وارد کردن دمیج واقعی
            PlayerHealth targetHealth = currentTarget.GetComponent<PlayerHealth>();
            if (targetHealth) targetHealth.TakeDamage(10);

            reward = (currentTarget.CompareTag("Player")) ? 10f : 2f;
        }
        else // Ranged
        {
            if (bulletPrefab != null)
            {
                Vector3 spawnPos = transform.position + transform.forward * 1.0f + Vector3.up * 0.5f;
                Instantiate(bulletPrefab, spawnPos, transform.rotation);
            }
            reward = (currentTarget.CompareTag("Player")) ? 10f : 2f;
        }

        brain.Learn(currentState, currentAction, reward);
    }

    // 5. نمایش وضعیت در UI
    void UpdateUI()
    {
        if (statusText != null)
        {
            statusText.text = $"{currentState}\n{currentAction}";

            // تغییر رنگ متن برای زیبایی
            if (currentState == EnemyBrain.State.LowHealth) statusText.color = Color.red;
            else if (currentAction == EnemyBrain.Action.AttackPlayer) statusText.color = Color.yellow;
            else statusText.color = Color.white;
        }
    }

    void OnDrawGizmos()
    {
        if (currentTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentTarget.position, meleeAttackRange);
            Gizmos.color = Color.blue;
            Vector3 direction = transform.position - currentTarget.position;
            direction.Normalize();
            Vector3 destination = new(currentTarget.position.x + direction.x * agent.stoppingDistance, currentTarget.position.y, currentTarget.position.z + direction.z * agent.stoppingDistance);
            Gizmos.DrawWireSphere(destination, 0.1f);

            Gizmos.color = Color.beige;
            Gizmos.DrawLine(transform.position, destination);
        }
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
    }
}