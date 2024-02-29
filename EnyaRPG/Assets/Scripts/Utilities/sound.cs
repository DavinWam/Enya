using UnityEngine;
using UnityEngine.UI;

public class sound : MonoBehaviour
{
    [SerializeField] Slider volume;
    void Start()
    {
        if (!PlayerPrefs.HasKey("music"))
        {
            PlayerPrefs.SetFloat("music", 1);
            load();
        }
        else
        {
            load();
        }
    }
    public void changeVolume()
    {
        AudioListener.volume = volume.value;
        save();
    }
    private void load()
    {
        volume.value = PlayerPrefs.GetFloat("music");
    }
    private void save()
    {
        PlayerPrefs.SetFloat("music", volume.value);
    }
}
