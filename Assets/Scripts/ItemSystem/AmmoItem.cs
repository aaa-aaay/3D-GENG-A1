using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoItem : MonoBehaviour, Item
{
    [SerializeField] private int ammoAmount;
    [SerializeField] private GameObject itemUI;

    public void Start()
    {
        itemUI.SetActive(false);
    }

    public void Use(FpsController fpsController)
    {

        fpsController.reloadCount++;
        fpsController.CurrentWeapon.InvokeReloadCount(fpsController.reloadCount);
        Destroy(gameObject);
        HideUI();

        AudioManager.instance.PlaySFX("GunSFX", transform.position);
    }
    public void ShowUI()
    {
        if (itemUI != null)
            itemUI.SetActive(true);
    }

    public void HideUI()
    {
        if(itemUI != null)
        itemUI.SetActive(false);
    }




}
