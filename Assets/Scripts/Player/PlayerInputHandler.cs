using System;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerInput.OnFootActions OnFoot { get; private set; }
    
    private PlayerInput playerInput;
    private PlayerMovement movement;
    private PlayerLook look;
    private PlayerWeapon attack;

    private void Awake()
    {
        playerInput = new PlayerInput();
        OnFoot = playerInput.OnFoot;

        movement = GetComponent<PlayerMovement>();
        look = GetComponent<PlayerLook>();
        attack = GetComponent<PlayerWeapon>();

        OnFoot.Jump.performed += ctx => movement.Jump();
        OnFoot.Reload.performed += ctx => attack.Reload();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        ProcessMovement();
        ProcessAttack();
    }

    private void LateUpdate()
    {
        ProcessLook();
    }

    private void ProcessMovement()
    {
        // Call to PlayerMovement component and pass the input value
        movement.ProcessMovement(
            OnFoot.Movement.ReadValue<Vector2>(), 
            OnFoot.Sprint.IsPressed(),
            Time.deltaTime);
        
        movement.ProcessHeight(OnFoot.Crouch.IsPressed(), Time.deltaTime);
    }

    private void ProcessLook()
    {
        // Call to PlayerLook component and pass the input value
        look.ProcessLook(OnFoot.Look.ReadValue<Vector2>());
    }

    private void ProcessAttack()
    {
        attack.ProcessAttack(OnFoot.Attack.IsPressed(), Time.deltaTime);
    }
    
    private void OnEnable()
    {
        OnFoot.Enable();
    }

    private void OnDisable()
    {
        OnFoot.Disable();
    }
}
