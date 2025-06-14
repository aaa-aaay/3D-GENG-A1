using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotGun : Weapon
{
    [SerializeField] private int pelletCount;
    [SerializeField] private float spreadAngle;
    protected override void Start()
    {
        base.Start();
        reloadTimeSegment = weaponData.reloadTime / 2;
        reloadRotation = Quaternion.Euler(80, 160, 0);

        scopePos = new(-0.2f, 0.2f, 0.5f);
        scopeRotation = Quaternion.Euler(5, 180, 0);

    }
    public override void Shoot()
    {
        if (Time.time >= nextFireTime && ammoCount > 0)
        {
            ShootEffects("ShotGunShootSFX");
            StartCoroutine(StartRecoilCoroutine()); 

            nextFireTime = Time.time + weaponData.fireRate;

            for (int i = 0; i < pelletCount; i++) //shoot multiple raycasts
            {
                Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));

                Vector3 spreadDirection = ray.direction +
                    new Vector3(
                        Random.Range(-spreadAngle, spreadAngle),
                        Random.Range(-spreadAngle, spreadAngle),
                        0f
                    ).normalized * Mathf.Tan(Mathf.Deg2Rad * spreadAngle);

                Ray spreadRay = new Ray(ray.origin, spreadDirection);

                // Perform the raycast for this pellet
                if (Physics.Raycast(spreadRay, out RaycastHit hit, weaponData.range, weaponData.hitLayers))
                {
                    if (hit.collider.gameObject.TryGetComponent(out Damageable damageable))
                    {
                        damageable.TakeDamage(weaponData.damage);
                    }
                    if (hit.collider.gameObject.TryGetComponent(out SpawnEffects spawnEffects))
                    {
                        spawnEffects.SpawnHitEffect(hit);
                    }
                    Debug.DrawRay(ray.origin, spreadRay.direction * weaponData.range, Color.red, 1.0f); 
                }
            }

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
        else if (!isScoping)
        {


            float elapsedTime = 0;

            while (elapsedTime < scopeAnimationTime)
            {
                // Interpolate the position and rotation using Lerp
                weaponOffset = Vector3.Lerp(startPos, endPos, elapsedTime / scopeAnimationTime);
                scopeAngleOffset = Quaternion.Lerp(startRotation, endRotation, elapsedTime / scopeAnimationTime);

                if (elapsedTime > scopeAnimationTime / 2)
                {
                    isScoping = true;
                    InvokeCrossHairChanged();
                }

                elapsedTime += Time.deltaTime;

                yield return null;
            }

            weaponOffset = endPos;
            scopeAngleOffset = endRotation;
        }

    }

    protected override IEnumerator StartReloadCoroutine()
    {
        isReloading = true;

        Quaternion startRotation = Quaternion.identity;
        Quaternion targetRotation = Quaternion.Inverse(originalWeaponRotation) * reloadRotation;

        float elapsedTime = 0f;
        while (elapsedTime < reloadTimeSegment)
        {
            reloadAngleOffset = Quaternion.Lerp(startRotation, targetRotation, elapsedTime / reloadTimeSegment);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        AudioManager.instance.PlaySFX("WeaponReloadSFX", effectStartPos);
        reloadAngleOffset = targetRotation;

        elapsedTime = 0f;
        while (elapsedTime < reloadTimeSegment)
        {
            reloadAngleOffset = Quaternion.Lerp(targetRotation, startRotation, elapsedTime / reloadTimeSegment);
            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        reloadAngleOffset = startRotation;
        isReloading = false;
    }
}
