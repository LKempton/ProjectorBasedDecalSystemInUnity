using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    [SerializeField]
    private bool destroyAfterTime = true;

    [SerializeField]
    private float destroyTime = 5.0f;

    private void Start()
    {
        BeginDestroy();
    }

    private void BeginDestroy()
    {
        if (destroyAfterTime)
        {
            Invoke("Destroy", destroyTime);
        }
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }

    public void SetState(bool _destroyAfterTime, float _destroyTime)
    {
        destroyAfterTime = _destroyAfterTime;
        destroyTime = _destroyTime;

        BeginDestroy();
    }
}
