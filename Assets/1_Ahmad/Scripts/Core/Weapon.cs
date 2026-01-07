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
    [SerializeField] private Transform _frontGun;
    [SerializeField] private bool _useBulletPrefab = false;
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 100f;
    [SerializeField] private float _bulletLifeTime = 1f;
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

        if (_useBulletPrefab)
        {
            FireBulletPrefab();
            return;
        }

        TryApplyHitscanDamage();
    }

    /// <summary>
    /// Returns the current aim ray used by this weapon (same as the debug gizmo ray).
    /// Useful for rotating the player/character so their forward matches the weapon aim direction.
    /// </summary>
    public Ray GetAimRay()
    {
        GetRay(out var ray);
        return ray;
    }

    private void FireBulletPrefab()
    {
        if (_bulletPrefab == null)
            return;

        GetRay(out var ray);
        float finalDamage = ComputeDamage(out bool isCritical);

        Quaternion rot = ray.direction.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(ray.direction, Vector3.up)
            : (_frontGun != null ? _frontGun.rotation : transform.rotation);

        GameObject bullet = Instantiate(_bulletPrefab, ray.origin, rot);

        var bulletComponent = bullet.GetComponent<Bullet>();
        if (bulletComponent == null)
            bulletComponent = bullet.AddComponent<Bullet>();

        bulletComponent.Initialize(finalDamage, isCritical, ownerRoot: transform.root, hitMask: _hitMask);

        // If the prefab has no Rigidbody, add one so it can move forward.
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = bullet.AddComponent<Rigidbody>();
            rb.useGravity = false;
        }

        rb.linearVelocity = ray.direction.normalized * Mathf.Max(0f, _bulletSpeed);

        float life = Mathf.Max(0.01f, _bulletLifeTime);
        Destroy(bullet, life);
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
        // Choose ray origin (gun height reference)
        Vector3 origin;
        if (_frontGun != null)
        {
            origin = _frontGun.position;
        }
        else if (_gunfireController != null && _gunfireController.muzzlePosition != null)
        {
            origin = _gunfireController.muzzlePosition.transform.position;
        }
        else
        {
            origin = transform.position;
        }

        if (_mainCamera == null)
            _mainCamera = Camera.main;

        // If we have a camera, aim toward mouse world position (but keep gun height).
        if (_mainCamera != null)
        {
            Ray mouseRay = _mainCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 aimPoint;
            if (Physics.Raycast(mouseRay, out var aimHit, 5000f, _hitMask))
            {
                aimPoint = aimHit.point;
            }
            else
            {
                // Fallback: intersect with a horizontal plane at gun height.
                var plane = new Plane(Vector3.up, new Vector3(0f, origin.y, 0f));
                if (plane.Raycast(mouseRay, out float enter))
                    aimPoint = mouseRay.GetPoint(enter);
                else
                    aimPoint = origin + transform.forward;
            }

            aimPoint.y = origin.y;
            Vector3 dir = aimPoint - origin;
            if (dir.sqrMagnitude < 0.0001f)
                dir = transform.forward;

            ray = new Ray(origin, dir.normalized);
            return;
        }

        // No camera available: fallback to gun/weapon forward.
        Vector3 forward = _frontGun != null ? _frontGun.forward : transform.forward;
        ray = new Ray(origin, forward);
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

