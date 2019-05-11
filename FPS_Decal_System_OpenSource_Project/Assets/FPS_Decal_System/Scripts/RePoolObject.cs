using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RePoolObject : MonoBehaviour
{
    [SerializeField]
    private bool repoolAfterTime = true;

    [SerializeField]
    private float repoolTime = 5.0f;

    private void OnEnable()
    {
        BeginDestroy();
    }

    private void BeginDestroy()
    {
        if (repoolAfterTime)
        {
            Invoke("Repool", repoolTime);
        }
    }

    public void Repool()
    {
        CancelInvoke();
        gameObject.SetActive(false);
        ObjectPooler.instance.SetParentObject(gameObject, ObjectPooler.instance.gameObject);
        ObjectPooler.instance.DeactivateObject(gameObject);
    }

    public void SetState(bool _destroyAfterTime, float _destroyTime)
    {
        repoolAfterTime = _destroyAfterTime;
        repoolTime = _destroyTime;
    }

}
