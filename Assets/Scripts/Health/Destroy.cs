using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    private DestroyEvent destroyEvent;

    private void Awake()
    {
        destroyEvent = GetComponent<DestroyEvent>();
    }
    
    private void OnEnable()
    {
        destroyEvent.OnDestroy += DestroyEvent_OnDestroy;
    }

    private void OnDisable()
    {
        destroyEvent.OnDestroy -= DestroyEvent_OnDestroy;
    }

    private void DestroyEvent_OnDestroy(DestroyEvent destroyEvent, DestroyEventArgs destroyEventArgs)
    {
        Debug.Log(destroyEventArgs.isPlayerDead);
        if (destroyEventArgs.isPlayerDead)
        {
            Debug.Log("MADAFAKA");
            gameObject.SetActive(false);

        }

        else
        {
            Destroy(gameObject);
        }
    }

}
