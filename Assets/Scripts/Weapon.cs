using System.Collections;
using UnityEngine;

public abstract class Weapon : MonoBehaviour, Item
{

    protected Camera playerCamera;
    [SerializeField] public WeaponData weaponData;
    [SerializeField] public GameObject pickUpText;
    [SerializeField] protected GameObject reloadMags;
    [SerializeField] private Vector2 wobbleMulti = new(-0.1f, 0.2f);
    [HideInInspector] public int ammoCount = 0;
    [HideInInspector] public Vector3 originalWeaponPos = Vector3.zero;
    [HideInInspector] public Quaternion originalWeaponRotation = Quaternion.identity;
    [HideInInspector] public Vector3 originalWeaponScale = Vector3.zero;


    protected Vector3 weaponOffset = Vector3.zero;
    protected Vector3 reloadTransform;
    protected Quaternion reloadRotation;
    protected float nextFireTime = 0f;
    protected bool isReloading = false;
    protected float reloadTimeSegment;

    protected Quaternion reloadAngleOffset = Quaternion.identity;
    protected Quaternion scopeAngleOffset = Quaternion.identity;
    protected Quaternion recoilAngleOffset = Quaternion.identity;   

    //SCOPING IN 
    protected bool isScoping = false;
    protected Vector3 scopePos;
    protected Quaternion scopeRotation;
    protected float scopeAnimationTime;
    protected float scopeFov;

    private float crosshairScale = 0.5f;
    private float scopeCrosshairScale = 1.4f;

    protected Vector3 recoilOffset;
    protected Vector3 cameraShakeOffset;

    //Shooting Effect
    [SerializeField] protected GameObject muzzleFlash;
    [SerializeField] protected GameObject shootLocation;
    protected Vector3 effectStartPos;

    public static System.Action<int, int> OnAmmoCountChanged;
    public static System.Action<int> OnReloadCountChanged;
    public static System.Action<Sprite> OnGunSpriteChanged;
    public static System.Action<Sprite, float> OnCrosshairChanged;


    protected virtual void Start()
    {
        ammoCount = weaponData.maxAmmo;
        InvokeAmmoCountChanged();
        pickUpText.SetActive(false);

        originalWeaponPos = gameObject.transform.localPosition;
        originalWeaponRotation = gameObject.transform.localRotation;
        originalWeaponScale  = gameObject.transform.localScale;

        scopeAnimationTime = weaponData.scopeTime;
         isScoping = false;
        playerCamera = Camera.main;

        effectStartPos = shootLocation.transform.localPosition;
    }   


    /*<--------------Methods for Weapons------------------->*/
    protected void PerformRayCast()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if(Physics.Raycast(ray, out RaycastHit hit , weaponData.range, weaponData.hitLayers))
        {
            Debug.Log("Hit Object: " + hit.collider.gameObject.name);

            GameObject hitObject  = hit.collider.gameObject;
            if (hitObject.TryGetComponent(out Damageable damageable))
            {
                damageable.TakeDamage(weaponData.damage);
            }
            if(hitObject.TryGetComponent(out SpawnEffects spawnEffects))
            {
                spawnEffects.SpawnHitEffect(hit);
            }
        }
    }

    public void Animations(float bobbingOffset)
    {
        gameObject.transform.localPosition = originalWeaponPos + weaponOffset + recoilOffset + new Vector3(bobbingOffset * wobbleMulti.x, bobbingOffset * wobbleMulti.y, 0);
        gameObject.transform.localRotation = originalWeaponRotation * scopeAngleOffset * reloadAngleOffset * recoilAngleOffset;
        effectStartPos = transform.position;
    }

public virtual float Scope()
    {
        if (!isReloading)
        {
            StartCoroutine(StartScopeCoroutine());
            return weaponData.scopeFov;
        }
        else return 1;
    }

    public virtual int Reload(int reloadCount)
    {

        if (isReloading)
        {
            Debug.Log("Already reloading.");
            return reloadCount;
        }

        if (reloadCount == 0)
        {
            Debug.Log("No reloads remaining!");
            return reloadCount;
        }

        if (isScoping)
        {
            StartCoroutine(StartScopeCoroutine());
        }
        else
        {
            StartCoroutine(StartReloadCoroutine());
            reloadCount--;
            ammoCount = weaponData.maxAmmo;
            InvokeAmmoCountChanged();
            InvokeReloadCount(reloadCount);
        }
        
        return reloadCount;
    }

    public abstract void Shoot();

    public void Drop(FpsController player) //droping weapon
    {
        transform.SetParent(null);
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(Camera.main.transform.position + player.gameObject.transform.forward * 10, ForceMode.Impulse);
        }
        else
        {
            transform.SetPositionAndRotation(Camera.main.transform.position + Camera.main.transform.forward * 3f, Quaternion.identity);

        }
        BoxCollider BC = gameObject.GetComponent<BoxCollider>();
        if(BC != null)
        {
            BC.enabled = true;
        }
        gameObject.layer = 8;
        player.weapons.Remove(this);
    }

    protected void ShootEffects(string sfxName)
    {
        AudioManager.instance.PlaySFX(sfxName, effectStartPos);
        GameObject effect = Instantiate(muzzleFlash, shootLocation.transform.position, Quaternion.identity);
        effect.transform.SetParent(shootLocation.transform, true);
    }

    protected abstract IEnumerator StartReloadCoroutine();
    protected abstract IEnumerator StartScopeCoroutine();
    protected virtual IEnumerator StartRecoilCoroutine()
    {
        Quaternion recoilRotation = Quaternion.Euler(weaponData.RecoilAngle);

        float elapsedTime = 0;

        // Recoil movement up
        while (elapsedTime < weaponData.fireRate / 2)
        {
            recoilOffset = Vector3.Lerp(Vector3.zero, weaponData.RecoilPos, elapsedTime / (weaponData.fireRate / 2));
            recoilAngleOffset = Quaternion.Lerp(Quaternion.identity, recoilRotation, elapsedTime / (weaponData.fireRate / 2));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        //set Final position
        recoilOffset = weaponData.RecoilPos;
        recoilAngleOffset = recoilRotation;

        elapsedTime = 0f;

        // Recoil movement back
        while (elapsedTime < weaponData.fireRate / 2)
        {
            recoilOffset = Vector3.Lerp(weaponData.RecoilPos, Vector3.zero, elapsedTime / (weaponData.fireRate / 2));
            recoilAngleOffset = Quaternion.Lerp(recoilRotation, Quaternion.identity, elapsedTime / (weaponData.fireRate / 2));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Set Final Position
        recoilOffset = Vector3.zero;
        recoilAngleOffset = Quaternion.identity;
    }

    public bool IsScoping() { return isScoping; }
    public Vector3 GetCameraShake() { return cameraShakeOffset; }


    /*<--------------Methods from Item class------------------->*/
    public void Use(FpsController player) //When Pick Up Weapon
    {

        transform.SetParent(player.weaponHolderGO.transform);
        transform.SetLocalPositionAndRotation(weaponData.gunLocalPosition, Quaternion.Euler(weaponData.gunLocalRotation));
        gameObject.layer = 6; //6 is weapon Layer
        BoxCollider BC = gameObject.GetComponent<BoxCollider>();
        if (BC != null)
        {
            BC.enabled = false;
        }
        gameObject.SetActive(false);
        player.weapons.Add(this);

        originalWeaponPos = gameObject.transform.localPosition;
        originalWeaponRotation = gameObject.transform.localRotation;

        AudioManager.instance.PlaySFX("GunSFX", transform.position);
    }

    public void ShowUI()
    {
        if (pickUpText != null)
            pickUpText.SetActive(true);
    }

    public void HideUI()
    {
        if (pickUpText != null)
            pickUpText.SetActive(false);
    }

    /*<--------------Methods to update UI------------------->*/
    public void InvokeAmmoCountChanged()
    {
        OnAmmoCountChanged?.Invoke(ammoCount, weaponData.maxAmmo);
    }

    public void InvokeReloadCount(int reloadCount)
    {
        OnReloadCountChanged?.Invoke(reloadCount);

    }

    public void InvokeGunSpriteChange()
    {
        OnGunSpriteChanged?.Invoke(weaponData.imageUI);
    }

    public void InvokeCrossHairChanged()
    {
        if (!isScoping)
            OnCrosshairChanged?.Invoke(weaponData.corsshair, crosshairScale);
        else
            OnCrosshairChanged?.Invoke(weaponData.scopeCrosshair, scopeCrosshairScale);
    }

}
