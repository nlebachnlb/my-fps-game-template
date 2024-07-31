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
        if (index < 0 || index >= weapons.Count) return;
        if (currentWeapon != null)
            currentWeapon.Deactivate();
        currentWeapon = weapons[index];
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.Activate();
        
        textAmmo.text = "" + currentWeapon.GetWeaponAmmo();
        textMagazine.text = "" + currentWeapon.GetWeaponMagazine();
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
}
