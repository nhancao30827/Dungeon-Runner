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

    public void SetSound(SoundEffectSO soundEffect){
        audioSource.pitch = Random.Range(soundEffect.minPitchVariation, // Randomize the pitch for a more natural sound
            soundEffect.maxPitchVariation);
        audioSource.volume = soundEffect.soundEffectVolume;
        audioSource.clip = soundEffect.soundEffectClip;
    }

}
