using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputHandler _inputHandler;
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private float moveSpeed = 5f;

    private void HandleMovement()
    {
        Vector2 moveInput = _inputHandler.move;
        Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.y);
        _characterController.Move(moveDirection * Time.deltaTime * moveSpeed);
    }

    private void Update()
    {
        HandleMovement();
        Debug.Log("Move Input: " + _inputHandler.move);
    }
}
