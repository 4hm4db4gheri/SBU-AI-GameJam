using UnityEngine;

[RequireComponent(typeof(StatsComponent))]
[RequireComponent(typeof(Health))]
public sealed class EnemyAgent : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private StatsComponent _stats;
    [SerializeField] private StatDefinition _moveSpeedStat;

    [Header("Simple Behavior")]
    [SerializeField] private Transform _target;

    private void Awake()
    {
        if (_stats == null)
            _stats = GetComponent<StatsComponent>();
    }

    private void Update()
    {
        if (_target == null)
            return;

        float moveSpeed = _stats.Stats.GetValue(_moveSpeedStat, 2f);
        Vector3 toTarget = _target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.01f)
            return;

        Vector3 dir = toTarget.normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);
        transform.forward = dir;
    }
}

