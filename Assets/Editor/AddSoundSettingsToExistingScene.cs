using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

public class AddSoundSettingsToExistingScene : EditorWindow
{
    [MenuItem("Parallax/Добавить только Настройки Звука")]
    public static void AddSoundSettings()
    {
        var activeScene = EditorSceneManager.GetActiveScene();
        if (activeScene.name == "MainMenu")
        {
            AddSoundSettingsToMainMenu(activeScene);
        }
        else if (activeScene.name == "GameScene")
        {
            AddSoundSettingsToGameScene(activeScene);
        }
        else
        {
            EditorUtility.DisplayDialog("Ошибка", "Пожалуйста, откройте сцену MainMenu или GameScene для добавления настроек звука!", "OK");
        }
    }

    private static void AddSoundSettingsToMainMenu(UnityEngine.SceneManagement.Scene scene)
    {
        MainMenu mm = Object.FindAnyObjectByType<MainMenu>();
        if (mm == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Не найден компонент MainMenu в активной сцене!", "OK");
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Не найден Canvas в активной сцене!", "OK");
            return;
        }

        // Ищем контейнер экрана (стекло) или используем сам Canvas
        Transform parentTransform = canvas.transform;
        GameObject screenContainerObj = FindInActiveScene("ScreenContainer");
        if (screenContainerObj != null) parentTransform = screenContainerObj.transform;

        // Удаляем старую панель настроек, если она уже была создана
        GameObject oldSettings = FindInActiveScene("SettingsPanel");
        if (oldSettings != null && oldSettings.transform.parent == parentTransform)
        {
            DestroyImmediate(oldSettings);
        }

        Color retroOrange = new Color(1f, 0.6f, 0f);

        // 1. Создаем панель настроек
        GameObject settingsPanelObj = new GameObject("SettingsPanel");
        settingsPanelObj.transform.SetParent(parentTransform, false);
        settingsPanelObj.transform.SetAsLastSibling();

        Image settingsBg = settingsPanelObj.AddComponent<Image>();
        settingsBg.color = new Color(0.05f, 0.03f, 0f, 0.98f); // CRT оранжево-черный фон
        settingsBg.raycastTarget = true;

        RectTransform settingsRt = settingsPanelObj.GetComponent<RectTransform>();
        settingsRt.anchorMin = Vector2.zero;
        settingsRt.anchorMax = Vector2.one;
        settingsRt.offsetMin = Vector2.zero;
        settingsRt.offsetMax = Vector2.zero;

        // Рамка панели настроек
        CreateNeonBorder(settingsPanelObj, retroOrange);

        // Заголовок настроек
        CreateText("SettingsTitle", settingsPanelObj.transform, new Vector2(0f, 0.75f), new Vector2(1f, 0.95f), "SOUND SETTINGS", 80, retroOrange);

        // Функция создания слайдеров
        Slider CreateVolumeSlider(string labelText, Vector2 posMin, Vector2 posMax)
        {
            GameObject container = new GameObject(labelText + "_SliderContainer");
            container.transform.SetParent(settingsPanelObj.transform, false);
            RectTransform contRt = container.AddComponent<RectTransform>();
            contRt.anchorMin = posMin;
            contRt.anchorMax = posMax;
            contRt.offsetMin = Vector2.zero;
            contRt.offsetMax = Vector2.zero;

            CreateText(labelText + "_Label", container.transform, new Vector2(0f, 0.6f), new Vector2(1f, 1f), labelText, 45, retroOrange);

            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(container.transform, false);
            RectTransform slRt = sliderObj.AddComponent<RectTransform>();
            slRt.anchorMin = new Vector2(0.15f, 0.15f);
            slRt.anchorMax = new Vector2(0.85f, 0.45f);
            slRt.offsetMin = Vector2.zero;
            slRt.offsetMax = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.12f, 0f, 1f);
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.25f);
            bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.25f);
            faRt.anchorMax = new Vector2(1f, 0.75f);
            faRt.offsetMin = new Vector2(5, 0);
            faRt.offsetMax = new Vector2(-5, 0);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = retroOrange;
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            GameObject handleArea = new GameObject("Handle Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform haRt = handleArea.AddComponent<RectTransform>();
            haRt.anchorMin = new Vector2(0f, 0f);
            haRt.anchorMax = new Vector2(1f, 1f);
            haRt.offsetMin = new Vector2(10, 0);
            haRt.offsetMax = new Vector2(-10, 0);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;
            RectTransform handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(25, 0);
            handleRt.anchorMin = new Vector2(0f, 0f);
            handleRt.anchorMax = new Vector2(0f, 1f);
            handleRt.anchoredPosition = Vector2.zero;

            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.minValue = 0f;
            slider.maxValue = 1f;

            return slider;
        }

        Slider musicSld = CreateVolumeSlider("MUSIC / AMBIENCE", new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.70f));
        Slider sfxSld = CreateVolumeSlider("SOUND EFFECTS (SFX)", new Vector2(0.1f, 0.23f), new Vector2(0.9f, 0.45f));

        // Кнопка BACK настроек
        GameObject backBtnObj = new GameObject("BackButton");
        backBtnObj.transform.SetParent(settingsPanelObj.transform, false);
        Image backBtnImg = backBtnObj.AddComponent<Image>();
        backBtnImg.color = new Color(1f, 1f, 1f, 0f);
        Button backBtn = backBtnObj.AddComponent<Button>();
        RectTransform backRt = backBtnObj.GetComponent<RectTransform>();
        backRt.anchorMin = new Vector2(0.3f, 0.05f);
        backRt.anchorMax = new Vector2(0.7f, 0.17f);
        backRt.offsetMin = Vector2.zero;
        backRt.offsetMax = Vector2.zero;
        CreateText("BackButton_Text", backBtnObj.transform, Vector2.zero, Vector2.one, "BACK", 60, retroOrange);

        // Связываем MainMenu
        mm.settingsPanel = settingsPanelObj;
        mm.musicSlider = musicSld;
        mm.sfxSlider = sfxSld;

        // Подключаем слушатели
        backBtn.onClick.RemoveAllListeners();
        musicSld.onValueChanged.RemoveAllListeners();
        sfxSld.onValueChanged.RemoveAllListeners();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtn.onClick, new UnityEngine.Events.UnityAction(mm.HideSettingsPanel));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(musicSld.onValueChanged, new UnityEngine.Events.UnityAction<float>(mm.OnMusicVolumeChanged));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(sfxSld.onValueChanged, new UnityEngine.Events.UnityAction<float>(mm.OnSFXVolumeChanged));

        settingsPanelObj.SetActive(false);

        // Связываем существующую кнопку SettingsButton
        GameObject settingsBtnObj = FindInActiveScene("SettingsButton");
        if (settingsBtnObj != null)
        {
            Button btn = settingsBtnObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                UnityEditor.Events.UnityEventTools.AddPersistentListener(btn.onClick, new UnityEngine.Events.UnityAction(mm.ShowSettingsPanel));
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("Успех", "Настройки звука успешно добавлены в существующее Главное Меню!", "Отлично");
    }

    private static void AddSoundSettingsToGameScene(UnityEngine.SceneManagement.Scene scene)
    {
        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Не найден компонент GameManager в активной сцене!", "OK");
            return;
        }

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Не найден Canvas в активной сцене!", "OK");
            return;
        }

        // Удаляем старую панель настроек, если она была создана
        GameObject oldSettings = FindInActiveScene("SettingsPanel");
        if (oldSettings != null && oldSettings.transform.parent == canvas.transform)
        {
            DestroyImmediate(oldSettings);
        }

        Color neonGreen = new Color(0.2f, 1f, 0.2f, 1f);

        // 1. Создаем панель настроек в Canvas
        GameObject settingsPanelObj = new GameObject("SettingsPanel");
        settingsPanelObj.transform.SetParent(canvas.transform, false);
        settingsPanelObj.transform.SetAsLastSibling();

        Image settingsBg = settingsPanelObj.AddComponent<Image>();
        settingsBg.color = new Color(0.01f, 0.05f, 0.01f, 0.98f);
        settingsBg.raycastTarget = true;

        RectTransform settingsRt = settingsPanelObj.GetComponent<RectTransform>();
        settingsRt.anchorMin = Vector2.zero;
        settingsRt.anchorMax = Vector2.one;
        settingsRt.offsetMin = Vector2.zero;
        settingsRt.offsetMax = Vector2.zero;

        CreateNeonBorder(settingsPanelObj, neonGreen);

        CreateText("SettingsTitle", settingsPanelObj.transform, "НАСТРОЙКИ ЗВУКА", 70, neonGreen, new Vector2(0, 350), new Vector2(800, 150));

        // Функция создания слайдеров
        System.Func<string, Vector2, Slider> CreateVolumeSlider = (labelText, anchoredPos) =>
        {
            GameObject container = new GameObject(labelText + "_SliderContainer");
            container.transform.SetParent(settingsPanelObj.transform, false);
            RectTransform contRt = container.AddComponent<RectTransform>();
            contRt.anchorMin = new Vector2(0.5f, 0.5f);
            contRt.anchorMax = new Vector2(0.5f, 0.5f);
            contRt.anchoredPosition = anchoredPos;
            contRt.sizeDelta = new Vector2(800, 200);

            CreateText(labelText + "_Label", container.transform, labelText, 36, neonGreen, new Vector2(0, 60), new Vector2(800, 50));

            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(container.transform, false);
            RectTransform slRt = sliderObj.AddComponent<RectTransform>();
            slRt.anchorMin = new Vector2(0.5f, 0.5f);
            slRt.anchorMax = new Vector2(0.5f, 0.5f);
            slRt.anchoredPosition = new Vector2(0, -20);
            slRt.sizeDelta = new Vector2(600, 40);

            Slider slider = sliderObj.AddComponent<Slider>();

            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0f, 0.2f, 0f, 1f);
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.25f);
            bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform faRt = fillArea.AddComponent<RectTransform>();
            faRt.anchorMin = new Vector2(0f, 0.25f);
            faRt.anchorMax = new Vector2(1f, 0.75f);
            faRt.offsetMin = new Vector2(5, 0);
            faRt.offsetMax = new Vector2(-5, 0);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImg = fill.AddComponent<Image>();
            fillImg.color = neonGreen;
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            GameObject handleArea = new GameObject("Handle Area");
            handleArea.transform.SetParent(sliderObj.transform, false);
            RectTransform haRt = handleArea.AddComponent<RectTransform>();
            haRt.anchorMin = new Vector2(0f, 0f);
            haRt.anchorMax = new Vector2(1f, 1f);
            haRt.offsetMin = new Vector2(10, 0);
            haRt.offsetMax = new Vector2(-10, 0);

            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(handleArea.transform, false);
            Image handleImg = handle.AddComponent<Image>();
            handleImg.color = Color.white;
            RectTransform handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(25, 0);
            handleRt.anchorMin = new Vector2(0f, 0f);
            handleRt.anchorMax = new Vector2(0f, 1f);
            handleRt.anchoredPosition = Vector2.zero;

            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.minValue = 0f;
            slider.maxValue = 1f;

            return slider;
        };

        Slider musicSld = CreateVolumeSlider("МУЗЫКА / ФОН", new Vector2(0, 100));
        Slider sfxSld = CreateVolumeSlider("ЭФФЕКТЫ / SFX", new Vector2(0, -100));

        // Кнопка BACK настроек
        GameObject backBtnObj = new GameObject("BackBtn");
        backBtnObj.transform.SetParent(settingsPanelObj.transform, false);
        Image backBtnImg = backBtnObj.AddComponent<Image>();
        backBtnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Button btnBack = backBtnObj.AddComponent<Button>();
        RectTransform backRt = backBtnObj.GetComponent<RectTransform>();
        backRt.anchorMin = new Vector2(0.5f, 0.5f);
        backRt.anchorMax = new Vector2(0.5f, 0.5f);
        backRt.anchoredPosition = new Vector2(0, -300);
        backRt.sizeDelta = new Vector2(400, 80);
        CreateText("Text", backBtnObj.transform, "НАЗАД", 30, Color.white, Vector2.zero, new Vector2(400, 80));

        // Связываем GameManager
        gm.settingsPanel = settingsPanelObj;
        gm.musicSlider = musicSld;
        gm.sfxSlider = sfxSld;

        btnBack.onClick.RemoveAllListeners();
        musicSld.onValueChanged.RemoveAllListeners();
        sfxSld.onValueChanged.RemoveAllListeners();

        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBack.onClick, new UnityEngine.Events.UnityAction(gm.CloseSettings));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(musicSld.onValueChanged, new UnityEngine.Events.UnityAction<float>(gm.OnMusicVolumeChanged));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(sfxSld.onValueChanged, new UnityEngine.Events.UnityAction<float>(gm.OnSFXVolumeChanged));

        settingsPanelObj.SetActive(false);

        // 2. Интегрируем кнопку SETTINGS в существующий PausePanel
        GameObject pausePanel = FindInActiveScene("PausePanel");
        if (pausePanel != null)
        {
            // Ищем или создаем SettingsBtn внутри PausePanel
            GameObject settingsBtnObj = FindInActiveScene("SettingsBtn");
            if (settingsBtnObj == null || settingsBtnObj.transform.parent != pausePanel.transform)
            {
                if (settingsBtnObj != null) DestroyImmediate(settingsBtnObj);

                settingsBtnObj = new GameObject("SettingsBtn");
                settingsBtnObj.transform.SetParent(pausePanel.transform, false);
                Image settingsBtnImg = settingsBtnObj.AddComponent<Image>();
                settingsBtnImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
                CreateText("Text", settingsBtnObj.transform, "НАСТРОЙКИ", 30, Color.white, Vector2.zero, new Vector2(400, 80));
            }

            Button btnSettings = settingsBtnObj.GetComponent<Button>();
            if (btnSettings == null) btnSettings = settingsBtnObj.AddComponent<Button>();
            
            RectTransform settingsBtnRt = settingsBtnObj.GetComponent<RectTransform>();
            settingsBtnRt.anchorMin = new Vector2(0.5f, 0.5f);
            settingsBtnRt.anchorMax = new Vector2(0.5f, 0.5f);
            settingsBtnRt.anchoredPosition = new Vector2(0, -25);
            settingsBtnRt.sizeDelta = new Vector2(400, 80);

            btnSettings.onClick.RemoveAllListeners();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettings.onClick, new UnityEngine.Events.UnityAction(gm.OpenSettings));

            // Автоматически перекомпонуем другие кнопки PausePanel, если они найдены
            GameObject resumeBtnObj = FindInActiveScene("ResumeBtn");
            if (resumeBtnObj != null && resumeBtnObj.transform.parent == pausePanel.transform)
            {
                RectTransform rRt = resumeBtnObj.GetComponent<RectTransform>();
                rRt.anchoredPosition = new Vector2(0, 75);
            }

            GameObject exitBtnObj = FindInActiveScene("ExitBtn");
            if (exitBtnObj != null && exitBtnObj.transform.parent == pausePanel.transform)
            {
                RectTransform eRt = exitBtnObj.GetComponent<RectTransform>();
                eRt.anchoredPosition = new Vector2(0, -125);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorUtility.DisplayDialog("Успех", "Настройки звука успешно интегрированы в вашу существующую Игровую Сцену!", "Отлично");
    }

    private static void CreateNeonBorder(GameObject parent, Color color)
    {
        // Верхняя
        GameObject LineT = new GameObject("LineT"); LineT.transform.SetParent(parent.transform, false);
        LineT.AddComponent<Image>().color = color;
        RectTransform rT = LineT.GetComponent<RectTransform>(); rT.anchorMin = new Vector2(0, 1); rT.anchorMax = new Vector2(1, 1); rT.anchoredPosition = Vector2.zero; rT.sizeDelta = new Vector2(0, 10);
        // Нижняя
        GameObject LineB = new GameObject("LineB"); LineB.transform.SetParent(parent.transform, false);
        LineB.AddComponent<Image>().color = color;
        RectTransform rB = LineB.GetComponent<RectTransform>(); rB.anchorMin = new Vector2(0, 0); rB.anchorMax = new Vector2(1, 0); rB.anchoredPosition = Vector2.zero; rB.sizeDelta = new Vector2(0, 10);
        // Левая
        GameObject LineL = new GameObject("LineL"); LineL.transform.SetParent(parent.transform, false);
        LineL.AddComponent<Image>().color = color;
        RectTransform rL = LineL.GetComponent<RectTransform>(); rL.anchorMin = new Vector2(0, 0); rL.anchorMax = new Vector2(0, 1); rL.anchoredPosition = Vector2.zero; rL.sizeDelta = new Vector2(10, 0);
        // Правая
        GameObject LineR = new GameObject("LineR"); LineR.transform.SetParent(parent.transform, false);
        LineR.AddComponent<Image>().color = color;
        RectTransform rR = LineR.GetComponent<RectTransform>(); rR.anchorMin = new Vector2(1, 0); rR.anchorMax = new Vector2(1, 1); rR.anchoredPosition = Vector2.zero; rR.sizeDelta = new Vector2(10, 0);
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string defaultText, int fontSize, Color color)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        RectTransform rect = txtObj.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return tmp;
    }

    private static TextMeshProUGUI CreateText(string name, Transform parent, string text, int fontSize, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        GameObject txtObj = new GameObject(name);
        txtObj.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        RectTransform rect = txtObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = sizeDelta;
        return tmp;
    }

    private static GameObject FindInActiveScene(string name)
    {
        var rootObjs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var root in rootObjs)
        {
            Transform found = FindRecursive(root.transform, name);
            if (found != null) return found.gameObject;
        }
        return null;
    }

    private static Transform FindRecursive(Transform parent, string name)
    {
        if (parent.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindRecursive(parent.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }
}
