using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private InputHandler _inputHandler;
    private CharacterController _characterController;
    [SerializeField] private Transform _playerShape;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [Header("Rotation While Shooting")]
    [SerializeField] private Camera _mainCamera;
    private float turnSmoothVelocity;
    private void Start()
    {
        _inputHandler = GetComponent<InputHandler>();
        _characterController = GetComponent<CharacterController>();
        if (_mainCamera == null) _mainCamera = Camera.main;
    }

    private void HandleMovement()
    {
        Vector2 moveInput = _inputHandler.move;
        Vector3 moveDirection = new(moveInput.x, 0, moveInput.y);
        _characterController.Move(moveSpeed * Time.deltaTime * moveDirection);
    }
    private void HandleRotation()
    {
        if (_inputHandler.shoot)
        {
            // if (_mainCamera == null || Mouse.current == null) return;

            // Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            // Ray ray = _mainCamera.ScreenPointToRay(mouseScreenPos);

            // // Intersect mouse ray with a horizontal plane at the player's height (works well for isometric aiming).
            // Plane plane = new(Vector3.up, new Vector3(0f, transform.position.y, 0f));
            // if (!plane.Raycast(ray, out float distance)) return;

            // Vector3 hitPoint = ray.GetPoint(distance);
            // Vector3 toHit = hitPoint - transform.position;
            // toHit.y = 0f;
            // if (toHit.sqrMagnitude < 0.0001f) return;

            // float targetAngle = Mathf.Atan2(toHit.x, toHit.z) * Mathf.Rad2Deg;
            // float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            // transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 mousePosition = new (0, 0, 0);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                mousePosition = hit.point;
            }

            Vector3 aimDirection = mousePosition - transform.position;
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
    
    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }


}
