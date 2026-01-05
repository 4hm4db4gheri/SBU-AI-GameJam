using UnityEngine;
using BigRookGames.Weapons;

[RequireComponent(typeof(GunfireController))]
public class Weapon : MonoBehaviour
{
    private GunfireController _gunfireController;
    [SerializeField] private float _fireRate = 0.5f;
    [SerializeField] private float _criticalHitChance = 0.1f;
    [SerializeField] private float _criticalHitMultiplier = 2f;
    [SerializeField] private float _damage = 10f;

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

    private void Start()
    {
        _gunfireController = GetComponent<GunfireController>();
        _mainCamera = Camera.main;
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
        if (_fireRate <= 0f)
            return true;

        if (Time.time < _nextTimeCanShoot)
            return false;

        _nextTimeCanShoot = Time.time + _fireRate;
        return true;
    }

    private void TryApplyHitscanDamage()
    {
        var ray = new Ray(_rayOriginOverride.position, _rayOriginOverride.forward);
        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, _hitMask))
            return;

        float finalDamage = ComputeDamage(out bool isCritical);

        if (hit.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(finalDamage, hit.point, hit.normal, isCritical);
            Debug.Log("Hit ");
            return;
        }
    }

    private float ComputeDamage(out bool isCritical)
    {
        isCritical = false;
        float dmg = Mathf.Max(0f, _damage);

        if (_criticalHitChance <= 0f || _criticalHitMultiplier <= 0f)
            return dmg;

        isCritical = UnityEngine.Random.value < Mathf.Clamp01(_criticalHitChance);
        return isCritical ? dmg * _criticalHitMultiplier : dmg;
    }

    private void OnDrawGizmos()
    {
        if (!_drawRayGizmos)
            return;

        float length = Mathf.Max(0.01f, _gizmoRayLength);

        // Always show weapon forward (what the weapon transform considers "shooting forward").
        Gizmos.color = _weaponForwardColor;
        Gizmos.DrawRay(_rayOriginOverride.position, _rayOriginOverride.forward * length);
    }
}

