using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class OptionsPanel : MonoBehaviour
{
    public Slider volumeSlider;
    public Toggle vsyncToggle;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown resolutionDropdown;


    private List<Resolution> resolutions;
    private Animator rpgPanelAnimator;
    public bool isPanelOpen = false;
    void Start()
    {
        // Load volume or set default to 1 if it doesn't exist
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1.0f);
        AudioListener.volume = volumeSlider.value;

        // Load quality level or default to medium (1) if it doesn't exist
        int qualityLevel = PlayerPrefs.GetInt("QualityLevel", 1);
        qualityDropdown.value = qualityLevel;
        QualitySettings.SetQualityLevel(qualityLevel);

        // Load VSync setting
        int vsyncEnabled = PlayerPrefs.GetInt("VSyncEnabled", 0);
        vsyncToggle.isOn = vsyncEnabled == 1;
        QualitySettings.vSyncCount = vsyncEnabled;
        vsyncToggle.GetComponentInChildren<TextMeshProUGUI>().text = vsyncToggle.isOn.ToString();

        // Load resolution or set to current resolution as default
        int defaultWidth = Screen.currentResolution.width;
        int defaultHeight = Screen.currentResolution.height;
        int defaultRefreshRate = Screen.currentResolution.refreshRate;
        int resWidth = PlayerPrefs.GetInt("ResolutionWidth", defaultWidth);
        int resHeight = PlayerPrefs.GetInt("ResolutionHeight", defaultHeight);
        int resRefreshRate = PlayerPrefs.GetInt("ResolutionRefreshRate", defaultRefreshRate);
        Screen.SetResolution(resWidth, resHeight, Screen.fullScreen);

        InitializeResolutions();
    }


  public void Toggle()
    {
        RPGPanel rPGPanel = FindObjectOfType<RPGPanel>();
        rpgPanelAnimator = rPGPanel.GetComponent<Animator>();
        if (isPanelOpen)
        {
            // Close the Options panel
            isPanelOpen = false;
            rpgPanelAnimator.SetBool("inOptions", false);

            // Additional logic for when the Options panel closes
            // e.g., saving settings, hiding submenus, etc.

            // If there's a need to wait for the animation to complete before performing some actions,
            // consider using a coroutine like in your RPGPanel example
        }
        else
        {
            // Open the Options panel
            isPanelOpen = true;
            rpgPanelAnimator.SetBool("inOptions", true);

            // Additional logic for when the Options panel opens
            // Load volume or set default to 1 if it doesn't exist
            volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1.0f);
            AudioListener.volume = volumeSlider.value;

            // Load quality level or default to medium (1) if it doesn't exist
            int qualityLevel = PlayerPrefs.GetInt("QualityLevel", 1);
            qualityDropdown.value = qualityLevel;
            QualitySettings.SetQualityLevel(qualityLevel);

            // Load VSync setting
            int vsyncEnabled = PlayerPrefs.GetInt("VSyncEnabled", 0);
            vsyncToggle.isOn = vsyncEnabled == 1;
            QualitySettings.vSyncCount = vsyncEnabled;
            vsyncToggle.GetComponentInChildren<TextMeshProUGUI>().text = vsyncToggle.isOn.ToString();

            // Load resolution or set to current resolution as default
            int defaultWidth = Screen.currentResolution.width;
            int defaultHeight = Screen.currentResolution.height;
            int defaultRefreshRate = Screen.currentResolution.refreshRate;
            int resWidth = PlayerPrefs.GetInt("ResolutionWidth", defaultWidth);
            int resHeight = PlayerPrefs.GetInt("ResolutionHeight", defaultHeight);
            int resRefreshRate = PlayerPrefs.GetInt("ResolutionRefreshRate", defaultRefreshRate);
            Screen.SetResolution(resWidth, resHeight, Screen.fullScreen);

            InitializeResolutions();
            // Initialize or update the Options panel
            // This could involve setting up the panel with current game settings
        }
    }
    private void InitializeResolutions()
    {
        resolutions = new List<Resolution>();
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();

        // Define desired resolutions
        int[,] desiredResolutions = new int[,] {
            { 960, 540 },
            { 1280, 720 },
            { 1366, 768 },
            { 1600, 900 },
            { 1920, 1080 },
            { 2560, 1440 },
            { 3200, 1800 },
            { 3840, 2160 }
        };

        int currentResolutionIndex = 0;

        for (int i = 0; i < desiredResolutions.GetLength(0); i++)
        {
            int width = desiredResolutions[i, 0];
            int height = desiredResolutions[i, 1];

      //      if (IsResolutionAvailable(width, height))
          //  {
                string option = width + " X " + height;
                options.Add(option);

                if (width == Screen.currentResolution.width && height == Screen.currentResolution.height)
                {
                    currentResolutionIndex = options.Count - 1;
                }

                resolutions.Add(new Resolution { width = width, height = height });
          //  }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
        resolutionDropdown.onValueChanged.AddListener(delegate { SetResolution(); });
    }

    private bool IsResolutionAvailable(int width, int height)
    {
        foreach (Resolution res in Screen.resolutions)
        {
            if (res.width == width && res.height == height)
                return true;
        }
        return false;
    }


    public void SetVolume()
    {
        float volume = volumeSlider.value;
        AudioListener.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume); // Save to PlayerPrefs
    }

    public void SetQuality()
    {
        int qualityLevel = qualityDropdown.value;
        QualitySettings.SetQualityLevel(qualityLevel);
        PlayerPrefs.SetInt("QualityLevel", qualityLevel); // Save to PlayerPrefs
    }

    public void SetVSync()
    {
        bool vsyncEnabled = vsyncToggle.isOn;
        QualitySettings.vSyncCount = vsyncEnabled ? 1 : 0;
        PlayerPrefs.SetInt("VSyncEnabled", vsyncEnabled ? 1 : 0); // Save to PlayerPrefs
        vsyncToggle.GetComponentInChildren<TextMeshProUGUI>().text = vsyncToggle.isOn.ToString();
    }

    public void SetResolution()
    {
        Resolution res = resolutions[resolutionDropdown.value];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionWidth", res.width);
        PlayerPrefs.SetInt("ResolutionHeight", res.height);
        PlayerPrefs.SetFloat("ResolutionRefreshRate", res.refreshRate);
    }
    public GameObject confirmationPanel; // Reference to the confirmation panel

    // Call this method when the quit button is clicked
    public void ConfirmQuit()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Quit in the Unity Editor
        #endif

    }

    // Call this method from the "Yes" button on the confirmation panel
    public void Quit()
    {
        confirmationPanel.SetActive(true); // Show the confirmation panel
    }

    // Call this method from the "No" button on the confirmation panel
    public void CancelQuit()
    {
        confirmationPanel.SetActive(false); // Hide the confirmation panel
    }

}
