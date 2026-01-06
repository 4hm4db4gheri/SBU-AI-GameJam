using UnityEngine;
using BigRookGames.Weapons;

[RequireComponent(typeof(GunfireController))]
public class Weapon : MonoBehaviour
{
    private GunfireController _gunfireController;

    [Header("Stats (Weapon)")]
    [SerializeField] private StatsComponent _statsComponent;
    [SerializeField] private StatDefinition _damageStat;
    [SerializeField] private StatDefinition _critChanceStat;
    [SerializeField] private StatDefinition _critMultiplierStat;
    [Tooltip("Interpretation: attacks per second. Cooldown = 1 / APS.")]
    [SerializeField] private StatDefinition _attacksPerSecondStat;
    [SerializeField] private StatDefinition _rangeStat;

    [Header("Optional Stats (Owner)")]
    [Tooltip("If assigned and an owner StatsContainer is set, final damage is multiplied by this value (base should usually be 1).")]
    [SerializeField] private StatDefinition _ownerDamageMultiplierStat;

    [Header("Hitscan")]
    [Tooltip("If set, raycast will start from here. Otherwise Camera.main is used (fallback: muzzlePosition if available).")]
    [SerializeField] private Transform _rayOriginOverride;
    [Tooltip("Raycast direction comes from Camera.main forward by default; disable to use origin forward.")]
    [SerializeField] private LayerMask _hitMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool _drawRayGizmos = true;
    [SerializeField] private Color _weaponForwardColor = Color.green;
    [SerializeField] private float _gizmoRayLength = 25f;

    private float _nextTimeCanShoot;
    private Camera _mainCamera;
    private StatsContainer _ownerStats;

    private void Start()
    {
        _gunfireController = GetComponent<GunfireController>();
        if (_statsComponent == null)
            _statsComponent = GetComponent<StatsComponent>();
        _mainCamera = Camera.main;
    }

    public void SetOwnerStats(StatsContainer ownerStats)
    {
        _ownerStats = ownerStats;
    }

    internal void Shoot()
    {
        if (!CanShootNow())
            return;

        _gunfireController.FireWeapon();
        TryApplyHitscanDamage();
    }

    private bool CanShootNow()
    {
        float attacksPerSecond = GetWeaponStat(_attacksPerSecondStat, fallback: 2f);
        if (attacksPerSecond <= 0f)
            return true;

        float cooldown = 1f / attacksPerSecond;

        if (Time.time < _nextTimeCanShoot)
            return false;

        _nextTimeCanShoot = Time.time + cooldown;
        return true;
    }

    private void TryApplyHitscanDamage()
    {
        GetRay(out var ray);
        float range = Mathf.Max(0.01f, GetWeaponStat(_rangeStat, fallback: 50f));

        if (!Physics.Raycast(ray, out var hit, range, _hitMask))
            return;

        float finalDamage = ComputeDamage(out bool isCritical);

        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(finalDamage, hit.point, hit.normal, isCritical);
            return;
        }
    }

    private float ComputeDamage(out bool isCritical)
    {
        isCritical = false;
        float dmg = Mathf.Max(0f, GetWeaponStat(_damageStat, fallback: 10f));

        float ownerDamageMult = 1f;
        if (_ownerStats != null && _ownerDamageMultiplierStat != null)
            ownerDamageMult = Mathf.Max(0f, _ownerStats.GetValue(_ownerDamageMultiplierStat, 1f));

        dmg *= ownerDamageMult;

        float critChance = Mathf.Clamp01(GetWeaponStat(_critChanceStat, fallback: 0.1f));
        float critMultiplier = Mathf.Max(1f, GetWeaponStat(_critMultiplierStat, fallback: 2f));

        if (critChance <= 0f)
            return dmg;

        isCritical = UnityEngine.Random.value < critChance;
        return isCritical ? dmg * critMultiplier : dmg;
    }

    private float GetWeaponStat(StatDefinition def, float fallback)
    {
        if (_statsComponent == null)
            return fallback;

        return _statsComponent.Stats.GetValue(def, fallback);
    }

    private void GetRay(out Ray ray)
    {
        if (_rayOriginOverride != null)
        {
            ray = new Ray(_rayOriginOverride.position, _rayOriginOverride.forward);
            return;
        }

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        if (_mainCamera != null)
        {
            ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            return;
        }

        if (_gunfireController != null && _gunfireController.muzzlePosition != null)
        {
            var t = _gunfireController.muzzlePosition.transform;
            ray = new Ray(t.position, t.forward);
            return;
        }

        ray = new Ray(transform.position, transform.forward);
    }

    private void OnDrawGizmos()
    {
        if (!_drawRayGizmos)
            return;

        float length = Mathf.Max(0.01f, _gizmoRayLength);
        GetRay(out var ray);

        // Always show weapon forward (what the weapon uses for hitscan).
        Gizmos.color = _weaponForwardColor;
        Gizmos.DrawRay(ray.origin, ray.direction * length);
    }
}
