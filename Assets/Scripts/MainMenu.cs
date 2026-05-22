using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Нужно для работы с текстом

public class MainMenu : MonoBehaviour
{
    [Header("Интерфейс (UI)")]
    public GameObject continueButton;
    public TextMeshProUGUI continueButtonText;
    public GameObject levelSelectButton; // Кнопка выбора уровней (размещена в редакторе)

    [Header("Звуки")]
    public AudioSource uiAudioSource;
    public AudioClip buttonClickSound;

    [Header("Настройки")]
    public GameObject settingsPanel;
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfxSlider;


    void Start()
    {
        // ОЧЕНЬ ВАЖНО: При загрузке меню сбрасываем время и возвращаем курсор.
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Проверяем прогресс игрока
        int currentShift = PlayerPrefs.GetInt("CurrentShift", 0);
        int unlockedShift = PlayerPrefs.GetInt("UnlockedShift", 0);
        
        // Синхронизируем анлок, если текущая смена выше разблокированной
        if (currentShift > unlockedShift) 
        {
            unlockedShift = currentShift;
            PlayerPrefs.SetInt("UnlockedShift", unlockedShift);
            PlayerPrefs.Save();
        }
        
        if (currentShift > 0)
        {
            // Если есть сохранения, показываем кнопку "Продолжить" с номером смены
            if (continueButton != null) continueButton.SetActive(true);
            if (continueButtonText != null) continueButtonText.text = "ПРОДОЛЖИТЬ СМЕНУ (" + (currentShift + 1) + ")";
        }
        else
        {
            // Если игрок только скачал игру, скрываем "Продолжить"
            if (continueButton != null) continueButton.SetActive(false);
        }

        // Показываем кнопку "ВЫБОР СМЕНЫ", если пройдена хотя бы 1 смена
        if (levelSelectButton != null)
        {
            levelSelectButton.SetActive(unlockedShift > 0);
        }

        // Инициализируем громкость звуков UI
        if (uiAudioSource != null)
        {
            uiAudioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        }
    }

    public void ContinueGame()
    {
        if (uiAudioSource != null && buttonClickSound != null) uiAudioSource.PlayOneShot(buttonClickSound);
        StartCoroutine(LoadSceneWithDelay("GameScene"));
    }

    public void NewGame()
    {
        PlayerPrefs.SetInt("CurrentShift", 0);
        PlayerPrefs.Save();
        
        if (uiAudioSource != null && buttonClickSound != null) uiAudioSource.PlayOneShot(buttonClickSound);
        StartCoroutine(LoadSceneWithDelay("GameScene"));
    }

    public void QuitGame()
    {
        Debug.Log("Выход из игры...");
        if (uiAudioSource != null && buttonClickSound != null) uiAudioSource.PlayOneShot(buttonClickSound);
        StartCoroutine(QuitWithDelay());
    }

    private System.Collections.IEnumerator LoadSceneWithDelay(string sceneName)
    {
        // Небольшая пауза, чтобы звук успел проиграться до того, как сцена удалится
        yield return new WaitForSeconds(0.4f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
    
    private System.Collections.IEnumerator QuitWithDelay()
    {
        yield return new WaitForSeconds(0.4f);
        Application.Quit();
    }

    // ==========================================
    // ЛОГИКА ВЫБОРА УРОВНЕЙ (Генерируется кодом)
    // ==========================================
    private GameObject levelsPanel;

    public void ShowLevelsPanel()
    {
        if (uiAudioSource != null && buttonClickSound != null) uiAudioSource.PlayOneShot(buttonClickSound);
        
        int unlockedShift = PlayerPrefs.GetInt("UnlockedShift", 0);

        if (levelsPanel == null)
        {
            CreateLevelsUIRuntime(unlockedShift);
        }
        else
        {
            levelsPanel.SetActive(true);
        }
    }

    public void HideLevelsPanel()
    {
        if (uiAudioSource != null && buttonClickSound != null) uiAudioSource.PlayOneShot(buttonClickSound);
        if (levelsPanel != null) levelsPanel.SetActive(false);
    }

    public void LoadSpecificShift(int shiftIndex)
    {
        if (uiAudioSource != null && buttonClickSound != null) uiAudioSource.PlayOneShot(buttonClickSound);
        PlayerPrefs.SetInt("CurrentShift", shiftIndex);
        PlayerPrefs.Save();
        StartCoroutine(LoadSceneWithDelay("GameScene"));
    }

    // Удален метод CreateLevelSelectButtonRuntime(), так как кнопка теперь в редакторе

    private void CreateLevelsUIRuntime(int maxUnlocked)
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        levelsPanel = new GameObject("RuntimeLevelsPanel");
        levelsPanel.transform.SetParent(canvas.transform, false);
        levelsPanel.transform.SetAsLastSibling();
        
        UnityEngine.UI.Image bg = levelsPanel.AddComponent<UnityEngine.UI.Image>();
        bg.color = new Color(0, 0, 0, 0.95f);
        bg.raycastTarget = true;

        RectTransform rt = levelsPanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Заголовок
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(levelsPanel.transform, false);
        TextMeshProUGUI title = titleObj.AddComponent<TextMeshProUGUI>();
        title.text = "ВЫБЕРИТЕ СМЕНУ";
        title.fontSize = 50;
        title.color = Color.white;
        title.alignment = TextAlignmentOptions.Center;
        RectTransform titleRt = titleObj.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 0.85f); titleRt.anchorMax = new Vector2(1, 1);
        titleRt.offsetMin = Vector2.zero; titleRt.offsetMax = Vector2.zero;

        // Кнопка Назад
        GameObject closeObj = new GameObject("CloseBtn");
        closeObj.transform.SetParent(levelsPanel.transform, false);
        UnityEngine.UI.Image closeImg = closeObj.AddComponent<UnityEngine.UI.Image>();
        closeImg.color = new Color(0.6f, 0.1f, 0.1f);
        UnityEngine.UI.Button closeBtn = closeObj.AddComponent<UnityEngine.UI.Button>();
        closeBtn.onClick.AddListener(HideLevelsPanel);
        RectTransform closeRt = closeObj.GetComponent<RectTransform>();
        closeRt.sizeDelta = new Vector2(250, 70);
        closeRt.anchorMin = new Vector2(0.5f, 0.1f); closeRt.anchorMax = new Vector2(0.5f, 0.1f);
        closeRt.anchoredPosition = new Vector2(0, 0);

        GameObject closeTxtObj = new GameObject("Text");
        closeTxtObj.transform.SetParent(closeObj.transform, false);
        TextMeshProUGUI closeTxt = closeTxtObj.AddComponent<TextMeshProUGUI>();
        closeTxt.text = "НАЗАД";
        closeTxt.color = Color.white;
        closeTxt.fontSize = 30;
        closeTxt.alignment = TextAlignmentOptions.Center;
        RectTransform closeTxtRt = closeTxtObj.GetComponent<RectTransform>();
        closeTxtRt.anchorMin = Vector2.zero; closeTxtRt.anchorMax = Vector2.one;
        closeTxtRt.offsetMin = Vector2.zero; closeTxtRt.offsetMax = Vector2.zero;

        // Сетка кнопок смен
        for (int i = 0; i <= maxUnlocked; i++)
        {
            int shiftIndex = i; // Обязательно для правильного замыкания в AddListener!
            
            GameObject btnObj = new GameObject("ShiftBtn_" + i);
            btnObj.transform.SetParent(levelsPanel.transform, false);
            UnityEngine.UI.Image btnImg = btnObj.AddComponent<UnityEngine.UI.Image>();
            btnImg.color = new Color(0.2f, 0.4f, 0.2f);
            UnityEngine.UI.Button btn = btnObj.AddComponent<UnityEngine.UI.Button>();
            
            btn.onClick.AddListener(() => LoadSpecificShift(shiftIndex));

            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.sizeDelta = new Vector2(180, 180);
            
            // Простая сетка: 4 в ряд, по центру экрана
            int row = i / 4;
            int col = i % 4;
            btnRt.anchorMin = new Vector2(0.5f, 0.5f);
            btnRt.anchorMax = new Vector2(0.5f, 0.5f);
            
            float startX = -330f;
            float startY = 150f;
            btnRt.anchoredPosition = new Vector2(startX + col * 220f, startY - row * 220f);

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = "Смена\n" + (i + 1);
            txt.color = Color.white;
            txt.fontSize = 35;
            txt.alignment = TextAlignmentOptions.Center;
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero; txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero; txtRt.offsetMax = Vector2.zero;
        }
    }

    // ==========================================
    // ЛОГИКА НАСТРОЕК ЗВУКА
    // ==========================================
    public void ShowSettingsPanel()
    {
        if (uiAudioSource != null && buttonClickSound != null) uiAudioSource.PlayOneShot(buttonClickSound);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

            if (musicSlider != null) musicSlider.value = musicVol;
            if (sfxSlider != null) sfxSlider.value = sfxVol;
        }
    }

    public void HideSettingsPanel()
    {
        if (uiAudioSource != null && buttonClickSound != null) uiAudioSource.PlayOneShot(buttonClickSound);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        if (uiAudioSource != null)
        {
            uiAudioSource.volume = value;
        }
    }
}
