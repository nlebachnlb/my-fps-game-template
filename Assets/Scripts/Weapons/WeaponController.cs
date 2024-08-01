using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public enum WeaponType
{
    Melee,
    Gun
}

public class WeaponController : MonoBehaviour
{
    public TextMeshProUGUI textAmmo;
    public Transform aimPoint;
    
    public string weaponName;
    public WeaponType weaponType;

    private Gun gun;

    private void Awake()
    {
        gun = GetComponent<Gun>();
        gun.OnAmmoChange = ammo => textAmmo.text = "" + ammo;
    }

    private void Start()
    {
        gun.aimPoint = aimPoint;
    }

    public void Activate()
    {
        // TODO: Play equip animation
    }
    
    public void Deactivate()
    {
        
    }

    public void ResetAttack()
    {
        gun.ResetAttack();
    }

    public void UpdateAttack(float dt)
    {
        gun.UpdateAttack(dt);
    }
    
    public void UpdateCoolDown(float dt)
    {
        gun.UpdateCoolDown(dt);
    }

    public void Reload()
    {
        gun.Reload();
    }

    public bool IsReloading()
    {
        return gun.IsReloading;
    }

    public int GetWeaponMagazine()
    {
        return gun.ammoMagazine;
    }

    public int GetWeaponAmmo()
    {
        return gun.Ammo;
    }
}
