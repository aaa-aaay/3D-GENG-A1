using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RayCastWeapon : Weapon
{
    [SerializeField] GameObject weaponScope;
    [SerializeField] GameObject weaponModel;
    protected override void Start()
    {
        base.Start();

        reloadTimeSegment = weaponData.reloadTime / 3;
        reloadTransform = new(0, 0.3f, 0);
        reloadRotation = Quaternion.Euler(30, -188, 0);

        scopePos = new(-0.19f, 0.07f, -0.3f);
        scopeRotation = Quaternion.Euler(0, -180, 0);
    }
    public override void Shoot()
    {
        if (Time.time >= nextFireTime && ammoCount > 0 && !isReloading)
        {
            ShootEffects("SniperShotSFX");
            StartCoroutine(StartRecoilCoroutine());
            nextFireTime = Time.time + weaponData.fireRate;
            Debug.Log(weaponData.name + " fired");
            PerformRayCast();
            ammoCount--;
            InvokeAmmoCountChanged();
        }
    }
    protected override IEnumerator StartScopeCoroutine()
    {
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = scopePos - originalWeaponPos;
        Quaternion startRotation = Quaternion.identity;
        Quaternion endRotation = Quaternion.Inverse(originalWeaponRotation) * scopeRotation;

        if (isScoping)
        {

            gameObject.transform.localScale = originalWeaponScale;
            float elapsedTime = 0;

            while (elapsedTime < scopeAnimationTime)
            {
                // Interpolate the position and rotation using Lerp
                weaponOffset = Vector3.Lerp(endPos, startPos, elapsedTime / scopeAnimationTime);
                scopeAngleOffset = Quaternion.Lerp(endRotation, startRotation, elapsedTime / scopeAnimationTime);

                if (elapsedTime > scopeAnimationTime / 2)
                {
                    isScoping = false;
                    InvokeCrossHairChanged();
                }


                elapsedTime += Time.deltaTime;

                yield return null;
            }

            weaponOffset = startPos;
            scopeAngleOffset = startRotation;
        }
        else if(!isScoping)
        {
            float elapsedTime = 0;

            while (elapsedTime < scopeAnimationTime)
            {
                // Interpolate the position and rotation using Lerp
                weaponOffset = Vector3.Lerp(startPos, endPos, elapsedTime / scopeAnimationTime);
                scopeAngleOffset = Quaternion.Lerp(startRotation, endRotation, elapsedTime / scopeAnimationTime);

                if(elapsedTime > scopeAnimationTime / 2) {
                    isScoping = true;
                    InvokeCrossHairChanged();
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            weaponOffset = endPos;
            scopeAngleOffset = endRotation;

            gameObject.transform.localScale = Vector3.zero; // "Hide" the gun
            weaponScope.transform.position = playerCamera.transform.position;
        }
        
    }
    protected override IEnumerator StartReloadCoroutine()
    {
        isReloading = true;
        Vector3 startPosition = Vector3.zero;
        Quaternion startRotation = Quaternion.identity;

        Vector3 targetPosition = reloadTransform - originalWeaponPos;
        Quaternion targetRotation = Quaternion.Inverse(originalWeaponRotation) * reloadRotation;

        float elapsedTime = 0f;

        while (elapsedTime < reloadTimeSegment)
        {
            weaponOffset = Vector3.Lerp(startPosition, targetPosition, elapsedTime / reloadTimeSegment);
            reloadAngleOffset = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / reloadTimeSegment);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Reset Pos
        weaponOffset = targetPosition;
        reloadAngleOffset = targetRotation;

        elapsedTime = 0f;
        Vector3 magStartPosition = reloadMags.transform.localPosition;
        Vector3 magTargetPosition = new(magStartPosition.x, magStartPosition.y - 5, magStartPosition.z);

        // Move magazine down
        while (elapsedTime < reloadTimeSegment / 2)
        {
            reloadMags.transform.localPosition = Vector3.Lerp(magStartPosition, magTargetPosition, elapsedTime / (reloadTimeSegment / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        reloadMags.transform.localPosition = magTargetPosition;
        AudioManager.instance.PlaySFX("WeaponReloadSFX", transform.position);



        // Start moving the magazine back 
        elapsedTime = 0f;
        while (elapsedTime < reloadTimeSegment / 2)
        {
            reloadMags.transform.localPosition = Vector3.Lerp(magTargetPosition, magStartPosition, elapsedTime / (reloadTimeSegment / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        reloadMags.transform.localPosition = magStartPosition;



        elapsedTime = 0f;
        while (elapsedTime < reloadTimeSegment)
        {
            weaponOffset = Vector3.Lerp(targetPosition, startPosition, elapsedTime / reloadTimeSegment);
            reloadAngleOffset = Quaternion.Lerp(targetRotation, startRotation, elapsedTime / reloadTimeSegment);

            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        // Ensure the weapon is back
        weaponOffset = startPosition;
        reloadAngleOffset = startRotation;

        isReloading = false;
    }

}
