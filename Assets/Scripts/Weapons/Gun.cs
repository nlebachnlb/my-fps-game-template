using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public enum ShootMode
{
    Manual,
    Automatic,
    Burst
}

public class Gun : MonoBehaviour
{
    public ShootMode shootMode;
    public float projectileSpeed;
    
    [Tooltip("How many time player fires per second")]
    public float fireRate;

    public float manualCooldownTime = 0.1f;
    
    public int ammoMagazine = 10;
    public int projectilesPerShot = 1;
    public float reloadDuration = 1f;
    public Transform aimPoint;
    public LayerMask checkMask;
    
    public int Ammo => numOfAmmo;
    public UnityAction<int> OnAmmoChange;
    public bool IsFullAmmo => !(Ammo < ammoMagazine);
    public bool IsReloading => isReloading;
    
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private Light muzzleLight;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float muzzleFlashDuration = 0.3f;
    [SerializeField] private List<AudioClip> shotSounds;

    private WeaponRecoil recoil;
    private Animator animator;
    private AudioSource audioSource;

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
        audioSource = GetComponent<AudioSource>();
        
        numOfAmmo = ammoMagazine;
        secondPerShot = shootMode == ShootMode.Automatic ? 1f / fireRate : manualCooldownTime;
        maxDistance = projectilePrefab.GetComponent<Projectile>().maxTraveledDistance;
    }

    public void ResetAttack()
    {
        isCoolingDown = true;
    }

    private void Update()
    {
        if (muzzleLight.intensity > 0f)
            muzzleLight.intensity -= Time.deltaTime * (1f / muzzleFlashDuration);
    }

    public void UpdateAttack(float dt)
    {
        if (isCoolingDown || isReloading) return;
        
        fireTimer += dt;
        if (fireTimer >= secondPerShot)
        {
            fireTimer = 0f;
            FireProjectile();
            if (shootMode == ShootMode.Manual)
                isCoolingDown = true;
        }
    }

    public void UpdateCoolDown(float dt)
    {
        if (shootMode == ShootMode.Manual)
        {
            if (recoil.RecoilTime > 0f)
            {
                recoil.RecoilTime -= dt;
                if (recoil.RecoilTime < 0f)
                    recoil.RecoilTime = 0f;
            }
        }

        if (!isCoolingDown) return;
        
        fireTimer += dt;
        if (fireTimer > secondPerShot)
        {
            fireTimer = secondPerShot;
            isCoolingDown = false;
            
            if (shootMode == ShootMode.Automatic)
                recoil.RecoilTime = 0f;
        }
    }

    private void FireProjectile()
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
        audioSource.PlayOneShot(shotSounds[Random.Range(0, shotSounds.Count)]);
        muzzleLight.intensity = 1f;
        recoil.RecoilTime += secondPerShot;
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
