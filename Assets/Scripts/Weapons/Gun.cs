using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum ShootMode
{
    OneShot,
    Automatic,
    Burst
}

public class Gun : MonoBehaviour
{
    public ShootMode shootMode;
    public float projectileSpeed;
    
    [Tooltip("How many time player fires per second")]
    public float fireRate;
    
    public int ammoMagazine = 10;
    public int projectilesPerShot = 1;
    public float reloadDuration = 1f;
    public Transform aimPoint;
    public LayerMask checkMask;
    
    public int Ammo => numOfAmmo;
    public UnityAction<int> OnAmmoChange;
    public bool IsFullAmmo => !(Ammo < ammoMagazine);
    
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private Transform muzzle;

    private WeaponRecoil recoil;
    private Animator animator;

    private int numOfAmmo;
    private float fireTimer = 0f;
    private float secondPerShot;
    private bool isCoolingDown = false;
    private bool isReloading = false;
    private float maxDistance;

    private void Start()
    {
        recoil = GetComponent<WeaponRecoil>();
        animator = GetComponent<Animator>();
        
        numOfAmmo = ammoMagazine;
        secondPerShot = 1f / fireRate;
        maxDistance = projectilePrefab.GetComponent<Projectile>().maxTraveledDistance;
    }

    public void ResetAttack()
    {
        isCoolingDown = true;
        recoil.RecoilTime = 0f;
    }

    public void UpdateAttack(float dt)
    {
        if (isCoolingDown || isReloading) return;
        
        fireTimer += dt;
        recoil.RecoilTime += dt;
        if (fireTimer >= secondPerShot)
        {
            fireTimer = 0f;
            FireProjectile();
        }
    }

    public void UpdateCoolDown(float dt)
    {
        if (!isCoolingDown) return;
        
        fireTimer += dt;
        if (fireTimer > secondPerShot)
        {
            fireTimer = secondPerShot;
            isCoolingDown = false;
        }
    }
    
    public void FireProjectile()
    {
        if (numOfAmmo <= 0) return;
        numOfAmmo--;

        for (int i = 0; i < projectilesPerShot; ++i)
        {
            Quaternion rotation = Quaternion.LookRotation(muzzle.forward * 1000f - aimPoint.position);
        
            RaycastHit hit;
            if (Physics.Raycast(aimPoint.position, aimPoint.forward, out hit, maxDistance, checkMask))
                rotation = Quaternion.LookRotation(hit.point - muzzle.position);
            
            var projectile = Instantiate(projectilePrefab, muzzle.position, rotation);
        }

        muzzleEffect.Play();
        DispatchOnAmmoChange();
        recoil.RecoilFire();
    }

    public void Reload()
    {
        if (IsFullAmmo) return;
        if (isReloading) return;
        StartCoroutine(ReloadProgress());
    }

    private IEnumerator ReloadProgress()
    {
        // TODO: Play reload animation
        animator.SetTrigger("Reload");
        isReloading = true;
        yield return new WaitForSeconds(reloadDuration);
        numOfAmmo = ammoMagazine;
        isReloading = false;
        DispatchOnAmmoChange();
    }

    private void DispatchOnAmmoChange()
    {
        OnAmmoChange(numOfAmmo);
    }
}
