using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float rotationSpeed;
    
    [Range(0.1f, 6f)]
    public float sensitivity = 1f;

    [SerializeField] private Transform playerCamera;
    [SerializeField] private Transform weaponSwaySocket;
    [SerializeField] private float swayStep = 4f;
    [SerializeField] private float maxSwayRotation = 5f;
    [SerializeField] private float swaySpeed = 10f;

    private float cameraVerticalAngle = 0f;
    private Vector3 swayRotation = Vector3.zero;
    
    public void ProcessLook(Vector2 input)
    {
        // horizontal character rotation
        {
            // rotate the transform with the input speed around its local Y axis
            transform.Rotate(
                new Vector3(0f, (input.x * rotationSpeed * sensitivity),
                    0f), Space.Self);
        }

        // vertical camera rotation
        {
            // add vertical inputs to the camera's vertical angle
            cameraVerticalAngle -= input.y * rotationSpeed * sensitivity;

            // limit the camera's vertical angle to min/max
            cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -89f, 89f);

            // apply the vertical angle as a local rotation to the camera transform along its right axis (makes it pivot up and down)
            playerCamera.localEulerAngles = new Vector3(cameraVerticalAngle, 0, 0);
        }
        
        // Weapon sway
        Vector2 invertLook = input * (-1f * swayStep);
        invertLook.x = Mathf.Clamp(invertLook.x, -maxSwayRotation, maxSwayRotation);
        invertLook.y = Mathf.Clamp(invertLook.y, -maxSwayRotation, maxSwayRotation);
        swayRotation = new Vector3(-invertLook.y, invertLook.x, 0f);

        weaponSwaySocket.localRotation = Quaternion.Slerp(weaponSwaySocket.localRotation,
            Quaternion.Euler(swayRotation), Time.deltaTime * swaySpeed);
    }
}
