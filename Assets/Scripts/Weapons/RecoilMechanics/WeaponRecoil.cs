using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponRecoil : MonoBehaviour
{
    public float RecoilTime { get; set; }

    [Tooltip("The maximum/amplitude angle of the vertical recoil angle following the curve")]
    [Range(0, 89)]
    [SerializeField] private float maxRecoilXAngle;
    
    [Tooltip("Increment rate of vertical recoil angle to maximum value follows this curve")]
    [SerializeField] private AnimationCurve recoilCurveX;
    
    [Tooltip("The horizontal recoil angle will randomly around the base value with an amplitude defined")]
    [Range(0, 60)]
    [SerializeField] private float recoilYAmplitude;
    
    [Tooltip("Increment rate of horizontal recoil angle to maximum value follows this curve")]
    [SerializeField] private AnimationCurve recoilCurveY;
        
    [SerializeField] private float snappiness;
    [SerializeField] private float returnSpeed;
    

    private Vector3 currentRotation;
    private Vector3 targetRotation;
    private Camera camera;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
        camera = Camera.main;
        RecoilTime = 0f;
    }

    private void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, snappiness * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, returnSpeed * Time.deltaTime);
        camera.transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire()
    {
        animator.SetTrigger("RecoilFire");
        
        float recoilValueX = -1f * recoilCurveX.Evaluate(RecoilTime) * maxRecoilXAngle;
        float recoilValueY = recoilCurveX.Evaluate(RecoilTime) * recoilYAmplitude;
        targetRotation += new Vector3(recoilValueX, Random.Range(-recoilValueY, recoilValueY), 0);
        targetRotation.x = Mathf.Clamp(targetRotation.x, -1f * maxRecoilXAngle, 0);
    }
}
