using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShootingScript : DecalSystem
{
    private Transform cam;

    [SerializeField][Header("Raycast and Ballistic Collision Settings")]
    private float raycastRange;
    public LayerMask layersToHit;
    public LayerMask stopLayer;

    [SerializeField][Header("Ballistic Shooting Settings")]
    private bool usingBallistic = false;
    [SerializeField]
    private GameObject projectile;

    [Header("Bullet Penetration Settings")]
    public bool bulletPenetration;
    public int numberOfPenetrations;
    public bool leavesExitDecals;

    private void Start()
    {
        cam = Camera.main.transform;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if (usingBallistic)
            {
                ShootBallistic();
            }
            else if (!usingBallistic)
            {
                ShootHitscan();
            }
        }
    }

    /// <summary>
    /// Shoots a raycast directly forward from the center of the camera.
    /// </summary>
    private void ShootHitscan()
    {
        if (!bulletPenetration)
        {
            RaycastHit hit;

            if (Physics.Raycast(cam.position, cam.forward, out hit, raycastRange))
            {
                Vector3 spawnPos = hit.point;
                LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, hit.normal), hit, true);
            }
        }
        else if (bulletPenetration)
        {
            int stoppingIndex = 0;
            int passes = 0;
            RaycastHit[] entryHits = Physics.RaycastAll(cam.position, cam.forward, raycastRange, layersToHit).OrderBy(h => h.distance).ToArray();

            for (int i = 0; i < entryHits.Length; i++)
            {
                if (1 << entryHits[i].transform.gameObject.layer == stopLayer.value)
                {
                    Vector3 spawnPos = entryHits[i].point;
                    LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, entryHits[i].normal), entryHits[i], true);
                    stoppingIndex = i;
                    break;
                }
                if (passes == numberOfPenetrations)
                {
                    Vector3 spawnPos = entryHits[i].point;
                    LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, entryHits[i].normal), entryHits[i], true);
                    stoppingIndex = i;
                    break;
                }
                else
                {
                    Vector3 spawnPos = entryHits[i].point;
                    LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, entryHits[i].normal), entryHits[i], true);
                    passes++;
                }
            }

            if (entryHits.Length != 0 && leavesExitDecals)
            {
                if (1 << entryHits[stoppingIndex].transform.gameObject.layer == stopLayer.value || passes == numberOfPenetrations)
                {
                    float distance = Vector3.Distance(cam.position, entryHits[stoppingIndex].point);
                    RaycastHit[] exitHits = Physics.RaycastAll(entryHits[stoppingIndex].point, -cam.forward, distance, layersToHit).OrderByDescending(h => h.distance).ToArray();

                    for (int i = 0; i < exitHits.Length; i++)
                    {
                        Vector3 spawnPos = exitHits[i].point; 
                        LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, exitHits[i].normal), exitHits[i], false);
                    }
                }
                else
                {
                    RaycastHit[] exitHits = Physics.RaycastAll(cam.position + (cam.forward * raycastRange), -cam.forward, raycastRange, layersToHit).OrderByDescending(h => h.distance).ToArray();

                    for (int i = 0; i < exitHits.Length; i++)
                    {
                        Vector3 spawnPos = exitHits[i].point;
                        LeaveDecal(spawnPos, Quaternion.FromToRotation(Vector3.forward, exitHits[i].normal), exitHits[i], false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Shoots a projectile directly forward from the center of the camera.
    /// 
    /// Requires passing in of this script to transfer settings across to the projectile prefab.
    /// </summary>
    private void ShootBallistic()
    {
        GameObject projectileObject = Instantiate(projectile, cam.position, cam.rotation);
        projectileObject.GetComponent<ProjectileBehaviour>().ProjectileSetup(this);
    }
}
