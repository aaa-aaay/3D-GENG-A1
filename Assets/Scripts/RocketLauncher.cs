using System.Collections;
using UnityEngine;

public class RocketLauncher : Weapon
{
    [SerializeField] private float shakePower;
    [SerializeField] private float shakeTimer;
    [SerializeField] private GameObject weaponAmmo;
    public static System.Action<float, float, Ray> Onfire;

    protected override void Start()
    {
        base.Start();
        reloadTimeSegment = weaponData.reloadTime / 3;
        reloadRotation = Quaternion.Euler(0.7f, 33, 0);
    }
    public override void Shoot()
    {
        if (Time.time >= nextFireTime && ammoCount > 0)
        {
            ShootEffects("RocketShootSFX");
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            RocketShoot(ray);
            nextFireTime = Time.time + weaponData.fireRate;
            //Debug.Log(weaponData.name + " fired");
            StartCoroutine(StartRecoilCoroutine());
            ammoCount--;
            InvokeAmmoCountChanged();
            StartCoroutine(StartCameraShake());
        }
    }

    private void RocketShoot(Ray ray)
    {
        Debug.Log(transform.position);
        GameObject ammo = Instantiate(weaponAmmo, effectStartPos, Quaternion.LookRotation(ray.direction));
        if(ammo.TryGetComponent(out RocketAmmo Rocket)) {
            Rocket.FireMissle(weaponData.damage, ray);
        }
        //Onfire?.Invoke(weaponData.damage, weaponData.range, ray);
    }

    private IEnumerator StartCameraShake()
    {
        float orginalTimer = shakeTimer;
        while (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            cameraShakeOffset = Random.insideUnitSphere * shakePower;
            yield return null;
        }

        cameraShakeOffset = Vector3.zero;
        shakeTimer = orginalTimer;
        yield return null;
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

        reloadAngleOffset = targetRotation;



        //add balls to the barral
        GameObject Ammo = Instantiate(reloadMags);
        Ammo.transform.SetParent(gameObject.transform);
        Vector3 AmmoSpawn = new(-2f, 0.0f, 0f);
        Ammo.transform.localRotation = Quaternion.identity;

        AudioManager.instance.PlaySFX("RocketReloadSFX", effectStartPos);

        elapsedTime = 0f;
        while (elapsedTime < reloadTimeSegment / 2)
        {
            Ammo.transform.localPosition = Vector3.Lerp(AmmoSpawn, Vector3.zero,
                elapsedTime / (reloadTimeSegment / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        AudioManager.instance.PlaySFX("RocketReloadSFX", effectStartPos);

        elapsedTime = 0f;
        while (elapsedTime < reloadTimeSegment / 2)
        {
            Ammo.transform.localPosition = Vector3.Lerp(AmmoSpawn, Vector3.zero,
                elapsedTime / (reloadTimeSegment / 2));
            elapsedTime += Time.deltaTime;
            yield return null;
        }


        Destroy(Ammo);

        // Reset weapon position and rotation
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

    protected override IEnumerator StartScopeCoroutine()
    {
        yield return null;
    }
}
