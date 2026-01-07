using UnityEngine;

[RequireComponent(typeof(InputHandler))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(StatsComponent))]
public class PlayerShooting : MonoBehaviour
{
    private InputHandler _inputHandler;
    private PlayerMovement _playerMovement;
    [SerializeField] private StatsComponent _statsComponent;
    [SerializeField] private Weapon _weapon;

    private void Start()
    {
        _inputHandler = GetComponent<InputHandler>();
        _playerMovement = GetComponent<PlayerMovement>();
        _statsComponent = GetComponent<StatsComponent>();

        if (_weapon != null && _statsComponent != null)
            _weapon.SetOwnerStats(_statsComponent.Stats);
    }

    private void Update()
    {
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (_inputHandler.shoot && !_playerMovement.IsRolling)
        {
            _weapon.Shoot();
        }
    }
}