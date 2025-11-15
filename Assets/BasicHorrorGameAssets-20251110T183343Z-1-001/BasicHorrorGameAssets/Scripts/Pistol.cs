﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pistol : MonoBehaviour
{
    public int maxAmmoInMag = 10;       // Maximum ammo capacity in the magazine
    public int maxAmmoInStorage = 30;   // Maximum ammo capacity in the storage
    public float shootCooldown = 0.5f;  // Cooldown time between shots
    public float reloadCooldown = 0.5f;  // Cooldown time between shots
    private float switchCooldown = 0.5f;  // Cooldown time between shots
    public float shootRange = 100f;     // Range of the raycast

    public ParticleSystem impactEffect; // Particle effect for impact

    public int currentAmmoInMag;       // Current ammo in the magazine
    public int currentAmmoInStorage;   // Current ammo in the storage
    public int damager;   // Current ammo in the storage
    public bool canShoot = true;       // Flag to check if shooting is allowed
    public bool canSwitch = true;       // Flag to check if shooting is allowed
    private bool isReloading = false;   // Flag to check if reloading is in progress
    private float shootTimer;           // Timer for shoot cooldown

    public Transform cartridgeEjectionPoint; // Ejection point of the cartridge
    public GameObject cartridgePrefab; // Prefab of the cartridge
    public float cartridgeEjectionForce = 5f; // Force applied to the cartridge

    public Animator gun;
    public ParticleSystem muzzleFlash;
    public GameObject muzzleFlashLight;
    public AudioSource shoot;

    void Start()
    {
        currentAmmoInMag = maxAmmoInMag;
        currentAmmoInStorage = maxAmmoInStorage;
        canSwitch = true;
        muzzleFlashLight.SetActive(false);
    }

    void Update()
    {
        // Update current ammo counts
        currentAmmoInMag = Mathf.Clamp(currentAmmoInMag, 0, maxAmmoInMag);
        currentAmmoInStorage = Mathf.Clamp(currentAmmoInStorage, 0, maxAmmoInStorage);

        // Check for shoot input
        if (Input.GetButtonDown("Fire1") && canShoot && !isReloading)
        {
            Debug.Log("Botón de disparo presionado");
            switchCooldown = shootCooldown;
            Shoot();
        }

        // Check for reload input
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Botón de recarga presionado");
            switchCooldown = reloadCooldown;
            Reload();
        }

        // Update the shoot timer
        if (shootTimer > 0f)
        {
            shootTimer -= Time.deltaTime;
        }
    }

    void Shoot()
    {
        Debug.Log("Entrando a método Shoot()");
        
        // Check if there is ammo in the magazine
        if (currentAmmoInMag > 0 && shootTimer <= 0f)
        {
            Debug.Log("Hay munición y el timer es <= 0");
            canSwitch = false;
            shoot.Play();
            muzzleFlash.Play();
            muzzleFlashLight.SetActive(true);
            gun.SetBool("shoot", true);

            // Perform the shoot action
            RaycastHit hit;
            Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f)); // Centro de la pantalla
            Ray ray = new Ray(rayOrigin, Camera.main.transform.forward);
            
            Debug.DrawRay(rayOrigin, Camera.main.transform.forward * shootRange, Color.red, 2f); // Dibuja rayo en la escena

            if (Physics.Raycast(ray, out hit, shootRange))
            {
                Debug.Log("Raycast golpeó algo: " + hit.collider.name + " (Tag: " + hit.collider.tag + ")");

                // Check if the hit object has the "enemy" tag
                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log("Raycast golpeó al enemigo: " + hit.collider.name);
                    
                    // Get the EnemyHealth component from the hit object
                    EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();

                    // Check if the enemy has the EnemyHealth component
                    if (enemyHealth != null)
                    {
                        Debug.Log("Enemigo tiene EnemyHealth, aplicando daño: " + damager);
                        enemyHealth.TakeDamage(damager);
                        
                        // Instantiate impact effect at the hit point
                        Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    }
                    else
                    {
                        Debug.LogError("ERROR: Enemigo no tiene script EnemyHealth adjunto: " + hit.collider.name);
                    }
                }
                else
                {
                    Debug.Log("El objeto golpeado no es un enemigo, es: " + hit.collider.tag);
                }
            }
            else
            {
                Debug.Log("Raycast no golpeó nada");
            }

            // Instantiate the empty cartridge
            GameObject cartridge = Instantiate(cartridgePrefab, cartridgeEjectionPoint.position, cartridgeEjectionPoint.rotation);
            Rigidbody cartridgeRigidbody = cartridge.GetComponent<Rigidbody>();

            // Apply force to eject the cartridge
            cartridgeRigidbody.AddForce(cartridgeEjectionPoint.right * cartridgeEjectionForce, ForceMode.Impulse);

            StartCoroutine(endAnimations());
            StartCoroutine(endLight());
            StartCoroutine(canswitchshoot());

            switchCooldown -= Time.deltaTime;

            // Reduce ammo count
            currentAmmoInMag--;
            Debug.Log("Munición restante: " + currentAmmoInMag);

            // Start the shoot cooldown
            shootTimer = shootCooldown;
        }
        else
        {
            // Out of ammo in the magazine or shoot on cooldown
            Debug.Log("No se puede disparar - Munición: " + currentAmmoInMag + ", Timer: " + shootTimer);
        }
    }

    void Reload()
    {
        switchCooldown -= Time.deltaTime;
        // Check if already reloading or out of ammo in the storage
        if (isReloading || currentAmmoInStorage <= 0)
        {
            Debug.Log("No se puede recargar - Recargando: " + isReloading + ", Munición: " + currentAmmoInStorage);
            return;
        }

        // Calculate the number of bullets to reload
        int bulletsToReload = maxAmmoInMag - currentAmmoInMag;

        // Check if there is enough ammo in the storage for reloading
        if (bulletsToReload > 0)
        {
            Debug.Log("Iniciando recarga de " + bulletsToReload + " balas");
            gun.SetBool("reload", true);
            StartCoroutine(endAnimations());

            // Determine the actual number of bullets to reload based on available ammo
            int bulletsAvailable = Mathf.Min(bulletsToReload, currentAmmoInStorage);

            // Update ammo counts
            currentAmmoInMag += bulletsAvailable;
            currentAmmoInStorage -= bulletsAvailable;

            Debug.Log("Recargado " + bulletsAvailable + " balas. Munición actual: " + currentAmmoInMag);

            // Start the reload cooldown
            StartCoroutine(ReloadCooldown());
        }
        else
        {
            Debug.Log("No se puede recargar - ya está lleno o sin munición");
        }
    }

    IEnumerator ReloadCooldown()
    {
        isReloading = true;
        canShoot = false;
        canSwitch = true;

        yield return new WaitForSeconds(reloadCooldown);

        isReloading = false;
        canShoot = true;
        canSwitch = true;
        
        Debug.Log("Recarga completada");
    }

    IEnumerator endAnimations()
    {
        yield return new WaitForSeconds(.1f);
        gun.SetBool("shoot", false);
        gun.SetBool("reload", false);
    }

    IEnumerator endLight()
    {
        yield return new WaitForSeconds(.1f);
        muzzleFlashLight.SetActive(false);
    }

    IEnumerator canswitchshoot()
    {
        yield return new WaitForSeconds(shootCooldown);
        canSwitch = true;
    }
}