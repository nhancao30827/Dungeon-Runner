using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MenuManager : MonoBehaviour
{
    /// <summary>
    /// Loads the scene with the specified index.
    /// </summary>
    /// <param name="sceneIndex">The index of the scene to load.</param>
    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitApp()
    {
        Application.Quit();
    }
}
