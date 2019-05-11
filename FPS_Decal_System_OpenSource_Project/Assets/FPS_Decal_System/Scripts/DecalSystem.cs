using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecalSystem : MonoBehaviour
{
    public GameObject decalProjector;

    [Header("Decal Settings")]
    public Decals[] decals;

    [Header("Splatter Settings")]
    public bool splatterEnabled;
    public SplatterDecals[] splatterDecals;
    public GameObject splatterParticleSystem;
    //public int numberOfDecals;
    //public float verticalSpread;
    //public float horizontalSpread;
    public float splatterRange;
    public LayerMask splatterMask;

    [Header("Bleed Settings")]
    public bool bleedingEnabled;
    public GameObject bleedParticleSystem;
    [Range(0, 100)]
    public float bleedChance;
    public LayerMask layersToSpawnBleedEffects;

    /// <summary>
    /// Set the parent transform of a passed in child object.
    /// </summary>
    /// <param name="child">A Unity GameObject</param>
    /// <param name="parent"></param>
    private void SetParentObject(GameObject child, GameObject parent)
    {
        if (parent != null)
            child.transform.SetParent(parent.transform);
    }

    /// <summary>
    /// Get a projector from the pool and place it at the correct location and rotation. Activate the gameobject and adjust the size, material and parent transform.
    /// </summary>
    /// <param name="position">The position in the world where the projector will be placed.</param>
    /// <param name="rotation">The rotation in the world where the projector will be placed.</param>
    /// <param name="hit">Details from the raycast hit.</param>
    /// <param name="_decals">Materials for each of the decals to be randomly projected.</param>
    private void DrawDecalProjector(Vector3 position, Quaternion rotation, RaycastHit hit, Decals _decals)
    {
        GameObject projector = ObjectPooler.instance.GetPooledObject();
        if (projector != null && _decals.decals != null)
        {
            projector.transform.position = position;
            projector.transform.rotation = Quaternion.FromToRotation(Vector3.forward, -hit.normal);
            projector.SetActive(true);

            int randIndex = Random.Range(0, _decals.decals.Length);
            Material randMaterial = _decals.decals[randIndex];

            Projector _projector = projector.GetComponentInChildren<Projector>();
            _projector.material = randMaterial;
            _projector.orthographicSize = _decals.decalSize;
            _projector.farClipPlane = _decals.decalDepth;

            SetParentObject(projector, hit.collider.gameObject);
        }
    }

    /// <summary>
    /// Get a projector from the pool and place it at the correct location and rotation. Activate the gameobject and adjust the size, material and parent transform.
    /// </summary>
    /// <param name="position">The position in the world where the projector will be placed.</param>
    /// <param name="rotation">The rotation in the world where the projector will be placed.</param>
    /// <param name="splatterParent">The parent transform to place the splatter projectors into.</param>
    /// <param name="hit">Details from the raycast hit.</param>
    private void DrawSplatterProjector(Vector3 position, Quaternion rotation, GameObject splatterParent, RaycastHit hit, SplatterDecals _splatterDecals)
    {
        for (int i = 0; i < _splatterDecals.numberOfDecals; i++)
        {
            GameObject projector = ObjectPooler.instance.GetPooledObject();
            if (projector != null)
            {
                projector.transform.position = position;
                projector.transform.rotation = Quaternion.FromToRotation(Vector3.forward, -hit.normal);
                projector.SetActive(true);

                SetParentObject(projector, splatterParent);

                projector.transform.localPosition = GenerateRandomSplatterSpread(_splatterDecals.horizontalSpread, _splatterDecals.verticalSpread);

                int randIndex = Random.Range(0, _splatterDecals.decals.Length);
                Material randMaterial = _splatterDecals.decals[randIndex];

                Projector _projector = projector.GetComponentInChildren<Projector>();
                _projector.material = randMaterial;
                _projector.orthographicSize = _splatterDecals.decalSize;
                _projector.farClipPlane = _splatterDecals.decalDepth;
            }
        }
    }

    /// <summary>
    /// Spawns a decal and tries to use the exit decal assets if available. Spawns splattering effects/decals from the exit location.
    /// </summary>
    /// <param name="position">The position in the world to spawn a decal/splatter effect.</param>
    /// <param name="rotation">The rotation in the world to spawn a decal/splatter effect.</param>
    /// <param name="hit">Information about the object hit.</param>
    private void LeaveExitDecal(Vector3 position, Quaternion rotation, RaycastHit hit)
    {
        // Leave decal override returns an array if the object hit has a tag that matches with one in the OverrideDecal class
        // Also needs to have an array passed in for if no overrides were found.
        DrawDecalProjector(position, rotation, hit, CheckOverrides(hit));

        if (splatterEnabled)
        {
            SplatterFromPoint(hit.point, Camera.main.transform.forward, hit);

            if (splatterParticleSystem != null)
            {
                SpawnSplatterParticle(hit.point, Camera.main.transform.forward, hit.collider.gameObject);
            }
        }
    }

    /// <summary>
    /// Leaves a decal based on information passed in.
    /// </summary>
    /// <param name="position">The position in the world to spawn a decal.</param>
    /// <param name="rotation">The rotation in the world to spawn a decal.</param>
    /// <param name="hit">Information about the object hit.</param>
    /// <param name="isEntry">Whether or not the hit is the entry or exit.</param>
    public void LeaveDecal(Vector3 position, Quaternion rotation, RaycastHit hit, bool isEntry)
    {
        if (isEntry)
        {
            // Leave decal override returns an array if the object hit has a tag that matches with one in the OverrideDecal class
            // Also needs to have an array passed in for if no overrides were found.
            DrawDecalProjector(position, rotation, hit, CheckOverrides(hit));

            if (bleedingEnabled && 1 << hit.transform.gameObject.layer == layersToSpawnBleedEffects.value)
            {
                SpawnBleedParticle(position, rotation, hit.collider.gameObject);
            }
        }
        else if (!isEntry)
        {
            LeaveExitDecal(position, rotation, hit);
        }
    }

    /// <summary>
    /// Checks to see if the hit colldier has a valid tag override and return the Decal class related to that override. If not returns the first/blank element.
    /// </summary>
    /// <param name="hit">Raycast hit information.</param>
    /// <returns></returns>
    private Decals CheckOverrides(RaycastHit hit)
    {
        Decals defaultDecals = null;
        string tag = hit.collider.tag;

        for (int i = 0; i < decals.Length; i++)
        {
            if (decals[i].overrideTag == "")
            {
                defaultDecals = decals[i];
            }

            if (tag == decals[i].overrideTag)
            {
                return decals[i];
            }
        }

        if (decals.Length > 0)
        {
            defaultDecals = decals[0];
        }

        return defaultDecals;
    }

    /// <summary>
    /// Checks to see if the hit colldier has a valid tag override and return the SplatterDecal class related to that override. If not returns the first/blank element.
    /// </summary>
    /// <param name="hit">Raycast hit information.</param>
    /// <returns></returns>
    private SplatterDecals CheckSplatterOverrides(RaycastHit hit)
    {
        SplatterDecals defaultDecals = null;
        string tag = hit.collider.tag;

        for (int i = 0; i < splatterDecals.Length; i++)
        {
            if (splatterDecals[i].overrideTag == "")
            {
                defaultDecals = splatterDecals[i];
            }

            if (tag == splatterDecals[i].overrideTag)
            {
                return splatterDecals[i];
            }
        }

        if (splatterDecals.Length > 0)
        {
            defaultDecals = splatterDecals[0];
        }

        return defaultDecals;
    }

    private void SpawnBleedParticle(Vector3 position, Quaternion direction, GameObject parent)
    {
        float randomNumber = Random.Range(0, 100);

        if (randomNumber <= bleedChance)
        {
            GameObject bleedParent = new GameObject("BleedParent");
            bleedParent.AddComponent<DestroyObject>();
            bleedParent.GetComponent<DestroyObject>().SetState(true, 10f);
            Instantiate(bleedParticleSystem, position, direction, bleedParent.transform);

            SetParentObject(bleedParent, parent);
        }
    }

    /// <summary>
    /// Create a splatter particle effect and play the effect once.
    /// </summary>
    /// <param name="postion">The position in the world to spawn the particle effect.</param>
    /// <param name="direction">The direction the particle effect will emit towards.</param>
    /// <param name="parent">The gameobject the particle system should become a child of.</param>
    private void SpawnSplatterParticle(Vector3 postion, Vector3 direction, GameObject parent)
    {
        GameObject splatterParent = new GameObject("SplatterParent");
        splatterParent.AddComponent<DestroyObject>();
        splatterParent.GetComponent<DestroyObject>().SetState(true, 3f);
        Instantiate(splatterParticleSystem, postion, Quaternion.FromToRotation(Vector3.forward, direction), splatterParent.transform);

        SetParentObject(splatterParent, parent);
    }

    /// <summary>
    /// Spawns splatter decals from a position and gets hit information of the splatter target.
    /// </summary>
    /// <param name="position">The position in the world to start the splatter raycast.</param>
    /// <param name="rotation">The direction in the world to cast the splatter.</param>
    private void SplatterFromPoint(Vector3 position, Vector3 rotation, RaycastHit _hit)
    {
        RaycastHit hit;

        if (Physics.Raycast(position, rotation, out hit, splatterRange, splatterMask))
        {
            Vector3 spawnPoint = hit.point + hit.normal.normalized * 0.001f;

            GameObject parentDecal = new GameObject("SplatterObject");
            parentDecal.transform.position = spawnPoint;
            parentDecal.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);

            DrawSplatterProjector(spawnPoint, Quaternion.FromToRotation(Vector3.forward, hit.normal), parentDecal, hit, CheckSplatterOverrides(_hit));
            
            SetParentObject(parentDecal, hit.collider.gameObject);
        }
    }

    /// <summary>
    /// Generate a random amount of spread between a range (+-) in both horizontal and vertical.
    /// </summary>
    /// <returns>Vector 3 representing the amount of random offset generated.</returns>
    private Vector3 GenerateRandomSplatterSpread(float horizontal, float vertical)
    {
        Vector3 offsetSpawnPoint;
        offsetSpawnPoint.x = Random.Range(-horizontal, horizontal);
        offsetSpawnPoint.y = Random.Range(-vertical, vertical);
        offsetSpawnPoint.z = 0f;

        return offsetSpawnPoint;
    }
}

[System.Serializable]
public class Decals
{
    [Tooltip("An array of materials that are randomly selected from when a decal is placed. Materials must be using a Projector shader.")]
    public Material[] decals;
    [Tooltip("The size of the projection onto surfaces.")]
    public float decalSize;
    [Tooltip("Leave a blank value to make a set of decals the default. If no blank override string exists then the first decal set in the array is used instead.")]
    public string overrideTag;
    [Tooltip("Adjusts the far clipping plane of the projector. Set higher if decals are fadding out or lower if decals are being placed on multiple sides of an object.")]
    public float decalDepth;
}

[System.Serializable]
public class SplatterDecals
{
    [Tooltip("An array of materials that are randomly selected from when a decal is placed. Materials must be using a Projector shader.")]
    public Material[] decals;
    [Tooltip("The size of the projection onto surfaces.")]
    public float decalSize;
    [Tooltip("Adjusts the far clipping plane of the projector. Set higher if decals are fadding out or lower if decals are being placed on multiple sides of an object.")]
    public float decalDepth;
    [Tooltip("How many splatter decals are created per hit.")]
    public int numberOfDecals;
    [Tooltip("Adjusts the random spread for the placement of the splatter decals.")]
    public float horizontalSpread, verticalSpread;
    [Tooltip("Leave a blank value to make a set of decals the default. If no blank override string exists then the first decal set in the array is used instead.")]
    public string overrideTag;
}
