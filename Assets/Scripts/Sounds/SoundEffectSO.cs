using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SoundEffect_", menuName = "Scriptable Objects/Sounds/SoundEffect")]
public class SoundEffectSO : ScriptableObject
{
    #region Header SOUND EFFECT DETAILS
    [Space(10)]
    [Header("SOUND EFFECT DETAILS")]
    #endregion

    #region Tooltip
    [Tooltip("Name of the sound effect")]
    #endregion
    public string soundEffectName;

    #region Tooltip
    [Tooltip("Prefab for the sound effect")]
    #endregion
    public GameObject soundPrefab;

    #region Tooltip
    [Tooltip("Audio clip for the sound effect")]
    #endregion
    public AudioClip soundEffectClip;

    #region Tooltip
    [Tooltip("Minimum pitch variation for the sound effect")]
    #endregion
    [Range(0.1f, 1.5f)]
    public float minPitchVariation = 0.8f;

    #region Tooltip
    [Tooltip("Maximum pitch variation for the sound effect")]
    #endregion
    [Range(0.1f, 1.5f)]
    public float maxPitchVariation = 1.2f;

    #region Tool tip
    [Tooltip("Volume of the sound effect")]
    #endregion
    [Range(0f, 1f)]
    public float soundEffectVolume = 1f;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(soundEffectName), soundEffectName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundPrefab), soundPrefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundEffectClip), soundEffectClip);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(soundEffectVolume), soundEffectVolume, true);
    }
#endif
    #endregion
}
