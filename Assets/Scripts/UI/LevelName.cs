using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelName : MonoBehaviour
{
    public TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
       
    }

    private void Update()
    {
        UpdateLevelName();
    }

    private void UpdateLevelName()
    {
        int levelIndex = GameManager.Instance.CurrentDungeonLevelListIndex + 1;
        text.text = "Level " + levelIndex.ToString();
    }
}
