using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UIBuilder : EditorWindow
{
    [MenuItem("Parallax/Собрать интерфейс автоматически")]
    public static void BuildUI()
    {
        // 1. Создаем Холст (Canvas)
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. Создаем EventSystem (нужен для кнопок)
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        // 3. Создаем Менеджер Игры
        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        // 4. Задний фон
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.15f, 0.15f, 0.15f); // Темно-серый
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 5. Место под фото посетителя
        GameObject visitorImgObj = new GameObject("VisitorImage");
        visitorImgObj.transform.SetParent(canvasObj.transform, false);
        Image visitorImg = visitorImgObj.AddComponent<Image>();
        RectTransform vImgRect = visitorImgObj.GetComponent<RectTransform>();
        vImgRect.anchorMin = new Vector2(0.2f, 0.3f);
        vImgRect.anchorMax = new Vector2(0.4f, 0.7f);
        vImgRect.offsetMin = Vector2.zero;
        vImgRect.offsetMax = Vector2.zero;
        gm.visitorImageDisplay = visitorImg;

        // Вспомогательная функция для текстов
        TextMeshProUGUI CreateText(string name, Vector2 anchorMin, Vector2 anchorMax, string defaultText, int fontSize)
        {
            GameObject txtObj = new GameObject(name);
            txtObj.transform.SetParent(canvasObj.transform, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = defaultText;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform rect = txtObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return tmp;
        }

        // 6. Тексты досье
        gm.dossierNameText = CreateText("NameText", new Vector2(0.6f, 0.65f), new Vector2(0.9f, 0.75f), "Имя:", 40);
        gm.dossierIdText = CreateText("IDText", new Vector2(0.6f, 0.55f), new Vector2(0.9f, 0.65f), "ID:", 40);
        gm.dossierEyesText = CreateText("EyesText", new Vector2(0.6f, 0.45f), new Vector2(0.9f, 0.55f), "Глаза:", 40);
        
        // 7. Тексты механик
        gm.quotaTextDisplay = CreateText("QuotaText", new Vector2(0.4f, 0.85f), new Vector2(0.6f, 0.95f), "Очередь", 50);
        gm.strikesTextDisplay = CreateText("StrikesText", new Vector2(0.05f, 0.85f), new Vector2(0.25f, 0.95f), "Штрафы", 50);

        // Вспомогательная функция для кнопок
        Button CreateButton(string name, Vector2 anchorMin, Vector2 anchorMax, string btnText, Color color)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(canvasObj.transform, false);
            Image img = btnObj.AddComponent<Image>();
            img.color = color;
            Button btn = btnObj.AddComponent<Button>();
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // Текст внутри кнопки
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = btnText;
            tmp.fontSize = 35;
            tmp.color = Color.black;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform txtRect = txtObj.GetComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = Vector2.zero;
            txtRect.offsetMax = Vector2.zero;
            
            return btn;
        }

        // 8. Кнопки
        Button approveBtn = CreateButton("ApproveButton", new Vector2(0.3f, 0.1f), new Vector2(0.45f, 0.2f), "ПРОПУСТИТЬ", new Color(0.3f, 0.8f, 0.3f));
        Button rejectBtn = CreateButton("RejectButton", new Vector2(0.55f, 0.1f), new Vector2(0.7f, 0.2f), "ЗАДЕРЖАТЬ", new Color(0.8f, 0.3f, 0.3f));

        // 9. Связываем кнопки со скриптом
        UnityEditor.Events.UnityEventTools.AddPersistentListener(approveBtn.onClick, new UnityEngine.Events.UnityAction(gm.OnApproveClicked));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(rejectBtn.onClick, new UnityEngine.Events.UnityAction(gm.OnRejectClicked));

        // Регистрируем изменения, чтобы можно было нажать Ctrl+Z
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create UI Canvas");
        Undo.RegisterCreatedObjectUndo(gmObj, "Create GameManager");

        // Выводим сообщение об успехе
        EditorUtility.DisplayDialog("Успех!", "Интерфейс и GameManager успешно собраны и связаны!\nОсталось только перетащить досье в массив GameManager.", "ОК");
    }
}
