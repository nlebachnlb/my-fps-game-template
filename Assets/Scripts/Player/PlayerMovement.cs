using System;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    [Header("General")] 
    [Tooltip("Force applied downward when in the air")]
    public float gravity = 20f;
    
    [Header("Movement")]
    public float runSpeed;
    public float sprintMultiplier;
    public float jumpHeight;
    public float speedAcceleration = 10f;
    [SerializeField] private float groundCheckDistance = 1f;
    [SerializeField] private LayerMask groundCheckLayers;

    [Header("Crouch")] 
    [Range(0.1f, 1f)] public float crouchHeightRatio = 0.5f;
    [Range(0f, 1f)] public float crouchSpeedRatio = 0.75f;
    [Range(0f, 1f)] public float eyesHeightRatio = 0.75f;
    public float crouchSharpness = 10f;


    [Header("Footstep")] 
    [SerializeField] private AudioClip sfxFootStepGround;
    [SerializeField] private AudioClip sfxLand;
    [SerializeField] private AudioClip sfxJump;
    
    [Header("Camera")]
    [SerializeField] private Transform eyesCamera;

    public UnityEvent OnLandedEvent;
    
    public bool IsGrounded { get; private set; } = true;

    public Vector3 GroundedVelocity => new(_velocity.x, 0, _velocity.z);
    public float CurrentSpeed { get; private set; }
    public float TargetSpeed { get; private set; }
    
    private CharacterController _controller;
    private PlayerWeapon _playerWeapon;
    private AudioSource _audioSource;
    private Animator _animator;
    private Vector3 _velocity;
    private float _targetHeight;
    private float _footstepTimer = 0f;
    private bool _sprint = false;
    private float _lastJumpTime;
    private float _sprintBlend;

    // Prevent snapping player on the ground after pressing jump, just snap on the falling phase
    private const float k_jumpFallSnapPreventionTime = 0.2f;
    private const float k_groundCheckDistanceOnAir = 0.1f;
    private const float k_defaultHeight = 2f;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _audioSource = GetComponent<AudioSource>();
        _animator = GetComponent<Animator>();
        _playerWeapon = GetComponent<PlayerWeapon>();
    }

    private void Start()
    {
        CurrentSpeed = runSpeed;
        _velocity = Vector3.zero;
    }
    
    public void ProcessMovement(Vector2 inputAxis, bool isSprinting, float dt)
    {
        bool wasGrounded = IsGrounded;
        GroundCheck();
        
        _sprint = isSprinting && !_playerWeapon.IsSprintTerminated();

        float targetRatio = _targetHeight < k_defaultHeight ? crouchSpeedRatio : 1f;
        if (Mathf.Approximately(targetRatio, 1f) && _sprint)
            targetRatio *= sprintMultiplier;
        
        TargetSpeed = runSpeed * targetRatio;
        CurrentSpeed = Mathf.MoveTowards(CurrentSpeed, TargetSpeed, dt * speedAcceleration);
        
        // Calculate movement
        Vector3 motion = new Vector3(inputAxis.x, 0, inputAxis.y);
        motion = transform.TransformDirection(motion) * CurrentSpeed;
        _velocity.x = motion.x;
        _velocity.z = motion.z;
        
        // Calculate gravity
        _velocity.y -= gravity * dt;
        if (IsGrounded && _velocity.y < 0)
            _velocity.y = -2f;
        
        _controller.Move(_velocity * dt);
        
        float movementFactor = Mathf.Clamp01(GroundedVelocity.magnitude / runSpeed * sprintMultiplier);
        float walkCycle = 3f / CurrentSpeed;
        _footstepTimer += dt * movementFactor * (IsGrounded ? 1f : 0f);
        if (_footstepTimer > walkCycle)
        {
            _audioSource.PlayOneShot(sfxFootStepGround);
            _footstepTimer = 0f;
        }

        float sprintValue = Mathf.Max(CurrentSpeed - runSpeed, 0f) / (runSpeed * sprintMultiplier - runSpeed);
        if (!_sprint) sprintValue = 0f;
        
        _sprintBlend = Mathf.Lerp(_sprintBlend, sprintValue, dt * speedAcceleration);
        _animator.SetFloat("Sprint", _sprintBlend);

        if (!wasGrounded && IsGrounded)
        {
            OnLanded();
        }
    }

    public void Jump()
    {
        if (IsGrounded)
        {
            _velocity = new Vector3(_velocity.x, 0, _velocity.z);
            _velocity += Vector3.up * jumpHeight;
            _audioSource.PlayOneShot(sfxJump);
            _footstepTimer = 0f;
            IsGrounded = false;
            _lastJumpTime = Time.time;
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
                GetCapsuleTopHemisphere(k_defaultHeight),
                _controller.radius,
                -1,
                QueryTriggerInteraction.Ignore);
            
            foreach (Collider c in standingOverlaps)
            {
                if (c != _controller)
                {
                    targetRatio = crouchHeightRatio;
                    break;
                }
            }
        }

        _targetHeight = targetRatio * k_defaultHeight;
        _controller.height = Mathf.Lerp(_controller.height, _targetHeight, dt * crouchSharpness);
        _controller.center = Vector3.up * (_controller.height * 0.5f);
        eyesCamera.localPosition = Vector3.up * (_controller.height * eyesHeightRatio);
    }

    private void OnLanded()
    {
        _audioSource.PlayOneShot(sfxLand);
        _animator.SetTrigger("Land");
        OnLandedEvent?.Invoke();
    }

    private void GroundCheck()
    {
        float checkDistance = IsGrounded ? (_controller.skinWidth + groundCheckDistance) : k_groundCheckDistanceOnAir;
        IsGrounded = false;

        Debug.DrawRay(GetCapsuleBottomHemisphere(), Vector3.down);

        if (Time.time <= _lastJumpTime + k_jumpFallSnapPreventionTime) return;
        
        if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(_controller.height),
                _controller.radius, Vector3.down, out RaycastHit hit, checkDistance, groundCheckLayers,
                QueryTriggerInteraction.Ignore))
        {
            // Debug.Log("Up and normal:" + Vector3.Angle(transform.up, hit.normal));
            // Debug.Log("Dot:" + Vector3.Dot(hit.normal, transform.up));
            if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                Vector3.Angle(transform.up, hit.normal) <= _controller.slopeLimit)
            {
                IsGrounded = true;

                if (hit.distance > _controller.skinWidth)
                {
                    _controller.Move(Vector3.down * hit.distance);
                }
            }
        }
    }
    
    // Gets the center point of the bottom hemisphere of the character controller capsule    
    private Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * _controller.radius);
    }
    
    private Vector3 GetCapsuleTopHemisphere(float height)
    {
        return transform.position + (transform.up * (height - _controller.radius));
    }
}
