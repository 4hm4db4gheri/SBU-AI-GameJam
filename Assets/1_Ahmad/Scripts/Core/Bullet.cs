using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Runtime")]
    [SerializeField] private float _damage;
    [SerializeField] private bool _isCritical;
    [SerializeField] private LayerMask _hitMask = ~0;

    private Transform _ownerRoot;
    private bool _hasHit;

    /// <summary>
    /// Called by the weapon right after instantiation.
    /// </summary>
    public void Initialize(float damage, bool isCritical, Transform ownerRoot, LayerMask hitMask)
    {
        _damage = damage;
        _isCritical = isCritical;
        _ownerRoot = ownerRoot;
        _hitMask = hitMask;

        // Prevent immediately colliding with the shooter (common when muzzle is inside the player collider).
        if (_ownerRoot != null)
            IgnoreOwnerCollisions();
    }

    private void IgnoreOwnerCollisions()
    {
        var ownerColliders = _ownerRoot.GetComponentsInChildren<Collider>();
        var bulletColliders = GetComponentsInChildren<Collider>();

        for (int i = 0; i < bulletColliders.Length; i++)
        {
            var b = bulletColliders[i];
            if (b == null) continue;

            for (int j = 0; j < ownerColliders.Length; j++)
            {
                var o = ownerColliders[j];
                if (o == null) continue;
                Physics.IgnoreCollision(b, o, true);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_hasHit) return;
        HandleHit(collision.collider, collision.GetContact(0).point, collision.GetContact(0).normal);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;

        // Trigger collisions don't give reliable normal; approximate.
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitNormal = (transform.position - hitPoint);
        if (hitNormal.sqrMagnitude < 0.0001f) hitNormal = -transform.forward;
        else hitNormal.Normalize();

        HandleHit(other, hitPoint, hitNormal);
    }

    private void HandleHit(Collider other, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (other == null)
            return;

        // Layer filtering (optional)
        if (((1 << other.gameObject.layer) & _hitMask.value) == 0)
            return;

        // Don't hit the shooter
        if (_ownerRoot != null && other.transform.IsChildOf(_ownerRoot))
            return;

        _hasHit = true;

        if (other.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(_damage, hitPoint, hitNormal, _isCritical);
        }

        Destroy(gameObject);
    }
}
