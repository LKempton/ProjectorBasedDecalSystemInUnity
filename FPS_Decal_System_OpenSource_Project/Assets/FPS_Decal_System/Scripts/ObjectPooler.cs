using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler instance;

    public List<GameObject> pooledObjects;
    public List<GameObject> activeObjects;
    public GameObject objectToPool;
    public float deactivateTime = 5.0f;
    public int numberOfObjects;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        pooledObjects = new List<GameObject>();

        for (int i = 0; i < numberOfObjects; i++)
        {
            GameObject obj = (GameObject)Instantiate(objectToPool);
            obj.SetActive(false);
            obj.AddComponent<RePoolObject>();
            obj.GetComponent<RePoolObject>().SetState(true, deactivateTime);
            SetParentObject(obj, gameObject);
            pooledObjects.Add(obj);
        }
    }

    public void SetParentObject(GameObject child, GameObject parent)
    {
        if (parent != null)
            child.transform.SetParent(parent.transform);
    }

    public GameObject GetPooledObject()
    {
        for (int i = 0; i < pooledObjects.Count; i++)
        {
            if (!pooledObjects[i].activeInHierarchy)
            {
                activeObjects.Add(pooledObjects[i]);
                return pooledObjects[i];
            }
        }

        if (activeObjects.Count > 0)
        {
            GameObject returnObj = activeObjects[0];
            activeObjects[0].GetComponent<RePoolObject>().Repool();
            activeObjects.Add(returnObj);
            return returnObj;
        }
        else
        {
            return null;
        }
    }

    public void DeactivateObject(GameObject obj)
    {
        if (activeObjects.Count > 0)
        {
            for (int i = 0; i < activeObjects.Count; i++)
            {
                if (obj == activeObjects[i])
                {
                    activeObjects.RemoveAt(i);
                    break;
                }
            }
        }
    }
}
