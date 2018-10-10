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

        // Oriente le fusil affiché chez les autres joueurs selon la rotation de la camera
        gunRemote.transform.rotation = cameraTransform.rotation;

        hasNotShootSince += Time.deltaTime;

        // Si le chargeur est vide ou si le joueur est en train de recharger, lance la fonction de recharge
        if ((magazineState <= 0 && (ammoState > 0 || maxAmmo == -1)) || isReloading)
            Reload();

        if (!isReloading)
        {
            // Permet au joueur de recharger
            if (Input.GetKeyDown(KeyCode.R) && magazineState < magazineCapacity && (ammoState > 0 || maxAmmo == -1))
                Reload();

            // Permet au joueur de tirer
            if (Input.GetButton("Fire1") && hasNotShootSince > shootCoolDown && magazineState > 0)
            {
                hasNotShootSince = 0;

                // Instancie l'effet du tir localement
                InstantiateMuzzleFlash(muzzleFlashLocal);

                // Envoi la commande de tir au serveur avec la position et la direction du tir
                CmdFire(bulletSpawn.position, bulletSpawn.forward);

                magazineState--;
            }
        }
	}

    [Command]
    void CmdFire(Vector3 origin, Vector3 direction)
    {
        RaycastHit hit;

        // Prépare le raycast
        Ray ray = new Ray(origin, direction);

        // Instancie l'effet de tir sur les autres clients
        RpcFireEffect();

        // Effectu le raycast et récupère l'objet touché
        if (Physics.Raycast(ray, out hit, shootRange))
        {
            // Tente de récuperé le comsant Health sur l'objet touché
            Health hitted = hit.transform.GetComponent<Health>();

            Debug.DrawLine(ray.origin, hit.transform.position, Color.red, 1f);

            // Si le composant Health éxiste
            if (hitted != null)
            {
                // Si il ne s'agit pas d'un joueur
                if (hitted.type != Health.Type.PLAYER)
                    // Inflige les dégats à la cible
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

        // Vérifie que le temps de rechargement est arrivé à son terme avant de recharger l'arme
        if (isRelodingSince >= reloadingTime)
        {
            int magazineBulletCount = magazineCapacity;

            // Effectu un ajustement supplémentaire si les munitions ne sont pas infinies
            if (maxAmmo > 0)
            {
                // Défini le nombre de balle requise pour remplir complétement le magasin
                int ammoToRemove = magazineCapacity - magazineState;

                // Si les munitions réstantes ne sont pas suffisantes pour remplir un chargeur complet
                if (ammoState + magazineState < magazineCapacity)
                    // Remplie le chargeur avec les munitions réstantes
                    magazineBulletCount = ammoState + magazineState;

                // Soustrait le totale de balle utilisé pour remplir le magasin
                ammoState -= ammoToRemove;
            }

            // Applique le nombre de balle dans le chargeur
            magazineState = magazineBulletCount;

            isReloading = false;
            isRelodingSince = 0;
        }
        else
            isRelodingSince += Time.deltaTime;
    }

    void InstantiateMuzzleFlash(GameObject muzzleFlashReference)
    {
        // Instancie l'effet de tir
        GameObject muzzleFlash = Instantiate(muzzleFlashReference);

        // Placee l'effet à la bonne position
        muzzleFlash.transform.parent = muzzleFlashReference.transform.parent;
        muzzleFlash.transform.localPosition = muzzleFlashReference.transform.localPosition;
        muzzleFlash.transform.localRotation = muzzleFlashReference.transform.localRotation;

        // Active l'effet
        muzzleFlash.SetActive(true);
    }
}
