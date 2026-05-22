using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public class MainMenuBuilder : EditorWindow
{
    [MenuItem("Parallax/Собрать Главное Меню")]
    public static void BuildMainMenu()
    {
        // 1. Создаем папку Scenes если её нет
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
        {
            AssetDatabase.CreateFolder("Assets", "Scenes");
        }

        // 2. Безопасно сохраняем текущую сцену
        EditorSceneManager.SaveOpenScenes();

        // 3. Создаем новую пустую сцену для Меню
        var mainMenuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 4. Строим UI
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            // Внимание: мы больше не добавляем StandaloneInputModule через код, 
            // так как это ломает новую систему ввода (Input System).
        }

        // Атмосферный фон (ЭЛТ-монитор)
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.white; // Белый цвет, чтобы картинка монитора не искажалась
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // Контейнер, который ограничивает зону только СТЕКЛОМ монитора
        // (чтобы текст не залезал на пластиковые рамки)
        GameObject screenContainerObj = new GameObject("ScreenContainer");
        screenContainerObj.transform.SetParent(canvasObj.transform, false);
        RectTransform screenRect = screenContainerObj.AddComponent<RectTransform>();
        screenRect.anchorMin = new Vector2(0.15f, 0.18f); // Отступы от краев (рамки монитора)
        screenRect.anchorMax = new Vector2(0.85f, 0.85f);
        screenRect.offsetMin = Vector2.zero;
        screenRect.offsetMax = Vector2.zero;

        // Вспомогательная функция для создания текстов
        TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string defaultText, int fontSize, Color color)
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

        Color retroOrange = new Color(1f, 0.6f, 0f); // Неоновый оранжевый

        // Заголовок PARALLAX (Внутри стекла монитора)
        CreateText("TitleText", screenContainerObj.transform, new Vector2(0.0f, 0.65f), new Vector2(1.0f, 0.95f), "PARALLAX", 180, retroOrange);

        // Объект для скрипта меню
        GameObject managerObj = new GameObject("MainMenuManager");
        MainMenu mm = managerObj.AddComponent<MainMenu>();

        // Вспомогательная функция для прозрачных ретро-кнопок
        Button CreateButton(string name, Vector2 anchorMin, Vector2 anchorMax, string btnText, bool addBox)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(screenContainerObj.transform, false); // Привязываем к СТЕКЛУ
            Image img = btnObj.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0f); // Полностью прозрачный фон
            
            Button btn = btnObj.AddComponent<Button>();
            
            // Настройка цветов (меняется цвет самого текста через Transition, но тут проще сделать через EventTrigger или оставить прозрачным)
            ColorBlock cb = btn.colors;
            cb.normalColor = new Color(1f, 1f, 1f, 0f);
            cb.highlightedColor = new Color(1f, 1f, 1f, 0.1f); // Легкая подсветка фона при наведении
            cb.pressedColor = new Color(1f, 1f, 1f, 0.2f);
            cb.selectedColor = cb.normalColor;
            cb.colorMultiplier = 1;
            btn.colors = cb;

            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            CreateText(name + "_Text", btnObj.transform, Vector2.zero, Vector2.one, btnText, 60, retroOrange);

            // Добавляем рамку, как в макете
            if (addBox)
            {
                // Верхняя
                GameObject tLine = new GameObject("LineT"); tLine.transform.SetParent(btnObj.transform, false);
                Image iT = tLine.AddComponent<Image>(); iT.color = retroOrange;
                RectTransform rT = tLine.GetComponent<RectTransform>(); rT.anchorMin = new Vector2(0, 1); rT.anchorMax = new Vector2(1, 1); rT.anchoredPosition = new Vector2(0, 0); rT.sizeDelta = new Vector2(0, 5);
                // Нижняя
                GameObject bLine = new GameObject("LineB"); bLine.transform.SetParent(btnObj.transform, false);
                Image iB = bLine.AddComponent<Image>(); iB.color = retroOrange;
                RectTransform rB = bLine.GetComponent<RectTransform>(); rB.anchorMin = new Vector2(0, 0); rB.anchorMax = new Vector2(1, 0); rB.anchoredPosition = new Vector2(0, 0); rB.sizeDelta = new Vector2(0, 5);
                // Левая
                GameObject lLine = new GameObject("LineL"); lLine.transform.SetParent(btnObj.transform, false);
                Image iL = lLine.AddComponent<Image>(); iL.color = retroOrange;
                RectTransform rL = lLine.GetComponent<RectTransform>(); rL.anchorMin = new Vector2(0, 0); rL.anchorMax = new Vector2(0, 1); rL.anchoredPosition = new Vector2(0, 0); rL.sizeDelta = new Vector2(5, 0);
                // Правая
                GameObject rLine = new GameObject("LineR"); rLine.transform.SetParent(btnObj.transform, false);
                Image iR = rLine.AddComponent<Image>(); iR.color = retroOrange;
                RectTransform rR = rLine.GetComponent<RectTransform>(); rR.anchorMin = new Vector2(1, 0); rR.anchorMax = new Vector2(1, 1); rR.anchoredPosition = new Vector2(0, 0); rR.sizeDelta = new Vector2(5, 0);
            }

            return btn;
        }

        // Создаем кнопки (координаты теперь относительно стекла)
        Button continueBtn = CreateButton("ContinueButton", new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.60f), "CONTINUE SHIFT", true);
        Button playBtn = CreateButton("PlayButton", new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.45f), "NEW GAME", false);
        Button settingsBtn = CreateButton("SettingsButton", new Vector2(0.2f, 0.22f), new Vector2(0.8f, 0.32f), "SETTINGS", false);
        Button quitBtn = CreateButton("QuitButton", new Vector2(0.2f, 0.08f), new Vector2(0.8f, 0.18f), "EXIT", false);

        // Привязываем переменные к скрипту меню
        mm.continueButton = continueBtn.gameObject;
        mm.continueButtonText = continueBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        // Привязываем методы к кнопкам
        UnityEditor.Events.UnityEventTools.AddPersistentListener(continueBtn.onClick, new UnityEngine.Events.UnityAction(mm.ContinueGame));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, new UnityEngine.Events.UnityAction(mm.NewGame));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(settingsBtn.onClick, new UnityEngine.Events.UnityAction(mm.ShowSettingsPanel));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.onClick, new UnityEngine.Events.UnityAction(mm.QuitGame));

        // ===================================================
        // ПРОЦЕДУРНОЕ СТРОЕНИЕ ПАНЕЛИ НАСТРОЕК (SettingsPanel)
        // ===================================================
        GameObject settingsPanelObj = new GameObject("SettingsPanel");
        settingsPanelObj.transform.SetParent(screenContainerObj.transform, false);
        settingsPanelObj.transform.SetAsLastSibling();
        
        Image settingsBg = settingsPanelObj.AddComponent<Image>();
        settingsBg.color = new Color(0.05f, 0.03f, 0f, 0.98f); // Очень темный оранжево-черный ЭЛТ
        settingsBg.raycastTarget = true;

        RectTransform settingsRt = settingsPanelObj.GetComponent<RectTransform>();
        settingsRt.anchorMin = Vector2.zero;
        settingsRt.anchorMax = Vector2.one;
        settingsRt.offsetMin = Vector2.zero;
        settingsRt.offsetMax = Vector2.zero;

        // Рамка панели настроек
        // Верхняя
        GameObject sLineT = new GameObject("LineT"); sLineT.transform.SetParent(settingsPanelObj.transform, false);
        sLineT.AddComponent<Image>().color = retroOrange;
        RectTransform srT = sLineT.GetComponent<RectTransform>(); srT.anchorMin = new Vector2(0, 1); srT.anchorMax = new Vector2(1, 1); srT.anchoredPosition = Vector2.zero; srT.sizeDelta = new Vector2(0, 10);
        // Нижняя
        GameObject sLineB = new GameObject("LineB"); sLineB.transform.SetParent(settingsPanelObj.transform, false);
        sLineB.AddComponent<Image>().color = retroOrange;
        RectTransform srB = sLineB.GetComponent<RectTransform>(); srB.anchorMin = new Vector2(0, 0); srB.anchorMax = new Vector2(1, 0); srB.anchoredPosition = Vector2.zero; srB.sizeDelta = new Vector2(0, 10);
        // Левая
        GameObject sLineL = new GameObject("LineL"); sLineL.transform.SetParent(settingsPanelObj.transform, false);
        sLineL.AddComponent<Image>().color = retroOrange;
        RectTransform srL = sLineL.GetComponent<RectTransform>(); srL.anchorMin = new Vector2(0, 0); srL.anchorMax = new Vector2(0, 1); srL.anchoredPosition = Vector2.zero; srL.sizeDelta = new Vector2(10, 0);
        // Правая
        GameObject sLineR = new GameObject("LineR"); sLineR.transform.SetParent(settingsPanelObj.transform, false);
        sLineR.AddComponent<Image>().color = retroOrange;
        RectTransform srR = sLineR.GetComponent<RectTransform>(); srR.anchorMin = new Vector2(1, 0); srR.anchorMax = new Vector2(1, 1); srR.anchoredPosition = Vector2.zero; srR.sizeDelta = new Vector2(10, 0);

        // Заголовок настроек
        CreateText("SettingsTitle", settingsPanelObj.transform, new Vector2(0f, 0.75f), new Vector2(1f, 0.95f), "SOUND SETTINGS", 80, retroOrange);

        // Вспомогательная функция для создания слайдеров
        Slider CreateVolumeSlider(string labelText, Vector2 posMin, Vector2 posMax)
        {
            GameObject container = new GameObject(labelText + "_SliderContainer");
            container.transform.SetParent(settingsPanelObj.transform, false);
            RectTransform contRt = container.AddComponent<RectTransform>();
            contRt.anchorMin = posMin;
            contRt.anchorMax = posMax;
            contRt.offsetMin = Vector2.zero;
            contRt.offsetMax = Vector2.zero;

            // Текстовая подпись
            CreateText(labelText + "_Label", container.transform, new Vector2(0f, 0.6f), new Vector2(1f, 1f), labelText, 45, retroOrange);

            // Сам объект Слайдера
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(container.transform, false);
            RectTransform slRt = sliderObj.AddComponent<RectTransform>();
            slRt.anchorMin = new Vector2(0.15f, 0.15f);
            slRt.anchorMax = new Vector2(0.85f, 0.45f);
            slRt.offsetMin = Vector2.zero;
            slRt.offsetMax = Vector2.zero;

            Slider slider = sliderObj.AddComponent<Slider>();

            // Background слайдера
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.12f, 0f, 1f); // Темный оранжевый
            RectTransform bgRt = bg.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0f, 0.25f);
            bgRt.anchorMax = new Vector2(1f, 0.75f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;

            // Fill Area слайдера
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
            fillImg.color = retroOrange; // Яркий неоновый оранжевый для заполненной части
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = Vector2.zero;
            fillRt.offsetMax = Vector2.zero;

            // Handle Area слайдера
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
            handleImg.color = Color.white; // Белая пиксельная ручка
            RectTransform handleRt = handle.GetComponent<RectTransform>();
            handleRt.sizeDelta = new Vector2(25, 0);
            handleRt.anchorMin = new Vector2(0f, 0f);
            handleRt.anchorMax = new Vector2(0f, 1f);
            handleRt.anchoredPosition = Vector2.zero;

            // Настройка компонента слайдера
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.minValue = 0f;
            slider.maxValue = 1f;

            return slider;
        }

        // Создаем слайдеры громкости музыки и SFX
        Slider musicSld = CreateVolumeSlider("MUSIC / AMBIENCE", new Vector2(0.1f, 0.48f), new Vector2(0.9f, 0.70f));
        Slider sfxSld = CreateVolumeSlider("SOUND EFFECTS (SFX)", new Vector2(0.1f, 0.23f), new Vector2(0.9f, 0.45f));

        // Кнопка BACK настроек
        Button backBtn = CreateButton("BackButton", new Vector2(0.3f, 0.05f), new Vector2(0.7f, 0.17f), "BACK", true);
        backBtn.transform.SetParent(settingsPanelObj.transform, false); // Привязываем к панели настроек

        // Назначаем ссылки в MainMenuManager
        mm.settingsPanel = settingsPanelObj;
        mm.musicSlider = musicSld;
        mm.sfxSlider = sfxSld;

        // Подключаем методы
        UnityEditor.Events.UnityEventTools.AddPersistentListener(backBtn.onClick, new UnityEngine.Events.UnityAction(mm.HideSettingsPanel));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(musicSld.onValueChanged, new UnityEngine.Events.UnityAction<float>(mm.OnMusicVolumeChanged));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(sfxSld.onValueChanged, new UnityEngine.Events.UnityAction<float>(mm.OnSFXVolumeChanged));

        settingsPanelObj.SetActive(false); // Скрыта по умолчанию


        // 5. Сохраняем сцену меню
        EditorSceneManager.SaveScene(mainMenuScene, "Assets/Scenes/MainMenu.unity");

        // 6. Добавляем обе сцены в настройки билда (чтобы Unity могла их переключать)
        EditorBuildSettingsScene[] originalScenes = EditorBuildSettings.scenes;
        bool hasMainMenu = false;
        bool hasGameScene = false;

        foreach (var s in originalScenes)
        {
            if (s.path == "Assets/Scenes/MainMenu.unity") hasMainMenu = true;
            if (s.path == "Assets/Scenes/GameScene.unity") hasGameScene = true;
        }

        int newLength = originalScenes.Length;
        if (!hasMainMenu) newLength++;
        if (!hasGameScene) newLength++;

        EditorBuildSettingsScene[] newScenes = new EditorBuildSettingsScene[newLength];
        int index = 0;
        
        // Главное меню всегда должно быть первым
        if (!hasMainMenu)
        {
            newScenes[index++] = new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true);
        }
        else
        {
            foreach (var s in originalScenes)
            {
                if (s.path == "Assets/Scenes/MainMenu.unity") newScenes[index++] = s;
            }
        }
        
        // Затем все остальные сцены
        foreach (var s in originalScenes)
        {
            if (s.path != "Assets/Scenes/MainMenu.unity" && s.path != "Assets/Scenes/GameScene.unity")
            {
                newScenes[index++] = s;
            }
        }
        
        // Игровая сцена
        if (!hasGameScene)
        {
            newScenes[index++] = new EditorBuildSettingsScene("Assets/Scenes/GameScene.unity", true);
        }
        else
        {
            foreach (var s in originalScenes)
            {
                if (s.path == "Assets/Scenes/GameScene.unity") newScenes[index++] = s;
            }
        }

        EditorBuildSettings.scenes = newScenes;

        // Покажем окно об успехе
        EditorUtility.DisplayDialog("Готово!", "Атмосферное Главное Меню успешно создано!\nОбе сцены сохранены и связаны.", "Супер");
    }
}
