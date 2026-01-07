using UnityEngine;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(Animator))]
public class PlayerAnimations : MonoBehaviour
{
    private Animator _animator;
    private InputHandler _inputHandler;
    private bool _rollAnimationPlaying = false;
    public bool RollAnimationPlaying { get => _rollAnimationPlaying; set => _rollAnimationPlaying = value; }

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
        if (_inputHandler.roll && !_rollAnimationPlaying)
        {
            _animator.SetTrigger("Roll");
            _rollAnimationPlaying = true;
        }
    }
}