using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBob : MonoBehaviour
{
    public float bobAmount;
    public float bobFrequency;
    public float bobSharpness;
    
    [SerializeField] private Transform weaponSocket;

    private PlayerMovement movement;
    private Vector3 weaponBobLocalPosition;
    private Vector3 weaponSocketLocalPosition;
    private float weaponBobFactor;

    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        weaponSocketLocalPosition = weaponSocket.localPosition;
    }

    private void Update()
    {
        UpdateWeaponBob(Time.deltaTime);
        weaponSocket.localPosition = weaponSocketLocalPosition + weaponBobLocalPosition;
    }

    private void UpdateWeaponBob(float dt)
    {
        if (dt <= 0) return;
        float movementFactor = 0f;
        if (movement.IsGrounded)
        {
            movementFactor = Mathf.Clamp01(movement.GroundedVelocity.magnitude / movement.runSpeed * movement.sprintMultiplier);
        }

        weaponBobFactor = Mathf.Lerp(weaponBobFactor, movementFactor, bobSharpness * dt);
        float hBobValue = Mathf.Sin(Time.time * bobFrequency) * bobAmount * weaponBobFactor;
        float vBobValue = ((Mathf.Sin(Time.time * bobFrequency * 2f) * 0.5f) + 0.5f) * bobAmount * weaponBobFactor;
        weaponBobLocalPosition.x = hBobValue;
        weaponBobLocalPosition.y = Mathf.Abs(vBobValue);
    }
}
