using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Slider to adjust the sound effect volume.")]
    #endregion
    [SerializeField] private Slider soundEffectSlider;

    #region Tooltip
    [Tooltip("Text to display the current sound effect volume.")]
    #endregion
    [SerializeField] private TextMeshProUGUI volumeText;

    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    private void Start()
    {
        this.gameObject.SetActive(false);

        soundEffectSlider.minValue = 0;
        soundEffectSlider.maxValue = 100;

        soundEffectSlider.value = PlayerPrefs.GetInt("SoundsVolume");

        soundEffectSlider.onValueChanged.AddListener(OnSoundEffectSliderValueChanged);
    }

    private void Update()
    {
        DisplayVolume();
    }


    ///<summary>
    /// Toggles the visibility of the pause menu.
    /// </summary>
    public void TogglePauseMenu()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }

    /// <summary>
    /// Handles the event when the sound effect slider value is changed.
    /// </summary>
    /// <param name="value">The new value of the sound effect slider.</param>
    private void OnSoundEffectSliderValueChanged(float value)
    {
        SoundEffectManager.Instance.ChangeSoundVolume((int)value);
        Debug.Log($"Slider value changed to: {value}");
    }

    /// <summary>
    /// Displays the current sound effect volume on the UI.
    /// </summary>
    private void DisplayVolume()
    {
        int soundVolume = (int)soundEffectSlider.value;
        volumeText.text = "SFX Volume " + soundVolume;
    }
}
