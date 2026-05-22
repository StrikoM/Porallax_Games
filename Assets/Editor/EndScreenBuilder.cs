using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class EndScreenBuilder : EditorWindow
{
    [MenuItem("Parallax/Собрать Экраны Конца Игры")]
    public static void BuildEndScreens()
    {
        // 1. Находим Canvas и GameManager
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        GameManager gm = Object.FindAnyObjectByType<GameManager>();

        if (canvas == null || gm == null)
        {
            EditorUtility.DisplayDialog("Ошибка", "Не найден Canvas или GameManager в сцене! Сначала соберите основной интерфейс.", "ОК");
            return;
        }

        // Вспомогательная функция текста
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

        // Вспомогательная функция кнопок
        Button CreateButton(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, string btnText, Color btnColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            Image img = btnObj.AddComponent<Image>();
            img.color = btnColor;
            
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2, -2);

            Button btn = btnObj.AddComponent<Button>();
            
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            CreateText(name + "_Text", btnObj.transform, Vector2.zero, Vector2.one, btnText, 45, Color.white);
            return btn;
        }

        // 2. СОЗДАЕМ ПАНЕЛЬ ПОРАЖЕНИЯ (КРАСНАЯ)
        GameObject goPanel = new GameObject("GameOverPanel");
        goPanel.transform.SetParent(canvas.transform, false);
        Image goImg = goPanel.AddComponent<Image>();
        goImg.color = new Color(0.3f, 0f, 0f, 0.95f); // Кроваво-красный прозрачный фон
        RectTransform goRect = goPanel.GetComponent<RectTransform>();
        goRect.anchorMin = Vector2.zero;
        goRect.anchorMax = Vector2.one;
        goRect.sizeDelta = Vector2.zero;
        
        CreateText("GOTitle", goPanel.transform, new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.9f), "ИГРА ОКОНЧЕНА", 120, Color.red);
        TextMeshProUGUI reasonTxt = CreateText("GOReason", goPanel.transform, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f), "Причина смерти", 60, Color.white);
        
        Button goMenuBtn = CreateButton("MenuBtn", goPanel.transform, new Vector2(0.35f, 0.15f), new Vector2(0.65f, 0.25f), "В ГЛАВНОЕ МЕНЮ", new Color(0.2f, 0.2f, 0.2f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(goMenuBtn.onClick, new UnityEngine.Events.UnityAction(gm.ReturnToMenu));

        goPanel.SetActive(false);

        // 3. СОЗДАЕМ ПАНЕЛЬ ПОБЕДЫ (ЗЕЛЕНАЯ)
        GameObject vicPanel = new GameObject("VictoryPanel");
        vicPanel.transform.SetParent(canvas.transform, false);
        Image vicImg = vicPanel.AddComponent<Image>();
        vicImg.color = new Color(0f, 0.2f, 0f, 0.95f); // Темно-зеленый фон
        RectTransform vicRect = vicPanel.GetComponent<RectTransform>();
        vicRect.anchorMin = Vector2.zero;
        vicRect.anchorMax = Vector2.one;
        vicRect.sizeDelta = Vector2.zero;
        
        CreateText("VicTitle", vicPanel.transform, new Vector2(0.1f, 0.7f), new Vector2(0.9f, 0.9f), "СМЕНА ЗАКРЫТА", 120, Color.green);
        TextMeshProUGUI statsTxt = CreateText("VicStats", vicPanel.transform, new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.6f), "Ошибок: 0", 60, Color.white);
        
        Button vicMenuBtn = CreateButton("MenuBtn", vicPanel.transform, new Vector2(0.35f, 0.15f), new Vector2(0.65f, 0.25f), "В ГЛАВНОЕ МЕНЮ", new Color(0.2f, 0.2f, 0.2f));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(vicMenuBtn.onClick, new UnityEngine.Events.UnityAction(gm.ReturnToMenu));

        vicPanel.SetActive(false);

        // 4. ПРИВЯЗЫВАЕМ К GAMEMANAGER
        gm.gameOverPanel = goPanel;
        gm.gameOverReasonText = reasonTxt;
        gm.victoryPanel = vicPanel;
        gm.victoryStatsText = statsTxt;

        Undo.RegisterCreatedObjectUndo(goPanel, "Create Game Over Panel");
        Undo.RegisterCreatedObjectUndo(vicPanel, "Create Victory Panel");
        EditorUtility.SetDirty(gm);

        EditorUtility.DisplayDialog("Готово!", "Экраны завершения игры (Победа и Поражение) успешно добавлены в Canvas и привязаны к GameManager!", "Отлично");
    }
}
