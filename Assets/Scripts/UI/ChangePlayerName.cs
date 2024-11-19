using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles the player name change functionality.
/// </summary>
public class ChangePlayerName : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInputField;

    private void Start()
    {
        InitializePlayerNameInputField();
        playerNameInputField.onEndEdit.AddListener(UpdatePlayerName);
    }

    /// <summary>
    /// Initializes the player name input field.
    /// </summary>
    private void InitializePlayerNameInputField()
    {
        if (playerNameInputField == null)
        {
            playerNameInputField = GameObject.Find("PlayerInputField").GetComponent<TMP_InputField>();
        }
    }

    /// <summary>
    /// Updates the player name with the new name provided.
    /// </summary>
    /// <param name="newName">The new name for the player.</param>
    public void UpdatePlayerName(string newName)
    {
        newName = ValidatePlayerName(newName);
        GameResources.Instance.currentPlayerSO.playerName = newName;
        Debug.Log("Player name: " + GameResources.Instance.currentPlayerSO.playerName);
    }

    /// <summary>
    /// Validates the player name, ensuring it is not empty and does not exceed the maximum length.
    /// </summary>
    /// <param name="newName">The new name for the player.</param>
    /// <returns>The validated player name.</returns>
    private string ValidatePlayerName(string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            return "Player";
        }
        else if (newName.Length > 12)
        {
            return newName.Substring(0, 12);
        }
        return newName;
    }
}
