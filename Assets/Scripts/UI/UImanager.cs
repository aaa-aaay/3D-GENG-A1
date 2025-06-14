using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoCountText; 
    [SerializeField] private TMP_Text reloadCountText;
    [SerializeField] private Image WeaponSprite;
    [SerializeField] private Image weaponCrosshair;
    void OnEnable()
    {
        Weapon.OnAmmoCountChanged += UpdateAmmoText;
        Weapon.OnReloadCountChanged += UpdateReloadText;
        Weapon.OnGunSpriteChanged += UpdateGunImage;
        Weapon.OnCrosshairChanged += UpdateCrossHair;
    }
    void OnDisable()
    {
        Weapon.OnAmmoCountChanged -= UpdateAmmoText;
        Weapon.OnReloadCountChanged -= UpdateReloadText;
        Weapon.OnGunSpriteChanged -= UpdateGunImage;
        Weapon.OnCrosshairChanged -= UpdateCrossHair;

    }
    private void UpdateAmmoText(int currentAmmoCount, int maxAmmoCount)
    {
        ammoCountText.text = currentAmmoCount + " / " + maxAmmoCount;
    }
    private void UpdateReloadText(int reloadCount)
    {
        reloadCountText.text = reloadCount.ToString();
    }

    private void UpdateGunImage(Sprite gunUI)
    {
        WeaponSprite.sprite = gunUI;
        WeaponSprite.SetNativeSize();
    }

    private void UpdateCrossHair(Sprite corssHair, float scaleMulti)
    {
        weaponCrosshair.sprite = corssHair;
        weaponCrosshair.SetNativeSize();
        weaponCrosshair.transform.localScale = new(scaleMulti, scaleMulti);
        
    }
}
