using UnityEngine;

public class AnimationSoundController : MonoBehaviour
{
    public AudioClip[] soundPool; // Array to store your sound clips
    private AudioSource audioSource;
    private int lastSoundIndex = -1; // Initialize to -1 to ensure the first sound played is not the same as the last one

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayRandomSound()
    {
        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, soundPool.Length);
        } while (randomIndex == lastSoundIndex); // Ensure the new sound is different from the last one

        audioSource.clip = soundPool[randomIndex];
        audioSource.Play();

        lastSoundIndex = randomIndex; // Update the last played sound index
    }
}
