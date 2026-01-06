using UnityEngine;

[RequireComponent(typeof(InputHandler))]
public class PlayerShooting : MonoBehaviour
{
    private InputHandler _inputHandler;
    [SerializeField] private StatsComponent _statsComponent;
    [SerializeField] private Weapon _weapon;

    private void Start()
    {
        _inputHandler = GetComponent<InputHandler>();
        if (_statsComponent == null)
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
        if (_inputHandler.shoot)
        {
            _weapon.Shoot();
        }
    }
}