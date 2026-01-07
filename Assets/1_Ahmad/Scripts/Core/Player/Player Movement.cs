using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(StatsComponent))]
[RequireComponent(typeof(PlayerAnimations))]
[RequireComponent(typeof(Health))]
public class PlayerMovement : MonoBehaviour
{
    private InputHandler _inputHandler;
    private CharacterController _characterController;
    private PlayerAnimations _playerAnimations;
    private Health _health;
    [Header("Aim (match weapon gizmo)")]
    [Tooltip("If set, player rotation while aiming/shooting will match this weapon's aim ray direction (green gizmo ray). If null, a Weapon will be found in children.")]
    [SerializeField] private Weapon _weaponForAim;

    [Header("Stats")]
    [SerializeField] private StatsComponent _statsComponent;
    [SerializeField] private StatDefinition _moveSpeedStat;
    [SerializeField] private StatDefinition _moveSpeedWhileShootingStat;
    [SerializeField] private StatDefinition _rollCooldownStat;

    [Header("Roll")]
    [SerializeField] private AnimationClip _rollAnimation;
    private float _rollSpeed;
    private bool _canRoll = true;
    private bool _isRolling = false;
    public bool IsRolling { get => _isRolling; }
    private float _rollTimeRemaining = 0f;
    private float RollTimer = 0f;
    private Vector3 _rollDirection = Vector3.forward;
    private bool _rollPressedLastFrame = false;
    [Header("Rotation")]
    [SerializeField] private float turnSmoothTime = 0.1f;
    [Header("Rotation While Shooting")]
    [SerializeField] private Camera _mainCamera;
    private float turnSmoothVelocity;
    private void Start()
    {
        _inputHandler = GetComponent<InputHandler>();
        _characterController = GetComponent<CharacterController>();
        _playerAnimations = GetComponent<PlayerAnimations>();
        _health = GetComponent<Health>();
        if (_statsComponent == null) _statsComponent = GetComponent<StatsComponent>();
        if (_mainCamera == null) _mainCamera = Camera.main;
        if (_weaponForAim == null) _weaponForAim = GetComponentInChildren<Weapon>();
    }

    private void HandleMovement()
    {
        if (_isRolling) return;
        Vector2 moveInput = _inputHandler.move;
        Vector3 moveDirection = new(moveInput.x, 0, moveInput.y);
        float moveSpeed = GetCurrentMoveSpeed();
        _characterController.Move(moveSpeed * Time.deltaTime * moveDirection);
    }

    private float GetCurrentMoveSpeed()
    {
        if (_inputHandler != null && _inputHandler.shoot)
        {
            return _statsComponent.Stats.GetValue(_moveSpeedWhileShootingStat, 10f);
        }

        return _statsComponent.Stats.GetValue(_moveSpeedStat, 15f);
    }
    private void HandleRotation()
    {
        if (_isRolling) return;

        // While aiming/shooting, match the weapon's aim direction exactly (angle to green gizmo ray becomes 0).
        if (_inputHandler.aim || _inputHandler.shoot)
        {
            if (_weaponForAim == null) return;

            Ray aimRay = _weaponForAim.GetAimRay();
            Vector3 aimDirection = aimRay.direction;
            aimDirection.y = 0f;
            if (aimDirection.sqrMagnitude < 0.0001f) return;
            aimDirection.Normalize();

            float targetAngle = Mathf.Atan2(aimDirection.x, aimDirection.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            return;
        }

        if (_inputHandler.move.magnitude == 0) return;

        float moveTargetAngle = Mathf.Atan2(_inputHandler.move.x, _inputHandler.move.y) * Mathf.Rad2Deg;
        float moveAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, moveTargetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, moveAngle, 0f);
    }

    private void HandleRoll()
    {
        // Trigger only on press (not while holding)
        bool rollPressedThisFrame = _inputHandler.roll && !_rollPressedLastFrame;
        _rollPressedLastFrame = _inputHandler.roll;

        if (_isRolling)
        {
            _rollSpeed = _statsComponent.Stats.GetValue(_moveSpeedStat, 15f) * 0.75f;
            _characterController.Move(_rollSpeed * Time.deltaTime * _rollDirection);
            _rollTimeRemaining -= Time.deltaTime;
            if (_rollTimeRemaining <= 0f)
            {
                _isRolling = false;
            }
        }

        if (!_canRoll)
        {
            RollTimer += Time.deltaTime;
            if (RollTimer >= GetRollCooldown())
            {
                _canRoll = true;
                _playerAnimations.RollAnimationPlaying = false;
            }
        }

        if (rollPressedThisFrame && _canRoll && !_isRolling)
        {
            StartRoll();
        }
    }

    private void StartRoll()
    {
        if (_rollAnimation == null) return;

        _health.Invulnerable(_rollAnimation.length);
        _rollSpeed = GetCurrentMoveSpeed() * 2f;

        Vector2 moveInput = _inputHandler.move;
        Vector3 inputDirection = new(moveInput.x, 0f, moveInput.y);

        // Prefer Roll in input direction (works well for strafing while shooting),
        // otherwise Roll forward from current facing.
        _rollDirection = inputDirection.sqrMagnitude > 0.001f ? inputDirection.normalized : transform.forward.normalized;

        // Ensure the character is facing the roll direction before the roll starts.
        // This prevents cases where movement input changed but rotation smoothing hasn't caught up yet.
        if (_rollDirection.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(_rollDirection, Vector3.up);
        }

        _isRolling = true;
        _rollTimeRemaining = _rollAnimation.length;
        _canRoll = false;
        RollTimer = 0f;
    }

    private float GetRollCooldown()
    {
        if (_rollCooldownStat == null) return 1f;
        if (_statsComponent == null) return _rollCooldownStat.DefaultBaseValue;
        return _statsComponent.Stats.GetValue(_rollCooldownStat, _rollCooldownStat.DefaultBaseValue);
    }

    private void Update()
    {
        HandleRoll();
        HandleMovement();
        HandleRotation();
    }

    //! Upgrade example for move speed
    // public void UpgradeMoveSpeed()
    // {
    //     if (_moveSpeedStat.HasDefaultUpgrade)
    //         _statsComponent.Stats.AddModifier(_moveSpeedStat, _moveSpeedStat.CreateDefaultUpgradeModifier(source: this));
    // }


}
