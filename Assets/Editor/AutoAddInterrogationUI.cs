using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

[InitializeOnLoad]
public class AutoAddInterrogationUI
{
    static AutoAddInterrogationUI()
    {
        EditorApplication.delayCall += RunOnce;
    }

    [MenuItem("Parallax/Добавить Допрос и Вопросы")]
    public static void ManualRun()
    {
        EditorPrefs.DeleteKey("AutoAddInterrogationUI_Active_v2");
        RunOnce();
    }

    public static void RunOnce()
    {
        if (Application.isPlaying) return;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameScene") return;

        // Исключаем повторные авто-запуски, разрешая ручной запуск через меню
        if (EditorPrefs.GetBool("AutoAddInterrogationUI_Active_v2", false)) return;
        EditorPrefs.SetBool("AutoAddInterrogationUI_Active_v2", true);

        Debug.Log("<color=cyan>[Antigravity] Восстанавливаю функционал Допроса и Вопросов...</color>");

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[Antigravity] Canvas не найден на сцене!");
            return;
        }

        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogWarning("[Antigravity] GameManager не найден на сцене!");
            return;
        }

        // Загружаем премиальный ретро-пиксельный шрифт VT323 из ресурсов проекта
        TMP_FontAsset vt323Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Visitors/VT323-Regular SDF.asset");
        if (vt323Font == null)
        {
            Debug.LogWarning("[Antigravity] Шрифт VT323-Regular SDF не найден по пути Assets/Visitors/VT323-Regular SDF.asset!");
        }

        // ==========================================
        // 1. НАСТРОЙКА КЛИКАБЕЛЬНОСТИ ПЕРСОНАЖА (VisitorImage)
        // ==========================================
        // Также ищем и удаляем любые старые/лишние кнопки допроса/вопросов на сцене по всем возможным именам
        string[] namesToDestroy = new string[] {
            "InterrogateBtn", "InterrogateButton", "QuestionBtn", "QuestionButton", 
            "InterrogationBtn", "InterrogationButton", "Вопрос", "Допрос", 
            "КнопкаВопроса", "КнопкаДопроса", "Question"
        };
        
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            foreach (string nameToDestroy in namesToDestroy)
            {
                if (obj != null && obj.name.Equals(nameToDestroy, System.StringComparison.OrdinalIgnoreCase))
                {
                    Object.DestroyImmediate(obj);
                    break;
                }
            }
        }

        Button interrogateBtnComp = null;
        if (gm.visitorImageDisplay != null)
        {
            // Убеждаемся, что спрайт персонажа принимает клики
            gm.visitorImageDisplay.raycastTarget = true;

            interrogateBtnComp = gm.visitorImageDisplay.gameObject.GetComponent<Button>();
            if (interrogateBtnComp == null)
            {
                interrogateBtnComp = gm.visitorImageDisplay.gameObject.AddComponent<Button>();
            }

            // Отключаем визуальные переходы кнопки (чтобы персонаж не менял цвет/размер при наведении/нажатии)
            interrogateBtnComp.transition = Selectable.Transition.None;
            
            // Очищаем и добавляем слушатель
            interrogateBtnComp.onClick.RemoveAllListeners();
            interrogateBtnComp.onClick.AddListener(gm.OnInterrogateClicked);
            
            Debug.Log("[Antigravity] Спрайт посетителя (VisitorImage) успешно настроен как кнопка допроса!");
        }
        else
        {
            Debug.LogWarning("[Antigravity] visitorImageDisplay не найден в GameManager!");
        }

        // ==========================================
        // 2. СОЗДАНИЕ ПАНЕЛИ ВОПРОСОВ (QuestionsPanel)
        // ==========================================
        Transform existingPanel = canvas.transform.Find("QuestionsPanel");
        if (existingPanel != null)
        {
            Object.DestroyImmediate(existingPanel.gameObject);
        }

        GameObject panelObj = new GameObject("QuestionsPanel");
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        
        // Размещаем в левой свободной зоне (слева от окна посетителя)
        panelRt.anchoredPosition = new Vector2(-600, 150);
        panelRt.sizeDelta = new Vector2(360, 320);

        // Роскошный ЭЛТ-дизайн панели (темно-зеленый с неоновой границей)
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0.04f, 0.06f, 0.04f, 0.98f); // Глубокий ЭЛТ-зеленый

        Outline panelOutline = panelObj.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0f, 0.7f, 0.2f, 0.8f); // Неоново-зеленое свечение
        panelOutline.effectDistance = new Vector2(3f, -3f);

        // Заголовок панели "ДОПРОС: ВОПРОСЫ"
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);

        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0, -15);
        titleRt.sizeDelta = new Vector2(-20, 40);

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "СИСТЕМА ДОПРОСА";
        if (vt323Font != null) titleText.font = vt323Font;
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0f, 0.9f, 0.3f, 1f); // Люминофорно-зеленый

        // Линия разделения
        GameObject lineObj = new GameObject("DividerLine");
        lineObj.transform.SetParent(panelObj.transform, false);
        RectTransform lineRt = lineObj.AddComponent<RectTransform>();
        lineRt.anchorMin = new Vector2(0.05f, 1f);
        lineRt.anchorMax = new Vector2(0.95f, 1f);
        lineRt.anchoredPosition = new Vector2(0, -50);
        lineRt.sizeDelta = new Vector2(0, 2);
        Image lineImg = lineObj.AddComponent<Image>();
        lineImg.color = new Color(0f, 0.7f, 0.2f, 0.4f);

        // Контейнер для кнопок
        GameObject containerObj = new GameObject("ButtonsContainer");
        containerObj.transform.SetParent(panelObj.transform, false);

        RectTransform containerRt = containerObj.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0, 0);
        containerRt.anchorMax = new Vector2(1, 1);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.offsetMin = new Vector2(15, 15);
        containerRt.offsetMax = new Vector2(-15, -60); // Отступ сверху под заголовок

        VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        // Создание трех вопросов
        string[] questionTexts = new string[]
        {
            "1. Несовпадение Имени / ID",
            "2. Несоответствие Глаз",
            "3. Истек Срок Паспорта"
        };

        string[] btnNames = new string[]
        {
            "NameQuestionBtn",
            "EyesQuestionBtn",
            "DateQuestionBtn"
        };

        for (int i = 0; i < 3; i++)
        {
            GameObject btnObj = new GameObject(btnNames[i]);
            btnObj.transform.SetParent(containerObj.transform, false);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.08f, 0.12f, 0.08f, 1f); // Темно-зеленая кнопка

            Outline btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0f, 0.5f, 0.15f, 0.6f);
            btnOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Button b = btnObj.AddComponent<Button>();

            // Текст вопроса
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);

            RectTransform txtRt = txtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = new Vector2(10, 0);
            txtRt.offsetMax = new Vector2(-10, 0);

            TextMeshProUGUI txtTmp = txtObj.AddComponent<TextMeshProUGUI>();
            txtTmp.text = questionTexts[i];
            if (vt323Font != null) txtTmp.font = vt323Font;
            txtTmp.fontSize = 22;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.alignment = TextAlignmentOptions.Center;
            txtTmp.color = new Color(0.8f, 1f, 0.8f, 1f); // Светло-зеленый
            txtTmp.textWrappingMode = TextWrappingModes.Normal;
        }

        // ==========================================
        // 3. ПРИВЯЗКА К GAMEMANAGER И НАСТРОЙКА
        // ==========================================
        gm.interrogateBtn = interrogateBtnComp;
        gm.questionsPanel = panelObj;

        // Добавляем вызов методов при клике
        if (interrogateBtnComp != null)
        {
            interrogateBtnComp.onClick.RemoveAllListeners();
            interrogateBtnComp.onClick.AddListener(gm.OnInterrogateClicked);
        }

        Button[] qBtns = panelObj.GetComponentsInChildren<Button>(true);
        for (int i = 0; i < qBtns.Length; i++)
        {
            int index = i; // capture variable
            qBtns[i].onClick.RemoveAllListeners();
            qBtns[i].onClick.AddListener(() => gm.AskQuestion(index));
        }

        // По умолчанию панель скрыта
        panelObj.SetActive(false);

        EditorUtility.SetDirty(gm);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>[Antigravity] Кнопка ДОПРОС и Панель Вопросов успешно добавлены и подключены!</color>");
    }
}
