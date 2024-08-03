using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWeapon : MonoBehaviour
{
    public List<GameObject> weaponPrefabs = new();
    public Transform aimPoint;

    [SerializeField] private Transform playerWeaponSocket;
    [SerializeField] private TextMeshProUGUI textAmmo;
    [SerializeField] private TextMeshProUGUI textMagazine;
    
    private List<WeaponController> weapons = new();
    private WeaponController currentWeapon;

    private Animator animator;
    private bool isEquipping = false;
    private int currentIndex = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        foreach (var prefab in weaponPrefabs)
        {
            var obj = Instantiate(prefab, playerWeaponSocket);
            var weapon = obj.GetComponent<WeaponController>();
            weapons.Add(weapon);
            obj.SetActive(false);
            weapon.textAmmo = textAmmo;
            weapon.aimPoint = aimPoint;
        }
        
        SwitchWeapon(0);
    }

    public void Reload()
    {
        currentWeapon.Reload();
    }

    public void SwitchWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count || isEquipping) return;
        if (weapons[index] == currentWeapon) return;
        if (currentWeapon && currentWeapon.IsReloading()) return;
        
        isEquipping = true;
        currentIndex = index;
        if (currentWeapon == null)
        {
            OnUnequipped();
            return;
        }

        currentWeapon.Deactivate();
        animator.SetTrigger("Unequip");
    }

    public void ProcessAttack(bool isAttacking, float dt)
    {
        if (!currentWeapon) return;
        if (!isAttacking)
        {
            currentWeapon.ResetAttack();
            currentWeapon.UpdateCoolDown(dt);
            return;
        }
        
        currentWeapon.UpdateAttack(dt);
    }

    public void OnUnequipped()
    {
        if (currentWeapon)
            currentWeapon.gameObject.SetActive(false);
        
        animator.SetTrigger("Equip");
        currentWeapon = weapons[currentIndex];
        currentWeapon.gameObject.SetActive(true);
    }

    public void OnEquipped()
    {
        currentWeapon.Activate();
        isEquipping = false;
        
        textAmmo.text = "" + currentWeapon.GetWeaponAmmo();
        textMagazine.text = "" + currentWeapon.GetWeaponMagazine();
    }
}
