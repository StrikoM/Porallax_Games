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
        Button continueBtn = CreateButton("ContinueButton", new Vector2(0.1f, 0.40f), new Vector2(0.9f, 0.55f), "CONTINUE SHIFT", true);
        Button playBtn = CreateButton("PlayButton", new Vector2(0.2f, 0.25f), new Vector2(0.8f, 0.35f), "NEW GAME", false);
        Button quitBtn = CreateButton("QuitButton", new Vector2(0.2f, 0.10f), new Vector2(0.8f, 0.20f), "EXIT", false);

        // Привязываем переменные к скрипту меню
        mm.continueButton = continueBtn.gameObject;
        mm.continueButtonText = continueBtn.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        // Привязываем методы к кнопкам
        UnityEditor.Events.UnityEventTools.AddPersistentListener(continueBtn.onClick, new UnityEngine.Events.UnityAction(mm.ContinueGame));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(playBtn.onClick, new UnityEngine.Events.UnityAction(mm.NewGame));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(quitBtn.onClick, new UnityEngine.Events.UnityAction(mm.QuitGame));

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
