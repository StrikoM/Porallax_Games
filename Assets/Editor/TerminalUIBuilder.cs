using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class TerminalUIBuilder : EditorWindow
{
    [MenuItem("Parallax/Собрать Терминал (Новый Интерфейс с БД)")]
    public static void BuildTerminalUI()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        GameManager gm = Object.FindAnyObjectByType<GameManager>();

        if (canvas == null || gm == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Не найден Canvas или GameManager!", "ОК");
            return;
        }

        // Удаляем ВЕСЬ старый UI, чтобы собрать с нуля
        for (int i = canvas.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(canvas.transform.GetChild(i).gameObject);
        }

        // Делаем цвета более "грязными" и реалистичными, чтобы не резало глаза
        Color terminalGreen = new Color(0.15f, 0.7f, 0.2f); // Менее яркий зеленый
        Color terminalDark  = new Color(0.05f, 0.08f, 0.05f);
        Color terminalYellow = new Color(0.9f, 0.7f, 0.1f);
        Color terminalRed    = new Color(0.8f, 0.2f, 0.2f);

        // ── Фон ──────────────────────────────────────────────────────
        GameObject bgObj = new GameObject("TerminalBackground");
        bgObj.transform.SetParent(canvas.transform, false);
        bgObj.transform.SetAsFirstSibling();
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = terminalDark;
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // ── Вспомогательные функции ───────────────────────────────────
        GameObject CreateFrame(string name, Vector2 aMin, Vector2 aMax, string title)
        {
            GameObject frame = new GameObject(name);
            frame.transform.SetParent(canvas.transform, false);
            Image img = frame.AddComponent<Image>();
            img.color = new Color(0.02f, 0.04f, 0.02f, 0.9f); // Темный полупрозрачный фон вместо заливки

            Outline ol = frame.AddComponent<Outline>();
            ol.effectColor = terminalGreen;
            ol.effectDistance = new Vector2(2, -2); // Уменьшил толщину обводки, чтобы не баговало
            
            RectTransform rect = frame.GetComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(frame.transform, false);
            TextMeshProUGUI tmp = titleObj.AddComponent<TextMeshProUGUI>();
            tmp.text = "> " + title; tmp.fontSize = 28;
            tmp.color = terminalGreen; tmp.fontStyle = FontStyles.Bold;
            RectTransform tr = titleObj.GetComponent<RectTransform>();
            tr.anchorMin = new Vector2(0, 0.88f); tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(10, 0); tr.offsetMax = Vector2.zero;
            return frame;
        }

        TextMeshProUGUI CreateText(string name, Transform parent, Vector2 aMin, Vector2 aMax, Color color)
        {
            GameObject o = new GameObject(name);
            o.transform.SetParent(parent, false);
            TextMeshProUGUI tmp = o.AddComponent<TextMeshProUGUI>();
            tmp.text = "DATA..."; tmp.fontSize = 28; tmp.color = color;
            RectTransform rect = o.GetComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = new Vector2(15, 0); rect.offsetMax = Vector2.zero;
            return tmp;
        }

        Button CreateBtn(string name, Transform parent, Vector2 aMin, Vector2 aMax, string label, Color color, int fontSize = 34)
        {
            GameObject o = new GameObject(name);
            o.transform.SetParent(parent, false);
            Image img = o.AddComponent<Image>();
            img.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
            Outline ol = o.AddComponent<Outline>();
            ol.effectColor = color; ol.effectDistance = new Vector2(2, -2);
            Button btn = o.AddComponent<Button>();
            RectTransform rect = o.GetComponent<RectTransform>();
            rect.anchorMin = aMin; rect.anchorMax = aMax;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            GameObject to = new GameObject("Text");
            to.transform.SetParent(o.transform, false);
            TextMeshProUGUI tmp = to.AddComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.fontSize = fontSize; tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center; tmp.fontStyle = FontStyles.Bold;
            RectTransform tr = to.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = tr.offsetMax = Vector2.zero;
            return btn;
        }

        // ── 1. Камера (посетитель) ────────────────────────────────────
        GameObject camFrame = CreateFrame("CameraFeed", new Vector2(0.03f, 0.28f), new Vector2(0.38f, 0.82f), "CAM_01_FEED");
        GameObject visitorImgObj = new GameObject("VisitorImage");
        visitorImgObj.transform.SetParent(camFrame.transform, false);
        Image visitorImg = visitorImgObj.AddComponent<Image>();
        RectTransform vRect = visitorImgObj.GetComponent<RectTransform>();
        vRect.anchorMin = new Vector2(0.05f, 0.05f);
        vRect.anchorMax = new Vector2(0.95f, 0.85f);
        vRect.offsetMin = vRect.offsetMax = Vector2.zero;
        gm.visitorImageDisplay = visitorImg;

        // ── 2. Паспорт ───────────────────────────────────────────────
        GameObject passFrame = CreateFrame("PassportDoc", new Vector2(0.42f, 0.28f), new Vector2(0.75f, 0.82f), "ПАСПОРТ_ЖИЛЬЦА");
        gm.passportNameText = CreateText("PassName", passFrame.transform, new Vector2(0, 0.58f), new Vector2(1, 0.85f), terminalGreen);
        gm.passportIdText   = CreateText("PassID",   passFrame.transform, new Vector2(0, 0.33f), new Vector2(1, 0.58f), terminalGreen);
        gm.passportEyesText = CreateText("PassEyes", passFrame.transform, new Vector2(0, 0.08f), new Vector2(1, 0.33f), terminalGreen);

        gm.dossierNameText = null;
        gm.dossierIdText   = null;
        gm.dossierEyesText = null;

        // ── 3. Кнопка "ОТКРЫТЬ ПАПКУ ЖИЛЬЦОВ" ─────────────────────────
        Button openDbBtn = CreateBtn("OpenDatabaseButton", canvas.transform,
            new Vector2(0.78f, 0.28f), new Vector2(0.97f, 0.82f),
            "[ ОТКРЫТЬ\nПАПКУ\nЖИЛЬЦОВ ]", terminalYellow, 30);

        // ── 4. Статистика и директива ─────────────────────────────────
        gm.quotaTextDisplay = CreateText("Quota", canvas.transform, new Vector2(0.38f, 0.85f), new Vector2(0.62f, 0.95f), terminalGreen);
        gm.quotaTextDisplay.fontSize = 36;
        gm.quotaTextDisplay.alignment = TextAlignmentOptions.Center;

        gm.strikesTextDisplay = CreateText("Strikes", canvas.transform, new Vector2(0.03f, 0.85f), new Vector2(0.37f, 0.95f), terminalGreen);
        gm.strikesTextDisplay.fontSize = 36;

        gm.directiveTextDisplay = CreateText("Directive", canvas.transform, new Vector2(0.03f, 0.22f), new Vector2(0.97f, 0.27f), terminalYellow);
        gm.directiveTextDisplay.fontSize = 30;
        gm.directiveTextDisplay.alignment = TextAlignmentOptions.Center;

        // ── 5. Кнопки решения ─────────────────────────────────────────
        Button approveBtn = CreateBtn("ApproveButton", canvas.transform,
            new Vector2(0.1f, 0.04f), new Vector2(0.42f, 0.19f), "[ ПРОПУСТИТЬ ]", terminalGreen);
        Button rejectBtn = CreateBtn("RejectButton", canvas.transform,
            new Vector2(0.58f, 0.04f), new Vector2(0.9f, 0.19f), "[ ЗАДЕРЖАТЬ ]", terminalRed);

        UnityEditor.Events.UnityEventTools.AddPersistentListener(approveBtn.onClick, new UnityEngine.Events.UnityAction(gm.OnApproveClicked));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(rejectBtn.onClick, new UnityEngine.Events.UnityAction(gm.OnRejectClicked));

        // ── 6. Панель "Папка Базы Данных" (всплывающее окно) ─────────
        GameObject folderPanel = new GameObject("FolderPanel");
        folderPanel.transform.SetParent(canvas.transform, false);
        Image fpImg = folderPanel.AddComponent<Image>();
        fpImg.color = new Color(0.05f, 0.08f, 0.05f, 0.98f); 
        Outline fpOl = folderPanel.AddComponent<Outline>();
        fpOl.effectColor = terminalYellow; fpOl.effectDistance = new Vector2(3, -3);
        RectTransform fpRect = folderPanel.GetComponent<RectTransform>();
        fpRect.anchorMin = new Vector2(0.15f, 0.1f);
        fpRect.anchorMax = new Vector2(0.85f, 0.92f);
        fpRect.offsetMin = fpRect.offsetMax = Vector2.zero;

        GameObject fpTitle = new GameObject("FolderTitle");
        fpTitle.transform.SetParent(folderPanel.transform, false);
        TextMeshProUGUI fpTmp = fpTitle.AddComponent<TextMeshProUGUI>();
        fpTmp.text = "> ПАПКА ЖИЛЬЦОВ ДОМА";
        fpTmp.fontSize = 32; fpTmp.color = terminalYellow; fpTmp.fontStyle = FontStyles.Bold;
        fpTmp.alignment = TextAlignmentOptions.Center;
        RectTransform fpTitleRect = fpTitle.GetComponent<RectTransform>();
        fpTitleRect.anchorMin = new Vector2(0, 0.87f); fpTitleRect.anchorMax = Vector2.one;
        fpTitleRect.offsetMin = fpTitleRect.offsetMax = Vector2.zero;

        GameObject fpImgObj = new GameObject("FolderImage");
        fpImgObj.transform.SetParent(folderPanel.transform, false);
        Image fpPhoto = fpImgObj.AddComponent<Image>();
        RectTransform fpPhotoRect = fpImgObj.GetComponent<RectTransform>();
        fpPhotoRect.anchorMin = new Vector2(0.05f, 0.38f);
        fpPhotoRect.anchorMax = new Vector2(0.45f, 0.85f);
        fpPhotoRect.offsetMin = fpPhotoRect.offsetMax = Vector2.zero;

        TextMeshProUGUI fpName = CreateText("FolderName", folderPanel.transform, new Vector2(0.5f, 0.65f), new Vector2(0.98f, 0.87f), terminalGreen);
        TextMeshProUGUI fpId   = CreateText("FolderID",   folderPanel.transform, new Vector2(0.5f, 0.48f), new Vector2(0.98f, 0.65f), terminalGreen);
        TextMeshProUGUI fpEyes = CreateText("FolderEyes", folderPanel.transform, new Vector2(0.5f, 0.32f), new Vector2(0.98f, 0.48f), terminalGreen);

        GameObject fpCounter = new GameObject("FolderCounter");
        fpCounter.transform.SetParent(folderPanel.transform, false);
        TextMeshProUGUI fpCounterTmp = fpCounter.AddComponent<TextMeshProUGUI>();
        fpCounterTmp.text = "ЗАПИСЬ 1 ИЗ 1";
        fpCounterTmp.fontSize = 28; fpCounterTmp.color = terminalYellow;
        fpCounterTmp.alignment = TextAlignmentOptions.Center;
        RectTransform fpCountRect = fpCounter.GetComponent<RectTransform>();
        fpCountRect.anchorMin = new Vector2(0.1f, 0.2f); fpCountRect.anchorMax = new Vector2(0.9f, 0.32f);
        fpCountRect.offsetMin = fpCountRect.offsetMax = Vector2.zero;

        Button fpPrev = CreateBtn("FolderPrev", folderPanel.transform,
            new Vector2(0.05f, 0.06f), new Vector2(0.35f, 0.18f), "◄ НАЗАД", terminalGreen, 28);
        Button fpNext = CreateBtn("FolderNext", folderPanel.transform,
            new Vector2(0.65f, 0.06f), new Vector2(0.95f, 0.18f), "ВПЕРЕД ►", terminalGreen, 28);

        Button fpClose = CreateBtn("FolderClose", folderPanel.transform,
            new Vector2(0.35f, 0.06f), new Vector2(0.65f, 0.18f), "[ ЗАКРЫТЬ ]", terminalRed, 26);

        DatabaseFolderUI dbUI = folderPanel.AddComponent<DatabaseFolderUI>();
        dbUI.folderPanel       = folderPanel;
        dbUI.folderVisitorImage = fpPhoto;
        dbUI.folderNameText     = fpName;
        dbUI.folderIdText       = fpId;
        dbUI.folderEyesText     = fpEyes;
        dbUI.pageCounterText    = fpCounterTmp;
        dbUI.prevButton         = fpPrev;
        dbUI.nextButton         = fpNext;

        UnityEditor.Events.UnityEventTools.AddPersistentListener(fpPrev.onClick,  new UnityEngine.Events.UnityAction(dbUI.PrevPage));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(fpNext.onClick,  new UnityEngine.Events.UnityAction(dbUI.NextPage));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(fpClose.onClick, new UnityEngine.Events.UnityAction(dbUI.CloseFolder));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(openDbBtn.onClick, new UnityEngine.Events.UnityAction(dbUI.ToggleFolder));

        gm.databaseFolder = dbUI;
        folderPanel.SetActive(false);

        // ── 7. Экран Поражения ─────────────────────────────────────────
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);
        Image goImg = gameOverPanel.AddComponent<Image>();
        goImg.color = new Color(0.1f, 0.0f, 0.0f, 0.95f);
        RectTransform goRect = gameOverPanel.GetComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero; goRect.anchorMax = Vector2.one;
        goRect.offsetMin = goRect.offsetMax = Vector2.zero;

        TextMeshProUGUI goTitle = CreateText("Title", gameOverPanel.transform, new Vector2(0, 0.7f), new Vector2(1, 0.9f), terminalRed);
        goTitle.text = "СИСТЕМА ЗАБЛОКИРОВАНА";
        goTitle.alignment = TextAlignmentOptions.Center;
        goTitle.fontSize = 50;

        gm.gameOverReasonText = CreateText("Reason", gameOverPanel.transform, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.6f), Color.white);
        gm.gameOverReasonText.alignment = TextAlignmentOptions.Center;
        gm.gameOverReasonText.fontSize = 35;

        Button goMenuBtn = CreateBtn("MenuBtn", gameOverPanel.transform, new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.25f), "[ ГЛАВНОЕ МЕНЮ ]", terminalRed, 30);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenuBtn.onClick, new UnityEngine.Events.UnityAction(gm.ReturnToMenu));
        
        gm.gameOverPanel = gameOverPanel;
        gameOverPanel.SetActive(false);

        // ── 8. Экран Победы ───────────────────────────────────────────
        GameObject victoryPanel = new GameObject("VictoryPanel");
        victoryPanel.transform.SetParent(canvas.transform, false);
        Image vicImg = victoryPanel.AddComponent<Image>();
        vicImg.color = new Color(0.0f, 0.1f, 0.0f, 0.95f);
        RectTransform vicRect = victoryPanel.GetComponent<RectTransform>();
        vicRect.anchorMin = Vector2.zero; vicRect.anchorMax = Vector2.one;
        vicRect.offsetMin = vicRect.offsetMax = Vector2.zero;

        TextMeshProUGUI vicTitle = CreateText("Title", victoryPanel.transform, new Vector2(0, 0.7f), new Vector2(1, 0.9f), terminalGreen);
        vicTitle.text = "ДЕНЬ ПРОЙДЕН С УСПЕХОМ";
        vicTitle.alignment = TextAlignmentOptions.Center;
        vicTitle.fontSize = 50;

        gm.victoryStatsText = CreateText("Stats", victoryPanel.transform, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.6f), Color.white);
        gm.victoryStatsText.alignment = TextAlignmentOptions.Center;
        gm.victoryStatsText.fontSize = 35;

        Button vicMenuBtn = CreateBtn("MenuBtn", victoryPanel.transform, new Vector2(0.3f, 0.1f), new Vector2(0.7f, 0.25f), "[ ПРОДОЛЖИТЬ СМЕНУ ]", terminalGreen, 30);
        UnityEditor.Events.UnityEventTools.AddPersistentListener(vicMenuBtn.onClick, new UnityEngine.Events.UnityAction(gm.ReturnToMenu));

        gm.victoryPanel = victoryPanel;
        victoryPanel.SetActive(false);

        EditorUtility.SetDirty(gm);
        EditorUtility.DisplayDialog("Терминал обновлён",
            "Улучшенный интерфейс (темные цвета, экраны победы/поражения) готов!\n\nНе забудьте сохранить сцену.", "OK");
    }
}
