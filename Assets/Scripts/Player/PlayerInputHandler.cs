using System;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    public PlayerInput.OnFootActions OnFoot { get; private set; }
    public float inputAcceleration = 5f;
    public float inputDeceleration = 5f;
    
    private PlayerInput playerInput;
    private PlayerMovement movement;
    private PlayerLook look;
    private PlayerWeapon attack;
    
    private Vector2 moveInput;

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
        moveInput = Vector2.zero;
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
        Vector2 inputAxis = OnFoot.Movement.ReadValue<Vector2>();
        if (inputAxis.x == 0)
            moveInput.x = Mathf.MoveTowards(moveInput.x, 0f, Time.deltaTime * inputDeceleration);
        else
            moveInput.x = Mathf.MoveTowards(moveInput.x, inputAxis.x, Time.deltaTime * inputAcceleration);
        
        if (inputAxis.y == 0)
            moveInput.y = Mathf.MoveTowards(moveInput.y, 0f, Time.deltaTime * inputDeceleration);
        else
            moveInput.y = Mathf.MoveTowards(moveInput.y, inputAxis.y, Time.deltaTime * inputAcceleration);

        moveInput.x = Mathf.Clamp(moveInput.x, -1f, 1f);
        moveInput.y = Mathf.Clamp(moveInput.y, -1f, 1f);
        
        // Call to PlayerMovement component and pass the input value
        movement.ProcessMovement(
            moveInput, 
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
