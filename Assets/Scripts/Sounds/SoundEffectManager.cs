using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
public class SoundEffectManager : SingletonMonobehaviour<SoundEffectManager>
{
    public int soundsVolume = 20;

    private void Start()
    {
        if (PlayerPrefs.HasKey("SoundsVolume"))
        {
            soundsVolume = PlayerPrefs.GetInt("SoundsVolume");
        }

        SetSoundsVolume(soundsVolume);
    }

    private void OnDisable()
    {
        PlayerPrefs.SetInt("SoundsVolume", soundsVolume);
    }

    /// <summary>
    /// Changes the sound volume to the specified new volume.
    /// </summary>
    /// <param name="newVolume">The new volume level, clamped between 0 and 100.</param>
    public void ChangeSoundVolume(int newVolume)
    {
        soundsVolume = Mathf.Clamp(newVolume, 0, 100);
        SetSoundsVolume(soundsVolume);
    }

    /// <summary>
    /// Plays the specified sound effect.
    /// </summary>
    /// <param name="soundEffect">The sound effect to play.</param>
    public void PlaySoundEffect(SoundEffectSO soundEffect)
    {
        SoundEffect sound = (SoundEffect)PoolManager.Instance.ReuseComponent(soundEffect.soundPrefab, Vector3.zero, Quaternion.identity);
        sound.SetSound(soundEffect);
        sound.gameObject.SetActive(true);
        StartCoroutine(DisableSound(sound, soundEffect.soundEffectClip.length));
    }

    /// <summary>
    /// Disables the sound after the specified duration.
    /// </summary>
    /// <param name="sound">The sound effect to disable.</param>
    /// <param name="soundDuration">The duration of the sound effect.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator DisableSound(SoundEffect sound, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        sound.gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the sound volume in the audio mixer.
    /// </summary>
    /// <param name="soundsVolume">The volume level to set.</param>
    private void SetSoundsVolume(int soundsVolume)
    {
        float muteDecibels = -80f;
        if (soundsVolume == 0)
        {
            GameResources.Instance.soundsMasterMixerGroup.audioMixer.SetFloat("soundsVolume",
                muteDecibels);
        }
        else
        {
            GameResources.Instance.soundsMasterMixerGroup.audioMixer.SetFloat("soundsVolume",
                HelperUtilities.LinearToDecibels(soundsVolume));
        }
    }
}
