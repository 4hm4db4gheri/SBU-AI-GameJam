using UnityEngine;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    private InputHandler _inputHandler;
    private CharacterController _characterController;
    [SerializeField] private float moveSpeed = 5f;

    private void Start()
    {
        _inputHandler = GetComponent<InputHandler>();
        _characterController = GetComponent<CharacterController>();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = _inputHandler.move;
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        _characterController.Move(moveDirection * Time.deltaTime * moveSpeed);
    }

    private void Update()
    {
        HandleMovement();
    }
}
