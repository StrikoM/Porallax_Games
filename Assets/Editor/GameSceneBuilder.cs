using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public class GameSceneBuilder : EditorWindow
{
    [MenuItem("Parallax/Собрать GameScene (Стиль Окна)")]
    public static void BuildGameScene()
    {
        var gameScene = EditorSceneManager.GetActiveScene();
        if (gameScene.name != "GameScene")
        {
            EditorUtility.DisplayDialog("Ошибка", "Пожалуйста, открой GameScene перед тем как собирать этот интерфейс!", "OK");
            return;
        }

        // Ищем старый Canvas и удаляем его, чтобы создать с нуля
        Canvas oldCanvas = Object.FindAnyObjectByType<Canvas>();
        if (oldCanvas != null)
        {
            DestroyImmediate(oldCanvas.gameObject);
        }

        // Ищем старый GameManager
        GameManager oldGm = Object.FindAnyObjectByType<GameManager>();
        ShiftData[] savedShifts = null;
        AudioClip oldCloseSound = null;
        AudioClip oldOpenSound = null;

        if (oldGm != null)
        {
            savedShifts = oldGm.shiftsDatabase; // Сохраняем смены, чтобы не пропали
            oldCloseSound = oldGm.shutterCloseSound;
            oldOpenSound = oldGm.shutterOpenSound;
            DestroyImmediate(oldGm.gameObject);
        }

        // Удаляем старые аудио объекты, если они есть
        GameObject oldBgSound = GameObject.Find("BackgroundAudio");
        if (oldBgSound != null) DestroyImmediate(oldBgSound);
        
        GameObject oldSfxSound = GameObject.Find("SFXAudio");
        if (oldSfxSound != null) DestroyImmediate(oldSfxSound);

        // 1. Создаем Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 0. Автоматически добавляем EventSystem, если его нет
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            // Для совместимости с новым Input System
            if (System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem") != null)
            {
                eventSystem.AddComponent(System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"));
            }
            else
            {
                eventSystem.AddComponent<StandaloneInputModule>();
            }
        }

        // Функция-помощник
        GameObject CreatePanel(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            Image img = obj.AddComponent<Image>();
            img.color = color;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            return obj;
        }

        TextMeshProUGUI CreateText(string name, Transform parent, string text, int size, Color color, Vector2 anchoredPos, Vector2 sizeDelta)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = sizeDelta;
            return tmp;
        }

        // --- СОЗДАЕМ ВИЗУАЛ (КАК В THATS NOT MY NEIGHBOR) ---

        // 1. Стена (Фон кабинки)
        GameObject wallObj = CreatePanel("WallBackground", canvasObj.transform, new Color(0.12f, 0.14f, 0.15f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        string wallImgPath = @"C:\Users\Madi\.gemini\antigravity\brain\5fcc281b-986b-455d-b64c-7229dce27ac4\wall_interior_minimalist_1778525220946.png";
        if (System.IO.File.Exists(wallImgPath))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(wallImgPath);
            Texture2D tex = new Texture2D(2, 2); tex.LoadImage(fileData);
            wallObj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            wallObj.GetComponent<Image>().color = Color.white;
        }

        // 2. Рамка окна (Железная окантовка)
        GameObject windowFrameObj = CreatePanel("WindowFrame", canvasObj.transform, new Color(0.2f, 0.22f, 0.25f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 150), new Vector2(840, 640));

        // 3. Само окно (То, что мы видим на улице) - ЭТО БУДЕТ МАСКА
        GameObject outsideBgObj = CreatePanel("OutsideBg", windowFrameObj.transform, new Color(0.02f, 0.02f, 0.03f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 600));
        string outsideImgPath = @"C:\Users\Madi\.gemini\antigravity\brain\5fcc281b-986b-455d-b64c-7229dce27ac4\hallway_outside_window_1778514034420.png";
        if (System.IO.File.Exists(outsideImgPath))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(outsideImgPath);
            Texture2D tex = new Texture2D(2, 2); tex.LoadImage(fileData);
            outsideBgObj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            outsideBgObj.GetComponent<Image>().color = Color.white;
        }
        Mask windowMask = outsideBgObj.AddComponent<Mask>(); // Добавляем маску!
        windowMask.showMaskGraphic = true;

        // 4. Посетитель (внутри маски окна!)
        GameObject visitorObj = CreatePanel("VisitorImage", outsideBgObj.transform, Color.white, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(400, 600));
        Image visitorImg = visitorObj.GetComponent<Image>();

        // 4.5. Охранники (черные силуэты, спрятаны по бокам ЗА маской)
        GameObject guardLeftObj = CreatePanel("GuardLeft", outsideBgObj.transform, new Color(0.05f, 0.05f, 0.05f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-600, -50), new Vector2(250, 600));
        GameObject guardRightObj = CreatePanel("GuardRight", outsideBgObj.transform, new Color(0.05f, 0.05f, 0.05f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(600, -50), new Vector2(250, 600));

        // 5. Шторка (внутри маски окна, чтобы не вылезала за стены!)
        GameObject shutterObj = CreatePanel("WindowShutter", outsideBgObj.transform, new Color(0.25f, 0.28f, 0.3f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(820, 620)); // Начинает ЗАКРЫТОЙ (Y = 0)
        
        // Полоски на шторке для красоты (имитация жалюзи/рольставней)
        for(int i = 0; i < 5; i++) {
            CreatePanel("Line", shutterObj.transform, new Color(0.15f, 0.18f, 0.2f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0, -100 - (i*100)), new Vector2(0, 10));
        }

        // 5. Стол (Нижняя часть экрана)
        GameObject deskObj = CreatePanel("Desk", canvasObj.transform, new Color(0.15f, 0.15f, 0.18f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0, 200), new Vector2(0, 400));
        string deskImgPath = @"C:\Users\Madi\.gemini\antigravity\brain\5fcc281b-986b-455d-b64c-7229dce27ac4\desk_surface_1778522752365.png";
        if (System.IO.File.Exists(deskImgPath))
        {
            byte[] fileData = System.IO.File.ReadAllBytes(deskImgPath);
            Texture2D tex = new Texture2D(2, 2); tex.LoadImage(fileData);
            deskObj.GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            deskObj.GetComponent<Image>().color = Color.white;
        }
        CreatePanel("DeskEdge", canvasObj.transform, new Color(0.1f, 0.12f, 0.15f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 390), new Vector2(1920, 20));

        // 7. Лоток с документами (На столе по центру)
        GameObject trayObj = CreatePanel("DocumentTray", canvasObj.transform, new Color(0.28f, 0.3f, 0.32f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(600, 250));
        
        // Тексты паспорта и досье в лотке
        TextMeshProUGUI pName = CreateText("PassportName", trayObj.transform, "ИМЯ:\nJohn Doe", 24, Color.white, new Vector2(-150, 50), new Vector2(250, 60));
        TextMeshProUGUI pId = CreateText("PassportID", trayObj.transform, "ID:\n1234", 24, Color.white, new Vector2(-150, -20), new Vector2(250, 60));
        TextMeshProUGUI pEyes = CreateText("PassportEyes", trayObj.transform, "ГЛАЗА:\nBlue", 24, Color.white, new Vector2(-150, -90), new Vector2(250, 60));
        
        TextMeshProUGUI dName = CreateText("DossierName", trayObj.transform, "ИМЯ:\nJohn Doe", 24, new Color(0.9f, 0.8f, 0.2f), new Vector2(150, 50), new Vector2(250, 60));
        TextMeshProUGUI dId = CreateText("DossierID", trayObj.transform, "ID:\n1234", 24, new Color(0.9f, 0.8f, 0.2f), new Vector2(150, -20), new Vector2(250, 60));
        TextMeshProUGUI dEyes = CreateText("DossierEyes", trayObj.transform, "ГЛАЗА:\nBlue", 24, new Color(0.9f, 0.8f, 0.2f), new Vector2(150, -90), new Vector2(250, 60));

        // 8. Физические Кнопки на столе (Одобрить / Отклонить)
        // База для зеленой кнопки (прикручена к столу, опущена ниже)
        GameObject approveBase = CreatePanel("ApproveBase", canvasObj.transform, new Color(0.05f, 0.05f, 0.05f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-450, 70), new Vector2(240, 120));
        GameObject btnApproveObj = CreatePanel("ApproveBtn", approveBase.transform, new Color(0.15f, 0.5f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(200, 80));
        Button btnApprove = btnApproveObj.AddComponent<Button>();
        CreateText("Text", btnApproveObj.transform, "ПРОПУСТИТЬ", 24, Color.white, Vector2.zero, new Vector2(200, 80));

        // База для красной кнопки (прикручена к столу, опущена ниже)
        GameObject rejectBase = CreatePanel("RejectBase", canvasObj.transform, new Color(0.05f, 0.05f, 0.05f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(450, 70), new Vector2(240, 120));
        GameObject btnRejectObj = CreatePanel("RejectBtn", rejectBase.transform, new Color(0.6f, 0.15f, 0.15f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(200, 80));
        Button btnReject = btnRejectObj.AddComponent<Button>();
        CreateText("Text", btnRejectObj.transform, "ИЗОЛИРОВАТЬ", 24, Color.white, Vector2.zero, new Vector2(200, 80));

        // 9. ФИЗИЧЕСКИЙ МОНИТОР НА СТОЛЕ (справа снизу)
        GameObject monitorBodyObj = CreatePanel("PhysicalMonitor", canvasObj.transform, new Color(0.12f, 0.12f, 0.12f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(350, -100), new Vector2(350, 280));
        Button btnOpenMonitor = monitorBodyObj.AddComponent<Button>();

        // Ободок экрана
        CreatePanel("Bezel", monitorBodyObj.transform, new Color(0.05f, 0.05f, 0.05f), new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
        
        // Сам экран (зеленый)
        GameObject monitorScreenObj = CreatePanel("MonitorScreen", monitorBodyObj.transform, new Color(0.02f, 0.12f, 0.02f), new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.92f), Vector2.zero, Vector2.zero);
        
        // Scanlines для атмосферы ЭЛТ
        for (int i = 0; i < 10; i++) {
            float yPos = i * 0.1f;
            CreatePanel("Scanline", monitorScreenObj.transform, new Color(0, 0, 0, 0.2f), new Vector2(0, yPos), new Vector2(1, yPos + 0.02f), Vector2.zero, Vector2.zero);
        }

        // Мигающий текст
        GameObject blinkTextObj = CreateText("BlinkText", monitorScreenObj.transform, "> СИСТЕМА\nАКТИВНА_", 18, new Color(0.2f, 1f, 0.2f), Vector2.zero, Vector2.zero).gameObject;
        blinkTextObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        blinkTextObj.GetComponent<RectTransform>().anchorMax = Vector2.one;
        blinkTextObj.GetComponent<TextMeshProUGUI>().alignment = TextAlignmentOptions.Center;

        // Контент (появляется только когда монитор раскрыт на весь экран)
        GameObject contentPanelObj = CreatePanel("ContentPanel", monitorScreenObj.transform, new Color(0, 0, 0, 0), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Скрипт InteractiveMonitor — управляет анимацией
        InteractiveMonitor interMon = monitorBodyObj.AddComponent<InteractiveMonitor>();
        interMon.monitorRect = monitorBodyObj.GetComponent<RectTransform>();
        interMon.contentPanel = contentPanelObj;
        interMon.blinkText = blinkTextObj;

        // DatabaseFolderUI — управляет данными
        DatabaseFolderUI folderUI = monitorBodyObj.AddComponent<DatabaseFolderUI>();
        folderUI.folderPanel = contentPanelObj;

        // --- Контент внутри (занимает весь монитор через якоря) ---
        
        // Фото слева
        GameObject photoFrame = CreatePanel("PhotoFrame", contentPanelObj.transform, new Color(0.1f, 0.4f, 0.1f), new Vector2(0.03f, 0.2f), new Vector2(0.44f, 0.82f), Vector2.zero, Vector2.zero);
        GameObject photoObj = CreatePanel("Photo", photoFrame.transform, Color.black, new Vector2(0.03f, 0.03f), new Vector2(0.97f, 0.97f), Vector2.zero, Vector2.zero);
        folderUI.folderVisitorImage = photoObj.GetComponent<Image>();

        // Тексты справа
        TextMeshProUGUI fName = CreateText("FName", contentPanelObj.transform, "ИМЯ:", 36, Color.green, Vector2.zero, Vector2.zero);
        fName.rectTransform.anchorMin = new Vector2(0.48f, 0.65f); fName.rectTransform.anchorMax = new Vector2(0.97f, 0.82f);
        fName.alignment = TextAlignmentOptions.Left; folderUI.folderNameText = fName;

        TextMeshProUGUI fId = CreateText("FId", contentPanelObj.transform, "ID:", 36, Color.green, Vector2.zero, Vector2.zero);
        fId.rectTransform.anchorMin = new Vector2(0.48f, 0.48f); fId.rectTransform.anchorMax = new Vector2(0.97f, 0.63f);
        fId.alignment = TextAlignmentOptions.Left; folderUI.folderIdText = fId;

        TextMeshProUGUI fEyes = CreateText("FEyes", contentPanelObj.transform, "ГЛАЗА:", 36, Color.green, Vector2.zero, Vector2.zero);
        fEyes.rectTransform.anchorMin = new Vector2(0.48f, 0.3f); fEyes.rectTransform.anchorMax = new Vector2(0.97f, 0.45f);
        fEyes.alignment = TextAlignmentOptions.Left; folderUI.folderEyesText = fEyes;

        // Счетчик страниц (вверху)
        TextMeshProUGUI fPage = CreateText("FPage", contentPanelObj.transform, "ЗАПИСЬ 1 ИЗ 1", 28, new Color(0.5f, 1f, 0.5f), Vector2.zero, Vector2.zero);
        fPage.rectTransform.anchorMin = new Vector2(0.1f, 0.88f); fPage.rectTransform.anchorMax = new Vector2(0.9f, 0.98f);
        fPage.alignment = TextAlignmentOptions.Center; folderUI.pageCounterText = fPage;

        // Кнопки << >> (внизу)
        GameObject btnPrevObj = CreatePanel("PrevBtn", contentPanelObj.transform, new Color(0.1f, 0.3f, 0.1f), new Vector2(0.03f, 0.05f), new Vector2(0.22f, 0.17f), Vector2.zero, Vector2.zero);
        Button btnPrev = btnPrevObj.AddComponent<Button>();
        TextMeshProUGUI tPrev = CreateText("Text", btnPrevObj.transform, "<<", 36, Color.white, Vector2.zero, Vector2.zero);
        tPrev.rectTransform.anchorMin = Vector2.zero; tPrev.rectTransform.anchorMax = Vector2.one;

        GameObject btnNextObj = CreatePanel("NextBtn", contentPanelObj.transform, new Color(0.1f, 0.3f, 0.1f), new Vector2(0.78f, 0.05f), new Vector2(0.97f, 0.17f), Vector2.zero, Vector2.zero);
        Button btnNext = btnNextObj.AddComponent<Button>();
        TextMeshProUGUI tNext = CreateText("Text", btnNextObj.transform, ">>", 36, Color.white, Vector2.zero, Vector2.zero);
        tNext.rectTransform.anchorMin = Vector2.zero; tNext.rectTransform.anchorMax = Vector2.one;

        // Кнопка ВЫКЛ (внизу по центру)
        GameObject btnCloseObj = CreatePanel("CloseBtn", contentPanelObj.transform, new Color(0.5f, 0.1f, 0.1f), new Vector2(0.37f, 0.05f), new Vector2(0.63f, 0.17f), Vector2.zero, Vector2.zero);
        Button btnClose = btnCloseObj.AddComponent<Button>();
        TextMeshProUGUI tClose = CreateText("Text", btnCloseObj.transform, "ВЫКЛ", 32, Color.white, Vector2.zero, Vector2.zero);
        tClose.rectTransform.anchorMin = Vector2.zero; tClose.rectTransform.anchorMax = Vector2.one;

        // Привязка событий
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnOpenMonitor.onClick, new UnityEngine.Events.UnityAction(interMon.OnMonitorClicked));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnClose.onClick, new UnityEngine.Events.UnityAction(interMon.OnCloseClicked));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnNext.onClick, new UnityEngine.Events.UnityAction(folderUI.NextPage));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPrev.onClick, new UnityEngine.Events.UnityAction(folderUI.PrevPage));
        
        // Скрываем контент в редакторе
        contentPanelObj.SetActive(false);

        // --- ЭКРАНЫ ЗАВЕРШЕНИЯ ИГРЫ И ПАУЗА ---
        // 0. Кнопка Паузы во время игры (Справа сверху)
        GameObject btnPauseObj = CreatePanel("PauseBtn", canvasObj.transform, new Color(0.1f, 0.1f, 0.1f, 0.8f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-120, -50), new Vector2(200, 60));
        Button btnPause = btnPauseObj.AddComponent<Button>();
        CreateText("Text", btnPauseObj.transform, "ПАУЗА", 24, Color.white, Vector2.zero, new Vector2(200, 60));

        // 0.5. Экран Паузы
        GameObject pausePanelObj = CreatePanel("PausePanel", canvasObj.transform, new Color(0, 0, 0, 0.85f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CreateText("PauseTitle", pausePanelObj.transform, "ИГРА ПРИОСТАНОВЛЕНА", 70, Color.white, new Vector2(0, 200), new Vector2(800, 150));
        
        GameObject btnResumeObj = CreatePanel("ResumeBtn", pausePanelObj.transform, new Color(0.2f, 0.5f, 0.2f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 75), new Vector2(400, 80));
        Button btnResume = btnResumeObj.AddComponent<Button>();
        CreateText("Text", btnResumeObj.transform, "ПРОДОЛЖИТЬ ИГРУ", 30, Color.white, Vector2.zero, new Vector2(400, 80));

        GameObject btnSettingsObj = CreatePanel("SettingsBtn", pausePanelObj.transform, new Color(0.2f, 0.2f, 0.2f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -25), new Vector2(400, 80));
        Button btnSettings = btnSettingsObj.AddComponent<Button>();
        CreateText("Text", btnSettingsObj.transform, "НАСТРОЙКИ", 30, Color.white, Vector2.zero, new Vector2(400, 80));

        GameObject btnPauseExitObj = CreatePanel("ExitBtn", pausePanelObj.transform, new Color(0.5f, 0.2f, 0.2f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -125), new Vector2(400, 80));
        Button btnPauseExit = btnPauseExitObj.AddComponent<Button>();
        CreateText("Text", btnPauseExitObj.transform, "В ГЛАВНОЕ МЕНЮ", 30, Color.white, Vector2.zero, new Vector2(400, 80));
        pausePanelObj.SetActive(false);

        // ===================================================
        // ПРОЦЕДУРНОЕ СТРОЕНИЕ ПАНЕЛИ НАСТРОЕК (SettingsPanel)
        // ===================================================
        GameObject settingsPanelObj = CreatePanel("SettingsPanel", canvasObj.transform, new Color(0.01f, 0.05f, 0.01f, 0.98f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        settingsPanelObj.transform.SetAsLastSibling();
        
        Color neonGreen = new Color(0.2f, 1f, 0.2f, 1f);

        // Рамка панели настроек
        // Верхняя
        GameObject sLineT = new GameObject("LineT"); sLineT.transform.SetParent(settingsPanelObj.transform, false);
        sLineT.AddComponent<Image>().color = neonGreen;
        RectTransform srT = sLineT.GetComponent<RectTransform>(); srT.anchorMin = new Vector2(0, 1); srT.anchorMax = new Vector2(1, 1); srT.anchoredPosition = Vector2.zero; srT.sizeDelta = new Vector2(0, 10);
        // Нижняя
        GameObject sLineB = new GameObject("LineB"); sLineB.transform.SetParent(settingsPanelObj.transform, false);
        sLineB.AddComponent<Image>().color = neonGreen;
        RectTransform srB = sLineB.GetComponent<RectTransform>(); srB.anchorMin = new Vector2(0, 0); srB.anchorMax = new Vector2(1, 0); srB.anchoredPosition = Vector2.zero; srB.sizeDelta = new Vector2(0, 10);
        // Левая
        GameObject sLineL = new GameObject("LineL"); sLineL.transform.SetParent(settingsPanelObj.transform, false);
        sLineL.AddComponent<Image>().color = neonGreen;
        RectTransform srL = sLineL.GetComponent<RectTransform>(); srL.anchorMin = new Vector2(0, 0); srL.anchorMax = new Vector2(0, 1); srL.anchoredPosition = Vector2.zero; srL.sizeDelta = new Vector2(10, 0);
        // Правая
        GameObject sLineR = new GameObject("LineR"); sLineR.transform.SetParent(settingsPanelObj.transform, false);
        sLineR.AddComponent<Image>().color = neonGreen;
        RectTransform srR = sLineR.GetComponent<RectTransform>(); srR.anchorMin = new Vector2(1, 0); srR.anchorMax = new Vector2(1, 1); srR.anchoredPosition = Vector2.zero; srR.sizeDelta = new Vector2(10, 0);

        // Заголовок настроек
        CreateText("SettingsTitle", settingsPanelObj.transform, "НАСТРОЙКИ ЗВУКА", 70, neonGreen, new Vector2(0, 350), new Vector2(800, 150));

        // Вспомогательная функция для создания слайдеров
        System.Func<string, Vector2, Slider> CreateVolumeSlider = (labelText, anchoredPos) =>
        {
            GameObject container = new GameObject(labelText + "_SliderContainer");
            container.transform.SetParent(settingsPanelObj.transform, false);
            RectTransform contRt = container.AddComponent<RectTransform>();
            contRt.anchorMin = new Vector2(0.5f, 0.5f);
            contRt.anchorMax = new Vector2(0.5f, 0.5f);
            contRt.anchoredPosition = anchoredPos;
            contRt.sizeDelta = new Vector2(800, 200);

            // Текстовая подпись
            CreateText(labelText + "_Label", container.transform, labelText, 36, neonGreen, new Vector2(0, 60), new Vector2(800, 50));

            // Сам объект Слайдера
            GameObject sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(container.transform, false);
            RectTransform slRt = sliderObj.AddComponent<RectTransform>();
            slRt.anchorMin = new Vector2(0.5f, 0.5f);
            slRt.anchorMax = new Vector2(0.5f, 0.5f);
            slRt.anchoredPosition = new Vector2(0, -20);
            slRt.sizeDelta = new Vector2(600, 40);

            Slider slider = sliderObj.AddComponent<Slider>();

            // Background слайдера
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(sliderObj.transform, false);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0f, 0.2f, 0f, 1f); // Темный зеленый
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
            fillImg.color = neonGreen; // Яркий неоновый зеленый для заполненной части
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
        };

        // Создаем слайдеры громкости музыки и SFX
        Slider musicSld = CreateVolumeSlider("МУЗЫКА / ФОН", new Vector2(0, 100));
        Slider sfxSld = CreateVolumeSlider("ЭФФЕКТЫ / SFX", new Vector2(0, -100));

        // Кнопка НАЗАД настроек (в стиле паузы)
        GameObject btnBackObj = CreatePanel("BackBtn", settingsPanelObj.transform, new Color(0.2f, 0.2f, 0.2f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -300), new Vector2(400, 80));
        Button btnBack = btnBackObj.AddComponent<Button>();
        CreateText("Text", btnBackObj.transform, "НАЗАД", 30, Color.white, Vector2.zero, new Vector2(400, 80));

        settingsPanelObj.SetActive(false); // Скрыта по умолчанию

        // 1. Панель Победы
        GameObject victoryPanelObj = CreatePanel("VictoryPanel", canvasObj.transform, new Color(0, 0, 0, 0.9f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CreateText("VictoryTitle", victoryPanelObj.transform, "СМЕНА ОКОНЧЕНА", 80, new Color(0.2f, 0.8f, 0.2f), new Vector2(0, 200), new Vector2(800, 150));
        TextMeshProUGUI victoryStats = CreateText("VictoryStats", victoryPanelObj.transform, "Вы отлично справились.", 40, Color.white, new Vector2(0, 50), new Vector2(800, 100));
        
        GameObject btnContinueShiftObj = CreatePanel("NextShiftBtn", victoryPanelObj.transform, new Color(0.2f, 0.2f, 0.2f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220, -150), new Vector2(400, 80));
        Button btnContinueShift = btnContinueShiftObj.AddComponent<Button>();
        CreateText("Text", btnContinueShiftObj.transform, "ПРОДОЛЖИТЬ СМЕНУ", 30, Color.white, Vector2.zero, new Vector2(400, 80));

        GameObject btnVictoryExitObj = CreatePanel("ExitBtn", victoryPanelObj.transform, new Color(0.2f, 0.2f, 0.2f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(220, -150), new Vector2(400, 80));
        Button btnVictoryExit = btnVictoryExitObj.AddComponent<Button>();
        CreateText("Text", btnVictoryExitObj.transform, "В ГЛАВНОЕ МЕНЮ", 30, Color.white, Vector2.zero, new Vector2(400, 80));
        
        victoryPanelObj.SetActive(false);

        // 2. Панель Поражения
        GameObject gameOverPanelObj = CreatePanel("GameOverPanel", canvasObj.transform, new Color(0, 0, 0, 0.95f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CreateText("GameOverTitle", gameOverPanelObj.transform, "ВЫ УВОЛЕНЫ", 100, Color.red, new Vector2(0, 200), new Vector2(800, 150));
        TextMeshProUGUI gameOverReason = CreateText("GameOverReason", gameOverPanelObj.transform, "Слишком много ошибок.", 40, Color.white, new Vector2(0, 50), new Vector2(800, 100));
        
        GameObject btnGameOverExitObj = CreatePanel("ExitBtn", gameOverPanelObj.transform, new Color(0.2f, 0.2f, 0.2f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(400, 80));
        Button btnGameOverExit = btnGameOverExitObj.AddComponent<Button>();
        CreateText("Text", btnGameOverExitObj.transform, "В ГЛАВНОЕ МЕНЮ", 30, Color.white, Vector2.zero, new Vector2(400, 80));
        
        gameOverPanelObj.SetActive(false);

        // --- СОБИРАЕМ GAMEMANAGER ---
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();
        
        gm.visitorImageDisplay = visitorImg;
        gm.windowShutter = shutterObj.GetComponent<RectTransform>();
        gm.documentTray = trayObj.GetComponent<RectTransform>();
        gm.guardLeft = guardLeftObj.GetComponent<RectTransform>();
        gm.guardRight = guardRightObj.GetComponent<RectTransform>();
        
        gm.passportNameText = pName;
        gm.passportIdText = pId;
        gm.passportEyesText = pEyes;
        gm.dossierNameText = dName;
        gm.dossierIdText = dId;
        gm.dossierEyesText = dEyes;
        
        gm.databaseFolder = folderUI;
        gm.pausePanel = pausePanelObj;
        gm.victoryPanel = victoryPanelObj;
        gm.victoryStatsText = victoryStats;
        gm.gameOverPanel = gameOverPanelObj;
        gm.gameOverReasonText = gameOverReason;
        gm.settingsPanel = settingsPanelObj;
        gm.musicSlider = musicSld;
        gm.sfxSlider = sfxSld;

        // --- ДИАЛОГИ (АНИМЕ СТИЛЬ) ---
        GameObject diagPanel = CreatePanel("DialoguePanel", canvasObj.transform, new Color(0.05f, 0.05f, 0.08f, 0.9f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(1200, 250));
        gm.dialoguePanel = diagPanel;
        
        // Рамка портрета
        GameObject portraitObj = CreatePanel("Portrait", diagPanel.transform, Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(130, 0), new Vector2(200, 200));
        gm.dialoguePortrait = portraitObj.GetComponent<Image>();
        
        // Имя
        GameObject nameLabel = CreatePanel("NameLabel", diagPanel.transform, new Color(0.2f, 0.2f, 0.3f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(100, 0), new Vector2(200, 50));
        gm.dialogueNameText = CreateText("NameText", nameLabel.transform, "ИМЯ", 24, Color.yellow, Vector2.zero, new Vector2(200, 50));
        
        // Текст диалога
        gm.dialogueContentText = CreateText("DialogueContent", diagPanel.transform, "...", 32, Color.white, new Vector2(150, -20), new Vector2(800, 150));
        gm.dialogueContentText.alignment = TextAlignmentOptions.TopLeft;

        if (savedShifts != null) gm.shiftsDatabase = savedShifts;

        // --- НАСТРОЙКА ЗВУКОВ (АВТОМАТИЧЕСКИ) ---
        GameObject bgAudioObj = new GameObject("BackgroundAudio");
        bgAudioObj.transform.SetParent(gmObj.transform);
        bgAudioObj.AddComponent<AudioSource>();
        bgAudioObj.AddComponent<DystopianAmbience>();

        GameObject sfxAudioObj = new GameObject("SFXAudio");
        sfxAudioObj.transform.SetParent(gmObj.transform);
        AudioSource sfxSource = sfxAudioObj.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        gm.sfxAudioSource = sfxSource;
        if (oldCloseSound != null) gm.shutterCloseSound = oldCloseSound;
        if (oldOpenSound != null) gm.shutterOpenSound = oldOpenSound;

        // Привязываем кнопки к GameManager
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnApprove.onClick, new UnityEngine.Events.UnityAction(gm.OnApproveClicked));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnReject.onClick, new UnityEngine.Events.UnityAction(gm.OnRejectClicked));

        // Привязываем кнопки паузы
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPause.onClick, new UnityEngine.Events.UnityAction(gm.TogglePause));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnResume.onClick, new UnityEngine.Events.UnityAction(gm.TogglePause));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnSettings.onClick, new UnityEngine.Events.UnityAction(gm.OpenSettings));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnPauseExit.onClick, new UnityEngine.Events.UnityAction(gm.ReturnToMainMenu));

        // Привязываем кнопки настроек
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnBack.onClick, new UnityEngine.Events.UnityAction(gm.CloseSettings));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(musicSld.onValueChanged, new UnityEngine.Events.UnityAction<float>(gm.OnMusicVolumeChanged));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(sfxSld.onValueChanged, new UnityEngine.Events.UnityAction<float>(gm.OnSFXVolumeChanged));

        // Привязываем кнопки завершения к GameManager
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnContinueShift.onClick, new UnityEngine.Events.UnityAction(gm.LoadNextShift));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnVictoryExit.onClick, new UnityEngine.Events.UnityAction(gm.ReturnToMainMenu));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(btnGameOverExit.onClick, new UnityEngine.Events.UnityAction(gm.ReturnToMainMenu));

        // --- ФИНАЛЬНЫЙ ШТРИХ: CRT ЭФФЕКТ (URP SAFE) ---
        GameObject crtOverlay = CreatePanel("CRT_Overlay_Safe", canvasObj.transform, new Color(1,1,1,0.2f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        crtOverlay.GetComponent<Image>().raycastTarget = false; 
        
        Shader crtShader = Shader.Find("UI/CRT_URP_Safe");
        if (crtShader != null)
        {
            crtOverlay.GetComponent<Image>().material = new Material(crtShader);
        }

        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorUtility.DisplayDialog("Успех", "Интерфейс собран! Добавлен БЕЗОПАСНЫЙ CRT-эффект для URP.\n\nТеперь никаких ошибок быть не должно.", "Понял");
    }
}
