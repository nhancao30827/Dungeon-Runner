using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[DisallowMultipleComponent]
public class SoundEffect : MonoBehaviour
{
    private AudioSource audioSource;

    private void Awake(){
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable(){
        if (audioSource.clip != null){
            audioSource.Play();
        }
    }

    private void OnDisable(){
        audioSource.Stop();
    }

    /// <summary>
    /// Sets the sound effect properties for the audio source.
    /// </summary>
    /// <param name="soundEffect">The sound effect scriptable object containing the sound properties.</param>
    public void SetSound(SoundEffectSO soundEffect)
    {
        audioSource.pitch = Random.Range(soundEffect.minPitchVariation, soundEffect.maxPitchVariation);
        audioSource.volume = soundEffect.soundEffectVolume;
        audioSource.clip = soundEffect.soundEffectClip;
    }

}
