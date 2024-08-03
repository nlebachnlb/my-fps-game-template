using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("General")] [Tooltip("Force applied downward when in the air")]
    public float gravity = 20f;
    
    [Header("Movement")]
    public float runSpeed;
    public float sprintMultiplier;
    public float jumpHeight;
    public float speedAcceleration = 10f;

    [Header("Crouch")] 
    [Range(0.1f, 1f)] public float crouchHeightRatio = 0.5f;
    [Range(0f, 1f)] public float crouchSpeedRatio = 0.75f;
    [Range(0f, 1f)] public float eyesHeightRatio = 0.75f;
    public float crouchSharpness = 10f;
    private float defaultHeight = 2f;


    [Header("Footstep")] 
    [SerializeField] private AudioClip sfxFootStepGround;
    [SerializeField] private AudioClip sfxLand;
    [SerializeField] private AudioClip sfxJump;
    
    public bool IsGrounded => controller.isGrounded;
    public Vector3 GroundedVelocity => new Vector3(velocity.x, 0, velocity.z);
    public float CurrentSpeed => currentSpeed;
    public float TargetSpeed => targetSpeed;

    [SerializeField] private Transform eyesCamera;
    
    private CharacterController controller;
    private float currentSpeed;
    private float targetSpeed;
    private Vector3 velocity;
    private float targetHeight;
    private PlayerBob bob;
    private AudioSource audioSource;
    private float footstepTimer = 0f;
    private Animator animator;
    private bool sprint = false;
    private PlayerWeapon playerWeapon;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        bob = GetComponent<PlayerBob>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        playerWeapon = GetComponent<PlayerWeapon>();
    }

    private void Start()
    {
        currentSpeed = runSpeed;
        velocity = Vector3.zero;
    }

    public void ProcessMovement(Vector2 inputAxis, bool isSprinting, float dt)
    {
        sprint = isSprinting && !playerWeapon.IsSprintTerminated();

        float targetRatio = targetHeight < defaultHeight ? crouchSpeedRatio : 1f;
        if (Mathf.Approximately(targetRatio, 1f) && sprint)
            targetRatio *= sprintMultiplier;
        
        targetSpeed = runSpeed * targetRatio;
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, dt * speedAcceleration);
        
        // Calculate movement
        Vector3 motion = new Vector3(inputAxis.x, 0, inputAxis.y);
        motion = transform.TransformDirection(motion) * currentSpeed;
        velocity.x = motion.x;
        velocity.z = motion.z;
        
        // Calculate gravity
        velocity.y -= gravity * dt;
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;
        
        controller.Move(velocity * dt);
        
        float movementFactor = Mathf.Clamp01(GroundedVelocity.magnitude / runSpeed * sprintMultiplier);
        float walkCycle = 3f / currentSpeed;
        footstepTimer += dt * movementFactor * (controller.isGrounded ? 1f : 0f);
        if (footstepTimer > walkCycle)
        {
            audioSource.PlayOneShot(sfxFootStepGround);
            footstepTimer = 0f;
        }

        animator.SetBool("IsSprinting", sprint);
    }

    public void Jump()
    {
        if (controller.isGrounded)
        {
            velocity = new Vector3(velocity.x, 0, velocity.z);
            velocity += Vector3.up * jumpHeight;
            audioSource.PlayOneShot(sfxJump);
            footstepTimer = 0f;
        }
    }
    
    public void ProcessHeight(bool isCrouching, float dt)
    {
        float targetRatio = 1f;
        if (isCrouching)
        {
            targetRatio = crouchHeightRatio;
        }
        else
        {
            Collider[] standingOverlaps = Physics.OverlapCapsule(
                GetCapsuleBottomHemisphere(),
                GetCapsuleTopHemisphere(defaultHeight),
                controller.radius,
                -1,
                QueryTriggerInteraction.Ignore);
            
            foreach (Collider c in standingOverlaps)
            {
                if (c != controller)
                {
                    targetRatio = crouchHeightRatio;
                    break;
                }
            }
        }

        targetHeight = targetRatio * defaultHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, dt * crouchSharpness);
        controller.center = Vector3.up * (controller.height * 0.5f);
        eyesCamera.localPosition = Vector3.up * (controller.height * eyesHeightRatio);
    }
    
    // Gets the center point of the bottom hemisphere of the character controller capsule    
    private Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * controller.radius);
    }
    
    private Vector3 GetCapsuleTopHemisphere(float height)
    {
        return transform.position + (transform.up * (height - controller.radius));
    }
}
