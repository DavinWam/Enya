using UnityEngine;

public class playMusicScript : MonoBehaviour
{
    public AudioClip engineRunning;
    public SoundManager soundManager;
    private AudioSource audioSource;
    private int overlappingAreas = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = engineRunning;
        audioSource.loop = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            overlappingAreas++;
            if (overlappingAreas == 1 || soundManager.currentAreaAudioSource.clip != audioSource.clip)
            {
                StartCoroutine(soundManager.CrossfadeMusic(soundManager.currentAreaAudioSource, audioSource));
            }
        }
    }
}
