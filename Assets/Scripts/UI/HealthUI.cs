using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    private List<GameObject> heartList = new List<GameObject>();

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        SetPlayerHealthBar(healthEventArgs);
    }

    private void ClearHealthList()
    {
        foreach (GameObject heart in heartList)
        {
            Destroy(heart);
            //Debug.Log("Destroying heart");
        }

        heartList.Clear();
    }

    private void SetPlayerHealthBar(HealthEventArgs healthEventArgs)
    {
        ClearHealthList();

        for (int i = 0; i < healthEventArgs.healthAmount; i++)
        {
            GameObject heart = Instantiate(GameResources.Instance.heartPrefab, transform);
            heart.GetComponent<RectTransform>().anchoredPosition = new Vector2(Settings.uiHeartIconSpacing * i, 0);

            heartList.Add(heart);
        }

    }
}
