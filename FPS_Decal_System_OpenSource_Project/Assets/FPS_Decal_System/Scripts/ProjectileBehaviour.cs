using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : DecalSystem
{
    [SerializeField][Header("Projectile Settings")]
    private float speed;
    [SerializeField]
    private float gravity;

    private LayerMask ballisticLayersToHit;
    private LayerMask ballisticStoppingLayers;

    private bool bulletPenetration;
    private bool leaveExitDecal;
    private int numberOfPenetrations;

    private int passes = 0;

    private Rigidbody rb;

    /// <summary>
    /// Takes the values from the shooting script and copies them to the projectile's own values.
    /// </summary>
    /// <param name="shootingScript">The master script that instantiated this projectile.</param>
    public void ProjectileSetup(ShootingScript shootingScript)
    {
        decalProjector = shootingScript.decalProjector;
        bulletPenetration = shootingScript.bulletPenetration;
        numberOfPenetrations = shootingScript.numberOfPenetrations;
        leaveExitDecal = shootingScript.leavesExitDecals;
        ballisticLayersToHit = shootingScript.layersToHit;
        ballisticStoppingLayers = shootingScript.stopLayer;
        splatterEnabled = shootingScript.splatterEnabled;
        splatterParticleSystem = shootingScript.splatterParticleSystem;
        splatterRange = shootingScript.splatterRange;
        splatterMask = shootingScript.splatterMask;
        bleedingEnabled = shootingScript.bleedingEnabled;
        bleedChance = shootingScript.bleedChance;
        bleedParticleSystem = shootingScript.bleedParticleSystem;
        layersToSpawnBleedEffects = shootingScript.layersToSpawnBleedEffects;
        decals = shootingScript.decals;
        splatterDecals = shootingScript.splatterDecals;
    }

    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb.velocity = transform.forward * speed * Time.deltaTime;

        rb.AddForce((Vector3.down * gravity), ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other)
    {
        RaycastHit hit;

        Vector3 origin = transform.position - transform.forward.normalized * 1f;
        Vector3 direction = transform.forward.normalized;

        if (bulletPenetration)
        {
            if (Physics.Raycast(origin, direction, out hit, 1f, ballisticLayersToHit) && passes == numberOfPenetrations)
            {
                Vector3 spawnPos = hit.point + hit.normal.normalized * 0.001f;
                LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit, true);
                StopProjectile();
            }
            else if (Physics.Raycast(origin, direction, out hit, 1f, ballisticStoppingLayers))
            {
                Vector3 spawnPos = hit.point + hit.normal.normalized * 0.001f;
                LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit, true);
                StopProjectile();
            }
            else if (Physics.Raycast(origin, direction, out hit, 1f, ballisticLayersToHit))
            {
                Vector3 spawnPos = hit.point + hit.normal.normalized * 0.001f;
                LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit, true);
                passes++;
            }
        }
        else if (!bulletPenetration)
        {
            if (Physics.Raycast(origin, direction, out hit, 1f, ballisticLayersToHit))
            {
                Vector3 spawnPos = hit.point + hit.normal.normalized * 0.001f;
                LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit, true);
                StopProjectile();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (leaveExitDecal)
        {
            RaycastHit hit;

            Vector3 origin = transform.position + transform.forward.normalized * 0.05f;
            Vector3 direction = transform.forward.normalized;

            if (Physics.Raycast(origin, -direction, out hit, 1f, ballisticLayersToHit))
            {
                Vector3 spawnPos = hit.point + hit.normal.normalized * 0.001f;
                LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit, false);
            }
        }
    }

    private void StopProjectile()
    {
        rb.velocity = Vector3.zero;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rb.isKinematic = true;
    }
}
