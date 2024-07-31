using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float rotationSpeed;
    
    [Range(0.1f, 6f)]
    public float sensitivity = 1f;

    [SerializeField] private Transform playerCamera;

    private float cameraVerticalAngle = 0f;
    
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
    }
}
