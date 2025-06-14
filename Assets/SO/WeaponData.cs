using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "newWeaponData")]
public class WeaponData : ScriptableObject
{

    public string weaponName = "Default";
    public float range = 10.0f;
    public float fireRate = 0.5f;
    public float damage = 10f;
    public int maxAmmo = 5;

    public float reloadTime;
    public float scopeTime;
    public float scopeFov;
    public Vector3 RecoilPos;
    public Vector3 RecoilAngle;
    public Vector3 gunLocalPosition;
    public Vector3 gunLocalRotation;

    public LayerMask hitLayers;

    public Sprite imageUI;
    public Sprite corsshair;
    public Sprite scopeCrosshair;

    public GameObject cameraScope = null;



}
