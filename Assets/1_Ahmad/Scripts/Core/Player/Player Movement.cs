using UnityEngine;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private InputHandler _inputHandler;
    private CharacterController _characterController;
    [SerializeField] private Transform _playerShape;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;
    private void Start()
    {
        _inputHandler = GetComponent<InputHandler>();
        _characterController = GetComponent<CharacterController>();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = _inputHandler.move;
        Vector3 moveDirection = new(moveInput.x, 0, moveInput.y);
        _characterController.Move(moveSpeed * Time.deltaTime * moveDirection);
    }
    private void HandleRotation()
    {
        if(_inputHandler.move.magnitude == 0) return;
        float targetAngle = Mathf.Atan2(_inputHandler.move.x, _inputHandler.move.y) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0, angle, 0);
    }

    private void Update()
    {
        HandleMovement();
        HandleRotation();
    }


}
