using UnityEngine;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimations : MonoBehaviour
{
    private Animator _animator;
    private InputHandler _inputHandler;


    private void Start()
    {
        _animator = GetComponent<Animator>();
        _inputHandler = GetComponent<InputHandler>();
    }

    private void Update()
    {
        if (_inputHandler.move.magnitude > 0)
        {
            _animator.SetBool("IsMoving", true);
        }
        else
        {
            _animator.SetBool("IsMoving", false);
        }

        if (_inputHandler.shoot)
        {
            _animator.SetBool("IsShooting", true);
        }
        else
        {
            _animator.SetBool("IsShooting", false);
        }
    }
}