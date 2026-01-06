using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(InputHandler))]
public class PlayerShooting : MonoBehaviour
{
    private InputHandler _inputHandler;
    [SerializeField] private Weapon _weapon;

    private void Start()
    {
        _inputHandler = GetComponent<InputHandler>();
    }

    private void Update()
    {
        HandleShooting();
        Debug.Log(Mouse.current.position.ReadValue());
    }

    private void HandleShooting()
    {
        if (_inputHandler.shoot)
        {
            _weapon.Shoot();
        }
    }
}