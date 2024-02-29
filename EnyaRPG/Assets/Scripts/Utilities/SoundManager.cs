using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Manager/SoundManager")]
public class SoundManager : ScriptableObject
{
    public AudioSource currentAreaAudioSource;
    public float crossfadeDuration = 0.5f; // Duration of the crossfade


    public float GetVolume()
    {
        return PlayerPrefs.GetFloat("Volume", 0.5f); // Default to 0.5 if not set
    }

    public IEnumerator CrossfadeMusic(AudioSource from, AudioSource to)
    {
        float timer = 0;

        // Starting volumes
        float fromInitialVolume = (from != null) ? from.volume : 0;
        float toInitialVolume = (to != null) ? to.volume : 0;

        // If starting a new track, begin playing it quietly
        if (to != null && !to.isPlaying)
        {
            to.Play();
            to.volume = 0;
        }

        // Crossfade loop
        while (timer < crossfadeDuration)
        {
            timer += Time.deltaTime;

            if (from != null)
            {
                from.volume = Mathf.Lerp(fromInitialVolume, 0, timer / crossfadeDuration);
            }

            if (to != null)
            {
                to.volume = Mathf.Lerp(toInitialVolume, GetVolume(), timer / crossfadeDuration);
            }

            yield return null;
        }

        // Stop the audio source that has been faded out
        if (from != null)
        {
            from.Stop();
        }

        // Update the current area audio source
        currentAreaAudioSource = to;
    }
}
