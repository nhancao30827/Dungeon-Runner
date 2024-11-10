using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ChangePlayerName : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInputField;

    private void Start()
    {
        // Ensure the input field is assigned
        if (playerNameInputField == null)
        {
            playerNameInputField = GameObject.Find("PlayerInputField").GetComponent<TMP_InputField>();
        }

        // Add listener to the input field's OnEndEdit event
        playerNameInputField.onEndEdit.AddListener(UpdatePlayerName);
    }

    public void UpdatePlayerName(string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            newName = "Player";
        }
        else if (newName.Length > 12)
        {
            newName = newName.Substring(0, 12); // Truncate to 12 characters
        }

        GameResources.Instance.currentPlayerSO.playerName = newName;
        Debug.Log("Player name: " + GameResources.Instance.currentPlayerSO.playerName);
    }
}
