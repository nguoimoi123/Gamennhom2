using UnityEngine;
using UnityEngine.UI;
using TMPro; // Thêm để sử dụng TextMeshPro
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsCanvas; // Canvas chứa menu Settings

    // Tabs bên trái
    [SerializeField] private Button graphicsTabButton;
    [SerializeField] private Button audioTabButton;
    [SerializeField] private Button videoTabButton;

    // Nội dung bên phải
    [SerializeField] private TMP_Text titleText; // Tiêu đề
    [SerializeField] private GameObject contentPanel; // Panel chứa nội dung
    [SerializeField] private GameObject videoContent; // Nội dung Video
    [SerializeField] private GameObject graphicContent; // Nội dung Graphics
    [SerializeField] private GameObject audioContent; // Nội dung Audio

    // Graphics
    [SerializeField] private TMP_Text screenResolutionText;
    [SerializeField] private TMP_Dropdown resolutionDropdown; // Dropdown cho độ phân giải
    [SerializeField] private TMP_Text languageText;
    [SerializeField] private TMP_Dropdown languageDropdown; // Dropdown cho ngôn ngữ
    [SerializeField] private Toggle highPerformanceToggle;
    [SerializeField] private Toggle mediumPerformanceToggle;
    [SerializeField] private Toggle lowPerformanceToggle;

    // Video
    [SerializeField] private Toggle highQualityShadersToggle;
    [SerializeField] private Slider motionBlurSlider;
    [SerializeField] private TMP_Text motionBlurValueText;
    [SerializeField] private Toggle highRenderQualityToggle;
    [SerializeField] private Toggle mediumRenderQualityToggle;
    [SerializeField] private Toggle lowRenderQualityToggle;

    // Audio
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TMP_Text masterVolumeValueText;
    [SerializeField] private Slider soundSlider;
    [SerializeField] private TMP_Text soundValueText;
    [SerializeField] private Toggle muteAllYesToggle;
    [SerializeField] private Toggle muteAllNoToggle;

    // Buttons
    [SerializeField] private Button resetButton;
    [SerializeField] private Button saveButton;

    private void Awake()
    {
        // Giữ SettingsManager không bị hủy khi chuyển scene
        DontDestroyOnLoad(gameObject);

        // Đảm bảo chỉ có một instance của SettingsManager (tùy chọn, nếu muốn Singleton)
        SettingsManager[] managers = FindObjectsByType<SettingsManager>(FindObjectsSortMode.None);
        if (managers.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Ẩn Settings Canvas khi khởi động
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(false);
        }

        // Gắn sự kiện cho nút Settings (giả định có settingsButton, thêm nếu cần)
        // Nếu không có settingsButton, bạn cần thêm [SerializeField] private Button settingsButton;

        // Gắn sự kiện cho các tab
        if (graphicsTabButton != null) graphicsTabButton.onClick.AddListener(() => ShowTab("Graphic", graphicContent));
        if (audioTabButton != null) audioTabButton.onClick.AddListener(() => ShowTab("Audio", audioContent));
        if (videoTabButton != null) videoTabButton.onClick.AddListener(() => ShowTab("Video", videoContent));

        // Gắn sự kiện cho các điều khiển
        if (highQualityShadersToggle != null) highQualityShadersToggle.onValueChanged.AddListener(UpdateHighQualityShaders);
        if (motionBlurSlider != null) motionBlurSlider.onValueChanged.AddListener(UpdateMotionBlur);
        if (motionBlurValueText != null) motionBlurSlider.onValueChanged.AddListener(value => motionBlurValueText.text = $"{(int)value}/100");
        if (highRenderQualityToggle != null) highRenderQualityToggle.onValueChanged.AddListener(value => UpdateRenderQuality("High", value));
        if (mediumRenderQualityToggle != null) mediumRenderQualityToggle.onValueChanged.AddListener(value => UpdateRenderQuality("Medium", value));
        if (lowRenderQualityToggle != null) lowRenderQualityToggle.onValueChanged.AddListener(value => UpdateRenderQuality("Low", value));
        if (highPerformanceToggle != null) highPerformanceToggle.onValueChanged.AddListener(value => UpdatePerformance("High", value));
        if (mediumPerformanceToggle != null) mediumPerformanceToggle.onValueChanged.AddListener(value => UpdatePerformance("Medium", value));
        if (lowPerformanceToggle != null) lowPerformanceToggle.onValueChanged.AddListener(value => UpdatePerformance("Low", value));
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.AddListener(UpdateResolution);
        if (languageDropdown != null) languageDropdown.onValueChanged.AddListener(UpdateLanguage);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(UpdateMasterVolume);
        if (masterVolumeValueText != null) masterVolumeSlider.onValueChanged.AddListener(value => masterVolumeValueText.text = $"{(int)value}/100");
        if (soundSlider != null) soundSlider.onValueChanged.AddListener(UpdateSoundVolume);
        if (soundValueText != null) soundSlider.onValueChanged.AddListener(value => soundValueText.text = $"{(int)value}/100");
        if (muteAllYesToggle != null) muteAllYesToggle.onValueChanged.AddListener(value => UpdateMuteAll(value, muteAllNoToggle));
        if (muteAllNoToggle != null) muteAllNoToggle.onValueChanged.AddListener(value => UpdateMuteAll(value, muteAllYesToggle));
        if (resetButton != null) resetButton.onClick.AddListener(ResetSettings);
        if (saveButton != null) saveButton.onClick.AddListener(SaveSettings);

        // Tải cài đặt đã lưu
        LoadSettings();
    }

    public void OpenSettings()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(true);
            ShowTab("Video", videoContent); // Mở tab Video mặc định (có thể thay bằng Graphic hoặc Audio)
        }
    }

    public void CloseSettings()
    {
        if (settingsCanvas != null)
        {
            settingsCanvas.gameObject.SetActive(false);
        }
    }

    private void ShowTab(string title, GameObject content)
    {
        if (titleText != null) titleText.text = title.ToUpper();

        videoContent.SetActive(false);
        graphicContent.SetActive(false);
        audioContent.SetActive(false);

        if (content != null) content.SetActive(true);
    }

    // Graphics
    private void UpdatePerformance(string quality, bool value)
    {
        if (value)
        {
            highPerformanceToggle.isOn = quality == "High";
            mediumPerformanceToggle.isOn = quality == "Medium";
            lowPerformanceToggle.isOn = quality == "Low";
            PlayerPrefs.SetString("Performance", quality);
        }
    }

    private void UpdateVibration(float value)
    {
        PlayerPrefs.SetFloat("Vibration", value);
    }

    private void UpdateResolution(int index)
    {
        string[] resolutions = { "1280x720", "1920x1080", "800x600" };
        PlayerPrefs.SetString("Resolution", resolutions[index]);
        resolutionDropdown.captionText.text = resolutions[index];
    }

    private void UpdateLanguage(int index)
    {
        string[] languages = { "ENGLISH", "SPANISH", "FRENCH" };
        PlayerPrefs.SetString("Language", languages[index]);
        languageDropdown.captionText.text = languages[index];
    }

    // Video
    private void UpdateHighQualityShaders(bool value)
    {
        PlayerPrefs.SetInt("HighQualityShaders", value ? 1 : 0);
    }

    private void UpdateMotionBlur(float value)
    {
        PlayerPrefs.SetFloat("MotionBlur", value);
        if (motionBlurValueText != null) motionBlurValueText.text = $"{(int)value}/100";
    }

    private void UpdateRenderQuality(string quality, bool value)
    {
        if (value)
        {
            highRenderQualityToggle.isOn = quality == "High";
            mediumRenderQualityToggle.isOn = quality == "Medium";
            lowRenderQualityToggle.isOn = quality == "Low";
            PlayerPrefs.SetString("RenderQuality", quality);
        }
    }

    // Audio
    private void UpdateMasterVolume(float value)
    {
        if (audioMixer != null && masterVolumeValueText != null)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(value) * 20);
            PlayerPrefs.SetFloat("MasterVolume", value);
            masterVolumeValueText.text = $"{(int)value}/100";

            // Cập nhật volume cho AudioManager
            AudioManager audioManager = FindFirstObjectByType<AudioManager>();
            if (audioManager != null)
            {
                audioManager.UpdateVolume(value);
            }
        }
    }

    private void UpdateSoundVolume(float value)
    {
        audioMixer.SetFloat("SoundVolume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("SoundVolume", value);
    }

    private void UpdateMuteAll(bool value, Toggle otherToggle)
    {
        if (value)
        {
            PlayerPrefs.SetInt("MuteAll", value ? 1 : 0);
            otherToggle.isOn = !value; // Tắt toggle còn lại
            audioMixer.SetFloat("MasterVolume", value ? -80f : Mathf.Log10(PlayerPrefs.GetFloat("MasterVolume", 0.28f)) * 20);
        }
    }

    private void ResetSettings()
    {
        // Đặt lại giá trị mặc định
        if (motionBlurSlider != null) motionBlurSlider.value = 54;
        if (highQualityShadersToggle != null) highQualityShadersToggle.isOn = true;
        if (highPerformanceToggle != null) highPerformanceToggle.isOn = true;
        if (highRenderQualityToggle != null) highRenderQualityToggle.isOn = true;
        if (mediumPerformanceToggle != null) mediumPerformanceToggle.isOn = false;
        if (lowPerformanceToggle != null) lowPerformanceToggle.isOn = false;
        if (mediumRenderQualityToggle != null) mediumRenderQualityToggle.isOn = false;
        if (lowRenderQualityToggle != null) lowRenderQualityToggle.isOn = false;
        if (resolutionDropdown != null) resolutionDropdown.value = 0; // Mặc định 1280x720
        if (languageDropdown != null) languageDropdown.value = 0; // Mặc định ENGLISH
        if (masterVolumeSlider != null) masterVolumeSlider.value = 28;
        if (soundSlider != null) soundSlider.value = 85;
        if (muteAllYesToggle != null) muteAllYesToggle.isOn = true;
        if (muteAllNoToggle != null) muteAllNoToggle.isOn = false;
        LoadSettings(); // Áp dụng lại mặc định
    }

    private void SaveSettings()
    {
        PlayerPrefs.Save();
        CloseSettings();
    }

    public void LoadSettings()
    {
        string performance = PlayerPrefs.GetString("Performance", "High");
        if (highPerformanceToggle != null && mediumPerformanceToggle != null && lowPerformanceToggle != null)
        {
            highPerformanceToggle.isOn = performance == "High";
            mediumPerformanceToggle.isOn = performance == "Medium";
            lowPerformanceToggle.isOn = performance == "Low";
            UpdatePerformance(performance, true);
        }
        string resolution = PlayerPrefs.GetString("Resolution", "1280x720");
        string[] resolutions = { "1280x720", "1920x1080", "800x600" };
        if (resolutionDropdown != null)
        {
            resolutionDropdown.value = System.Array.IndexOf(resolutions, resolution);
            UpdateResolution(resolutionDropdown.value);
        }
        string language = PlayerPrefs.GetString("Language", "ENGLISH");
        string[] languages = { "ENGLISH", "SPANISH", "FRENCH" };
        if (languageDropdown != null)
        {
            languageDropdown.value = System.Array.IndexOf(languages, language);
            UpdateLanguage(languageDropdown.value);
        }

        // Video
        if (highQualityShadersToggle != null)
        {
            bool highQuality = PlayerPrefs.GetInt("HighQualityShaders", 1) == 1;
            highQualityShadersToggle.isOn = highQuality;
            UpdateHighQualityShaders(highQuality);
        }
        if (motionBlurSlider != null)
        {
            float motionBlur = PlayerPrefs.GetFloat("MotionBlur", 54);
            motionBlurSlider.value = motionBlur;
            UpdateMotionBlur(motionBlur);
        }
        string renderQuality = PlayerPrefs.GetString("RenderQuality", "High");
        if (highRenderQualityToggle != null && mediumRenderQualityToggle != null && lowRenderQualityToggle != null)
        {
            highRenderQualityToggle.isOn = renderQuality == "High";
            mediumRenderQualityToggle.isOn = renderQuality == "Medium";
            lowRenderQualityToggle.isOn = renderQuality == "Low";
            UpdateRenderQuality(renderQuality, true);
        }

        // Audio
        if (masterVolumeSlider != null)
        {
            float masterVolume = PlayerPrefs.GetFloat("MasterVolume", 28);
            masterVolumeSlider.value = masterVolume;
            UpdateMasterVolume(masterVolume);
        }
        if (soundSlider != null)
        {
            float soundVolume = PlayerPrefs.GetFloat("SoundVolume", 85);
            soundSlider.value = soundVolume;
            UpdateSoundVolume(soundVolume);
        }
        bool muteAll = PlayerPrefs.GetInt("MuteAll", 1) == 1;
        if (muteAllYesToggle != null) muteAllYesToggle.isOn = muteAll;
        if (muteAllNoToggle != null) muteAllNoToggle.isOn = !muteAll;
        UpdateMuteAll(muteAll, muteAllNoToggle);
    }
}