using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Gun : NetworkBehaviour
{
    [Header("Gun parts")]
    [SerializeField] Transform bulletSpawn;
    [SerializeField] Transform gunRemote;
    [SerializeField] GameObject muzzleFlashRemote;
    [SerializeField] Transform gunLocal;
    [SerializeField] GameObject muzzleFlashLocal;
    [SerializeField] GameObject bulletPrefabs;
    [SerializeField] AudioSource reloadingSound;

    [Header("Gun attributs")]
    [SerializeField] int maxAmmo;
    [SerializeField] public int magazineCapacity;
    [SerializeField] float reloadingTime;
    [SerializeField] float shootCoolDown;
    [SerializeField] int shootDamage;
    [SerializeField] float shootRange;
    [SerializeField] Transform cameraTransform;

    public int ammoState;
    public int magazineState;

    float hasNotShootSince = 0f;
    float isRelodingSince = 0f;

    public bool isReloading = false;
    public bool isShooting = false;

    Coroutine coroutineShoot;

    private void Start()
    {
        if (!isLocalPlayer)
            return;

        hasNotShootSince = shootCoolDown;
        ammoState = maxAmmo;
        magazineState = magazineCapacity;
    }

    // Update is called once per frame
    void Update ()
    {
        if (!isLocalPlayer)            
            return;
        
        // Rotate the remote gun according to the local camera rotation
        gunRemote.transform.rotation = cameraTransform.rotation;

        hasNotShootSince += Time.deltaTime;

        // If the magazine is empty or the player is reloading, reload
        if ((magazineState <= 0 && (ammoState > 0 || maxAmmo == -1)) || isReloading)
            Reload();

        if (!isReloading)
        {
            // Allow the player to reload
            if (Input.GetKeyDown(KeyCode.R) && magazineState < magazineCapacity && (ammoState > 0 || maxAmmo == -1))
                Reload();

            // Allow the player to shoot
            if (Input.GetButton("Fire1") && hasNotShootSince > shootCoolDown && magazineState > 0)
            {
                hasNotShootSince = 0;
                
                // Instanciate the local shoot effect
                InstantiateMuzzleFlash(muzzleFlashLocal);

                // Send shoot command to the server with the origin and the direction
                CmdFire(bulletSpawn.position, bulletSpawn.forward);

                magazineState--;
            }
        }
	}

    [Command]
    void CmdFire(Vector3 origin, Vector3 direction)
    {
        RaycastHit hit;

        // Prepare the raycast
        Ray ray = new Ray(origin, direction);

        // Instanciate the remote shoot effect
        RpcFireEffect();

        // Do the raycast and get the hitted object
        if (Physics.Raycast(ray, out hit, shootRange))
        {
            // Try to get the Health component
            Health hitted = hit.transform.GetComponent<Health>();

            Debug.DrawLine(ray.origin, hit.transform.position, Color.red, 1f);

            // If it exists
            if (hitted != null)
            {
                // If it is not a player
                if (hitted.type != Health.Type.PLAYER)
                    // Deal damage
                    hitted.TakeDamage(shootDamage);
            }
        }
    }

    [ClientRpc]
    void RpcFireEffect()
    {
        if (isLocalPlayer)
            return;

        InstantiateMuzzleFlash(muzzleFlashRemote);
    }

    void Reload()
    {
        isReloading = true;

        if (!reloadingSound.isPlaying)
            reloadingSound.Play();
        
        // Check if the reloading action is finished
        if (isRelodingSince >= reloadingTime)
        {
            int magazineBulletCount = magazineCapacity;
            
            // Do a specific adjustement if ammo are not unlimited
            if (maxAmmo > 0)
            {
                // Define the number of bullets needed to completely fill the magazine
                int ammoToRemove = magazineCapacity - magazineState;
                
                // If the ammo left are not enough to fully fill the magazine
                if (ammoState + magazineState < magazineCapacity)
                    // Fill the magazine with the all the bullets left
                    magazineBulletCount = ammoState + magazineState;
                
                // Substract the total ammo used to fill the magazine
                ammoState -= ammoToRemove;
            }
            
            // Apply the bullet number to the magazine
            magazineState = magazineBulletCount;

            isReloading = false;
            isRelodingSince = 0;
        }
        else
            isRelodingSince += Time.deltaTime;
    }

    void InstantiateMuzzleFlash(GameObject muzzleFlashReference)
    {
        // Instanciate the shooting effect
        GameObject muzzleFlash = Instantiate(muzzleFlashReference);

        // Place the effect on the right position
        muzzleFlash.transform.parent = muzzleFlashReference.transform.parent;
        muzzleFlash.transform.localPosition = muzzleFlashReference.transform.localPosition;
        muzzleFlash.transform.localRotation = muzzleFlashReference.transform.localRotation;

        // Enable the effect
        muzzleFlash.SetActive(true);
    }
}
