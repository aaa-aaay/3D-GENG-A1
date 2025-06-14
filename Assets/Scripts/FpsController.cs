using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FpsController : MonoBehaviour
{
    [Header("CAMERA")]
    [SerializeField] private float maxCameraLookAngle;
    [SerializeField] private float LookSense;
    private InputAction lookAction;
    private Camera playerCamera;
    private float cameraPitch;
    //bobbing
    [SerializeField] private float bobFrequency;
    [SerializeField] private float bobAmpli;
    private float bobbingOffset;
    private float timer;
    private bool isMoving;

    //for CameraShake
    private Vector3 shakeOffset;
    private InputAction shootAction;
    private Vector3 orignalCameraPosition;

    //for dynamic FOV
    private float originalFov;
    private float sprintFov;
    [SerializeField]private float sprintFovMultipler = 0.8f;
    private float currentFov;
    private bool isSprinting;
    private bool finishedSliding;
    

    [Header("MOVEMENTS")]
    [SerializeField] private float moveSpeed;
    [SerializeField] float sprintSpeed = 15;
    [SerializeField] float playerAcceleration = 1.0f;
    [SerializeField] private float slideSpeed = 2.0f;
    [SerializeField] private float slideTime = 2.0f;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction slideAction;
    private float interpolateValue = 0;
    private float finalSpeed;
    private bool isSliding;
    private float slideDirection;
    private Vector3 move;

    [Header("JUMP")]
    [SerializeField] private float jumpHeight;
    private InputAction jumpAction;
    private Vector3 jumpVelocity;

    [Header("CROUCH")]
    [SerializeField] private float crouchHeight = 1;
    private InputAction crouchAction;
    private float normalHeight = 2;

    [Header("Pick Up")]
    private InputAction pickUpAction;
    [SerializeField] private float pickUpRange;
    [SerializeField] private LayerMask itemLayer;

    [Header("Weapon List")]
    [SerializeField] public GameObject weaponHolderGO;
    [SerializeField] public List<Weapon> weapons = new List<Weapon>();
    private int currentWeaponIndex;
    [SerializeField] public int reloadCount = 3;

    [Header("current Weapon")]
    [SerializeField] private Weapon currentWeapon;
    private InputAction reloadAction, scopeAction, switchWeaponAction, dropWeaponAction;
    private float weaponScopingFOV;
    public Weapon CurrentWeapon { get { return currentWeapon; } set {  currentWeapon = value; } }


    private PlayerInput playerInput;
    private CharacterController cc;
    private Item lastHitItem;

    private float gravity = -9.81f;
    private Vector2 mouseDelta;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerCamera = GetComponentInChildren<Camera>();
        playerInput = GetComponent<PlayerInput>(); 
        cc = GetComponent<CharacterController>();

        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        crouchAction = playerInput.actions["Crouch"];
        shootAction = playerInput.actions["Attack"];
        slideAction = playerInput.actions["Slide"];
        pickUpAction = playerInput.actions["PickUp"];
        reloadAction = playerInput.actions["Reload"];
        switchWeaponAction = playerInput.actions["SwitchWeapon"];
        scopeAction = playerInput.actions["Scope"];
        dropWeaponAction = playerInput.actions["Drop"];

        //for bobbing
        timer = 0;
        //for camera shake
        orignalCameraPosition = playerCamera.transform.localPosition; //get orignal camera position
        //for dynamicFOV
        currentFov = originalFov = playerCamera.fieldOfView;
        sprintFov = originalFov * sprintFovMultipler;
        finishedSliding = false;

        lastHitItem = null;
        currentWeapon.InvokeReloadCount(reloadCount);
        currentWeapon.InvokeCrossHairChanged();
    }

    void Update()
    {
        Movement();
        Look();
        Jump();
        Crouch();
        WeaponInputs();
        PickUp();
        SwitchWeapon();
        jumpVelocity.y += gravity * Time.deltaTime;

    }

    private void LateUpdate()
    {
        LookUp();
        CameraBob();
        CameraShake();
        CameraFov();

        playerCamera.transform.localPosition = shakeOffset + orignalCameraPosition + new Vector3(0,bobbingOffset, 0);
    }
    private void Movement()
    {
        
        Vector3 Input = moveAction.ReadValue<Vector2>();
        move = transform.right * Input.x + transform.forward * Input.y;

        if (sprintAction.IsInProgress() && Input != Vector3.zero)
        {
            interpolateValue += Time.deltaTime * playerAcceleration;
            isSprinting = true;
        }
        else
        {
            isSprinting = false;
            interpolateValue -= Time.deltaTime * playerAcceleration;
        }
        interpolateValue = Mathf.Clamp01(interpolateValue);
        finalSpeed = Mathf.Lerp(moveSpeed, sprintSpeed, interpolateValue);
        Vector3 finalMove = jumpVelocity + move * finalSpeed;
        cc.Move(finalMove * Time.deltaTime);
        isMoving = move.magnitude > 0;


        //sliding
        if (slideAction.WasPressedThisFrame() && !isSliding && cc.isGrounded)
        {
            if(!isMoving) {
               
                StartCoroutine(StartSlide(new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z)));
            }
            else
            StartCoroutine(StartSlide(move));
        }
    }    

    private void Look()
    {
        mouseDelta = lookAction.ReadValue<Vector2>();
        transform.Rotate(Vector2.up * Time.deltaTime * mouseDelta.x * LookSense);
    }

    private void LookUp()
    {
        cameraPitch -= mouseDelta.y * LookSense * Time.deltaTime;
        cameraPitch = Mathf.Clamp(cameraPitch, -maxCameraLookAngle, maxCameraLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }

    private void Jump()
    {
        if(jumpAction.IsPressed() && cc.isGrounded)
            jumpVelocity.y = Mathf.Sqrt(-2 * gravity * jumpHeight);

        if(cc.isGrounded && jumpVelocity.y < 0)
            jumpVelocity.y = -2f;
    }

    private void Crouch()
    {
        if(crouchAction.IsPressed())
        {
            cc.height = crouchHeight;
            cc.center = new Vector3 (0, crouchHeight /2, 0); // adjust center to avoid clipping
        }
        else
        {
            cc.height = normalHeight;
            cc.center = new Vector3(0, 0, 0); // adjust center to avoid clipping
        }
    }

    private void CameraBob()
    {
        if(isMoving)
        {
            timer += Time.deltaTime * bobFrequency;
            bobbingOffset = Mathf.Sin(timer) * bobAmpli;
        }
        else
        {
            bobbingOffset = Mathf.Lerp(bobbingOffset,0, Time.deltaTime);
        }

        //weapon animation
        if (currentWeapon != null) currentWeapon.Animations(bobbingOffset);
    }

    private void CameraShake()
    {
        shakeOffset = currentWeapon.GetCameraShake();
    }

    private void CameraFov()
    {
        if (currentWeapon.IsScoping())
        {
            //for weapon FOV
            currentFov = weaponScopingFOV * originalFov;
        }
        else if (isSprinting)
        {
            // For Sprinting FOV
            currentFov = Mathf.Lerp(currentFov, sprintFov, Time.deltaTime * playerAcceleration);
        }
        else if(sprintAction.WasReleasedThisFrame())
        {
            // When stop sprinting lerp back,
            currentFov = Mathf.Lerp(currentFov, originalFov, Time.deltaTime * playerAcceleration);
        }
        else if (isSliding)
        {
            //Sliding FOV
            float targetFov;
            if (slideDirection > 0)
            {
                targetFov = originalFov * 0.8f;
            }
            else if (slideDirection < 0)
            {
                targetFov = originalFov * 1.2f;
            }
            else
            {
                targetFov = originalFov;
            }

            currentFov = Mathf.Lerp(currentFov, targetFov, slideTime);
        }
        else if (finishedSliding)
        {
            currentFov = Mathf.Lerp(currentFov, originalFov, 0.1f);
            if (Mathf.Abs(currentFov - originalFov) < 0.1f)
            {
                finishedSliding = false;
            }
        }
        else
        {
            currentFov = originalFov;
        }

        //apply the set FOV
        playerCamera.fieldOfView = currentFov;
    }

    //When interacting with Items
    private void PickUp()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        if (Physics.Raycast(ray, out RaycastHit hit, pickUpRange, itemLayer))
        {
            Item item = hit.collider.GetComponent<Item>();
            lastHitItem = item;
            item?.ShowUI();
            if (pickUpAction.IsPressed())
            {
                item?.Use(this);
            }   
        }
        else
        {
            lastHitItem?.HideUI();
        }
    }

    //When working with weapons
    private void WeaponInputs()
    {
        if (shootAction.IsPressed())
        {
            currentWeapon.Shoot();
        }

        if (reloadAction.WasPressedThisFrame())
        {
            reloadCount = currentWeapon.Reload(reloadCount);
        }

        if (scopeAction.WasPressedThisFrame())
        {
            weaponScopingFOV = currentWeapon.Scope();
        }

        if (dropWeaponAction.WasPressedThisFrame())
        {
            //check if there is more than 1 weapon.
            if(weapons.Count > 1)
            {
                currentWeapon.Drop(this);


                currentWeapon = weapons[0];
                currentWeapon.gameObject.SetActive(true);
                currentWeapon.transform.localScale = currentWeapon.originalWeaponScale;

                currentWeapon.InvokeAmmoCountChanged();
                currentWeapon.InvokeGunSpriteChange();
                currentWeapon.InvokeCrossHairChanged();
                //switchweapon to the next in the list
            }


        }
    }

    private void SwitchWeapon()
    {
        Vector2 scroll = switchWeaponAction.ReadValue<Vector2>();

        if (scroll.y == 0) return;
        if (weapons.Count == 1) return;

        if (currentWeapon.IsScoping()) //if weapon is scoped when switching wepaon, stop scoping first
            currentWeapon.Scope();
        

        currentWeapon.transform.localScale = Vector3.zero; // to make weapon auto scope Out when switching when scoped in (wont work if set active to false)
        currentWeaponIndex = (currentWeaponIndex + (scroll.y < 0 ? 1 : -1) + weapons.Count) % weapons.Count;
        currentWeapon = weapons[currentWeaponIndex];

        // Update the UI
        currentWeapon.InvokeAmmoCountChanged();
        currentWeapon.InvokeGunSpriteChange();
        currentWeapon.InvokeCrossHairChanged();

        // Show the new weapon
        currentWeapon.gameObject.SetActive(true);
        currentWeapon.transform.localScale = currentWeapon.originalWeaponScale;

        //play audiuo
        AudioManager.instance.PlaySFX("GunSFX", transform.position);
    }

    IEnumerator StartSlide(Vector3 direction)
    {
        float oringalSlideTime = slideTime;
        float slopeAngle;
        isSliding = true;
        float startTime = Time.time;

        if (Physics.Raycast(transform.position,Vector3.down, out RaycastHit hit, 2.0f)){
             slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, hit.normal).normalized;

            if (slopeAngle > 0f) // If there is a slope
            {
                direction = slopeDirection; // Update sliding direction to follow slope
            }
        }

        while (Time.time < startTime + slideTime)
        {

            float timeFraction = (Time.time - startTime) / slideTime;

            float currentSlideSpeed;

            Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit2, 2.0f);
            slopeAngle = Vector3.Angle(hit2.normal, Vector3.up);

            if (slopeAngle > 0) {

                currentSlideSpeed = slideSpeed;

                Physics.Raycast(transform.position + transform.forward * 0.3f, Vector3.down, out RaycastHit hit3, 2.0f);
                if (hit2.point.y > hit3.point.y) //when down a slope
                {
                    slideTime += Time.deltaTime;
                    direction = Vector3.ProjectOnPlane(Vector3.down, hit2.normal).normalized;
                }
                else if (hit2.point.y < hit3.point.y) //when moving up a slope
                {
                    slideTime -= Time.deltaTime;
                }
            }
            else
            {
                currentSlideSpeed = Mathf.Lerp(slideSpeed, 0, timeFraction);
            }

            //couch
            cc.height = crouchHeight;
            cc.center = new Vector3(0, crouchHeight / 2, 0); 

            //apply direction and force to move
            Vector3 gravityEffect = 9.81f * Mathf.Sin(slopeAngle) * Time.deltaTime * Vector3.down ;
            cc.Move( (direction * currentSlideSpeed + gravityEffect) * Time.deltaTime);

            float forwardDirection = Vector3.Dot(direction.normalized, transform.forward);
            slideDirection = Mathf.Sign(forwardDirection);

            yield return null;
        }

        //reset values
        slideTime = oringalSlideTime;
        cc.height = normalHeight;
        cc.center = new Vector3(0, 0, 0); 
        isSliding = false;
        finishedSliding = true;
        slideDirection = 0;
    }


}
    