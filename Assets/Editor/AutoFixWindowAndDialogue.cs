using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

[InitializeOnLoad]
public class AutoFixWindowAndDialogue
{
    static AutoFixWindowAndDialogue()
    {
        EditorApplication.delayCall += () => RunOnce();
    }

    [MenuItem("Parallax/FIX WINDOW & DIALOGUE PANEL")]
    public static void ManualFix()
    {
        Debug.Log("[Antigravity] Ручной запуск исправления окна и диалога...");
        RunOnce(true);
    }

    public static void RunOnce(bool force = false)
    {
        if (Application.isPlaying) return;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameScene") return;

        // Дамп иерархии сцены для анализа
        try
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("=== SCENE HIERARCHY DUMP ===");
            foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                DumpGameObject(root, "", sb);
            }
            System.IO.File.WriteAllText("scene_hierarchy_dump.txt", sb.ToString());
            Debug.Log("[Antigravity] Hierarchy dumped successfully to scene_hierarchy_dump.txt");
        }
        catch (System.Exception e)
        {
            Debug.LogError("[Antigravity] Error dumping scene: " + e.Message);
        }

        if (!force && EditorPrefs.GetBool("AutoFixWindowAndDialogue_v15", false)) return;
        EditorPrefs.SetBool("AutoFixWindowAndDialogue_v15", true);

        Debug.Log("<color=cyan>[Antigravity] НАЧИНАЮ ПОЛНУЮ НАСТРОЙКУ ОКНА И ДИАЛОГОВОЙ ПАНЕЛИ...</color>");

        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("[Antigravity] Ошибка: Canvas не найден на сцене!");
            return;
        }

        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm == null)
        {
            Debug.LogWarning("[Antigravity] Предупреждение: GameManager не найден!");
        }

        // Загружаем премиальный ретро-пиксельный шрифт VT323 из ресурсов проекта
        TMP_FontAsset vt323Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Visitors/VT323-Regular SDF.asset");
        if (vt323Font == null)
        {
            Debug.LogWarning("[Antigravity] Предупреждение: Шрифт VT323-Regular SDF не найден по пути Assets/Visitors/VT323-Regular SDF.asset!");
        }

        // ==========================================
        // 1. ИСПРАВЛЕНИЕ ИЕРАРХИИ ОКНА И МАСКИРОВАНИЯ
        // ==========================================
        GameObject windowFrame = GameObject.Find("WindowFrame");
        if (windowFrame == null)
        {
            Debug.LogError("[Antigravity] Ошибка: WindowFrame не найден!");
            return;
        }

        // Убеждаемся, что WindowFrame лежит прямо в Canvas
        windowFrame.transform.SetParent(canvas.transform, false);

        GameObject outsideBg = GameObject.Find("OutsideBg");
        if (outsideBg == null)
        {
            // Если OutsideBg пропал, создаем его заново на Canvas
            outsideBg = new GameObject("OutsideBg");
            outsideBg.transform.SetParent(canvas.transform, false);
            outsideBg.AddComponent<Image>();
        }
        else
        {
            outsideBg.transform.SetParent(canvas.transform, false);
        }

        // Убеждаемся, что у OutsideBg есть Mask
        Mask mask = outsideBg.GetComponent<Mask>();
        if (mask == null) mask = outsideBg.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        Image outsideImg = outsideBg.GetComponent<Image>();
        if (outsideImg != null)
        {
            outsideImg.color = Color.white;
            // Пробуем восстановить исходную текстуру улицы, если она сбросилась
            string outsideImgPath = "Assets/Sprites/outside_bg.png"; // Альтернативные пути
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(outsideImgPath);
            if (s != null) outsideImg.sprite = s;
        }

        // Позиционируем WindowFrame прямо в Canvas
        RectTransform frameRt = windowFrame.GetComponent<RectTransform>();
        if (frameRt != null)
        {
            frameRt.anchorMin = new Vector2(0.5f, 0.5f);
            frameRt.anchorMax = new Vector2(0.5f, 0.5f);
            frameRt.pivot = new Vector2(0.5f, 0.5f);
            frameRt.anchoredPosition3D = new Vector3(0f, 150f, 0f); // Z = 0 обязательно!
            frameRt.sizeDelta = new Vector2(840f, 640f);
            frameRt.localScale = Vector3.one;
        }

        // Позиционируем OutsideBg (Mask) прямо в Canvas (точно сопоставлен с WindowFrame)
        RectTransform outsideRt = outsideBg.GetComponent<RectTransform>();
        if (outsideRt != null)
        {
            outsideRt.anchorMin = new Vector2(0.5f, 0.5f);
            outsideRt.anchorMax = new Vector2(0.5f, 0.5f);
            outsideRt.pivot = new Vector2(0.5f, 0.5f);
            outsideRt.anchoredPosition3D = new Vector3(0f, 150f, 0f); // Z = 0 обязательно!
            outsideRt.sizeDelta = new Vector2(800f, 600f);
            outsideRt.localScale = Vector3.one;
        }

        // Перемещаем всех персонажей и шторку внутрь OutsideBg (Маски)
        GameObject visitor = GameObject.Find("VisitorImage");
        GameObject guardLeft = GameObject.Find("GuardLeft");
        GameObject guardRight = GameObject.Find("GuardRight");
        GameObject shutter = GameObject.Find("WindowShutter");

        if (visitor != null) visitor.transform.SetParent(outsideBg.transform, false);
        if (guardLeft != null) guardLeft.transform.SetParent(outsideBg.transform, false);
        if (guardRight != null) guardRight.transform.SetParent(outsideBg.transform, false);
        if (shutter != null) shutter.transform.SetParent(outsideBg.transform, false);

        // Настраиваем VisitorImage
        if (visitor != null)
        {
            RectTransform rt = visitor.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition3D = new Vector3(0f, -50f, 0f); // Z = 0 обязательно!
                rt.sizeDelta = new Vector2(400f, 600f);
                rt.localScale = Vector3.one;
            }
        }

        // Настраиваем GuardLeft
        if (guardLeft != null)
        {
            RectTransform rt = guardLeft.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition3D = new Vector3(-600f, -50f, 0f);
                rt.sizeDelta = new Vector2(250f, 600f);
                rt.localScale = Vector3.one;
            }
        }

        // Настраиваем GuardRight
        if (guardRight != null)
        {
            RectTransform rt = guardRight.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition3D = new Vector3(600f, -50f, 0f);
                rt.sizeDelta = new Vector2(250f, 600f);
                rt.localScale = Vector3.one;
            }
        }

        // ==========================================
        // 2. НАСТРОЙКА ПРЕМИАЛЬНОЙ ДВЕРИ-ШТОРКИ (WindowShutter)
        // ==========================================
        if (shutter != null)
        {
            shutter.SetActive(true);
            Image shutterImg = shutter.GetComponent<Image>();
            if (shutterImg != null)
            {
                Sprite doorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/heavy_iron_shutter_asset_1778255440075.png");
                if (doorSprite != null)
                {
                    shutterImg.sprite = doorSprite;
                    shutterImg.color = Color.white;
                }
                else
                {
                    shutterImg.color = new Color(0.25f, 0.28f, 0.3f, 1f);
                }
            }

            RectTransform rt = shutter.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition3D = Vector3.zero; // Z = 0 обязательно!
                rt.sizeDelta = new Vector2(800f, 600f);
                rt.localScale = Vector3.one;
            }

            // Удаляем временные полоски с прошлой версии
            for (int i = shutter.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = shutter.transform.GetChild(i);
                if (child.name == "Line") Object.DestroyImmediate(child.gameObject);
            }
        }

        // НАСТРАИВАЕМ ПОРЯДОК СИБЛИНГОВ внутри OutsideBg (Маски)
        // Важно: Посетитель и Охранники сзади, а Шторка (дверь) СПЕРЕДИ (последний сиблинг)!
        if (visitor != null) visitor.transform.SetSiblingIndex(0);
        if (guardLeft != null) guardLeft.transform.SetSiblingIndex(1);
        if (guardRight != null) guardRight.transform.SetSiblingIndex(2);
        if (shutter != null) shutter.transform.SetAsLastSibling(); // Последний индекс = рендерится поверх всех персонажей в маске!

        // ==========================================
        // 3. НАСТРОЙКА ДЕГАЗАЦИИ (DecontaminationGas)
        // ==========================================
        GameObject gas = GameObject.Find("DecontaminationGas");
        if (gas != null)
        {
            gas.transform.SetParent(outsideBg.transform, false);
            gas.transform.SetAsLastSibling(); // Поверх посетителя и шторки
            
            RectTransform gasRt = gas.GetComponent<RectTransform>();
            if (gasRt != null)
            {
                gasRt.anchorMin = Vector2.zero;
                gasRt.anchorMax = Vector2.one;
                gasRt.offsetMin = Vector2.zero;
                gasRt.offsetMax = Vector2.zero;
                gasRt.anchoredPosition3D = Vector3.zero;
                gasRt.localScale = Vector3.one;
            }

            // Сбрасываем Z у всех дочерних распылителей газа
            foreach (Transform child in gas.transform)
            {
                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, 0f);
            }
        }

        // ==========================================
        // 4. НАСТРОЙКА ТЕЛЕФОНА (PhoneButton)
        // ==========================================
        GameObject phoneObj = GameObject.Find("PhoneButton");
        if (phoneObj == null) phoneObj = GameObject.Find("PhoneBtn");
        if (phoneObj == null) phoneObj = GameObject.Find("Telephone");

        if (phoneObj != null)
        {
            phoneObj.name = "PhoneButton";
            phoneObj.SetActive(true);
            phoneObj.transform.SetParent(canvas.transform, false);

            Image phoneImg = phoneObj.GetComponent<Image>();
            if (phoneImg != null)
            {
                Sprite phoneSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Visitors/Phone_GuyCN-removebg-preview.png");
                if (phoneSprite != null)
                {
                    phoneImg.sprite = phoneSprite;
                    phoneImg.color = Color.white;
                }
                else
                {
                    phoneImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
                }
            }

            RectTransform rt = phoneObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
                rt.anchoredPosition3D = new Vector3(60f, 40f, 0f); // Слева внизу на столе, Z = 0
                rt.sizeDelta = new Vector2(250f, 180f);
                rt.localScale = Vector3.one;
            }
        }

        GameObject trayContainer = GameObject.Find("DocumentTray");
        GameObject accessSlip = null;
        if (trayContainer != null)
        {
            // --- РЕПОЗИЦИОНИРОВАНИЕ И СТИЛИЗАЦИЯ ПАСПОРТНЫХ ДАННЫХ (на левую сторону папки) ---
            string[] passportFieldNames = new string[] { "PassportName", "PassportLastNameText", "PassportID", "PassportExpDateText", "PassportEyes" };
            Vector3[] passportPositions = new Vector3[] {
                new Vector3(-110f, 95f, 0f),   // PassportName (Имя)
                new Vector3(-110f, 50f, 0f),   // PassportLastNameText (Фамилия)
                new Vector3(-110f, 5f, 0f),    // PassportID
                new Vector3(-110f, -40f, 0f),  // PassportExpDateText
                new Vector3(-110f, -85f, 0f)   // PassportEyes
            };

            for (int i = 0; i < passportFieldNames.Length; i++)
            {
                Transform fieldTr = trayContainer.transform.Find(passportFieldNames[i]);
                if (fieldTr != null)
                {
                    RectTransform fieldRt = fieldTr.GetComponent<RectTransform>();
                    if (fieldRt != null)
                    {
                        fieldRt.anchorMin = new Vector2(0.5f, 0.5f);
                        fieldRt.anchorMax = new Vector2(0.5f, 0.5f);
                        fieldRt.pivot = new Vector2(0.5f, 0.5f);
                        fieldRt.anchoredPosition3D = passportPositions[i];
                        fieldRt.sizeDelta = new Vector2(250f, 40f);
                        fieldRt.localScale = Vector3.one;
                    }

                    TextMeshProUGUI fieldTmp = fieldTr.GetComponent<TextMeshProUGUI>();
                    if (fieldTmp != null)
                    {
                        if (vt323Font != null) fieldTmp.font = vt323Font;
                        fieldTmp.fontSize = 20;
                        fieldTmp.color = new Color(0.18f, 0.15f, 0.12f); // Красивые темно-коричневые ретро-чернила
                        fieldTmp.alignment = TextAlignmentOptions.Left;
                    }
                }
            }

            // --- СОЗДАНИЕ ВЫДЕЛЕННОГО ВЪЕЗДНОГО ТАЛОНА (на правую сторону папки) ---
            Transform oldSlip = trayContainer.transform.Find("AccessSlip");
            if (oldSlip != null) Object.DestroyImmediate(oldSlip.gameObject);

            accessSlip = new GameObject("AccessSlip");
            accessSlip.transform.SetParent(trayContainer.transform, false);

            RectTransform slipRt = accessSlip.AddComponent<RectTransform>();
            slipRt.anchorMin = new Vector2(0.5f, 0.5f);
            slipRt.anchorMax = new Vector2(0.5f, 0.5f);
            slipRt.pivot = new Vector2(0.5f, 0.5f);
            slipRt.anchoredPosition3D = new Vector3(160f, 0f, 0f); // Справа на папке
            slipRt.sizeDelta = new Vector2(240f, 310f); // Компактный вертикальный бланк
            slipRt.localScale = Vector3.one;

            Image slipImg = accessSlip.AddComponent<Image>();
            // Процедурная текстура ретро-бумаги (теплый бежевый крем) с серой картонной каемкой
            Texture2D slipTex = new Texture2D(240, 310);
            slipTex.filterMode = FilterMode.Point;
            for (int y = 0; y < 310; y++)
            {
                for (int x = 0; x < 240; x++)
                {
                    Color c = new Color(0.96f, 0.93f, 0.85f); // Теплый кремовый беж
                    // Легкий шум волокон бумаги для реализма
                    if ((x + y) % 29 == 0 || (x - y) % 31 == 0) c *= 0.98f;
                    
                    // Темная рамка бланка
                    if (x < 3 || x > 236 || y < 3 || y > 306)
                    {
                        c = new Color(0.38f, 0.35f, 0.3f);
                    }
                    slipTex.SetPixel(x, y, c);
                }
            }
            slipTex.Apply();
            slipImg.sprite = Sprite.Create(slipTex, new Rect(0, 0, 240, 310), new Vector2(0.5f, 0.5f));
            slipImg.color = Color.white;

            // Тень талона для визуального отделения от папки
            Outline slipOutline = accessSlip.AddComponent<Outline>();
            slipOutline.effectColor = new Color(0.1f, 0.08f, 0.05f, 0.3f);
            slipOutline.effectDistance = new Vector2(2f, -3f);

            // Заголовок "ВЪЕЗДНОЙ ТАЛОН"
            GameObject slipTitle = new GameObject("Title");
            slipTitle.transform.SetParent(accessSlip.transform, false);
            RectTransform slipTitleRt = slipTitle.AddComponent<RectTransform>();
            slipTitleRt.anchorMin = new Vector2(0f, 1f);
            slipTitleRt.anchorMax = new Vector2(1f, 1f);
            slipTitleRt.pivot = new Vector2(0.5f, 1f);
            slipTitleRt.anchoredPosition3D = new Vector3(0f, -15f, 0f);
            slipTitleRt.sizeDelta = new Vector2(-10f, 30f);
            slipTitleRt.localScale = Vector3.one;

            TextMeshProUGUI titleTxt = slipTitle.AddComponent<TextMeshProUGUI>();
            titleTxt.text = "ВЪЕЗДНОЙ ТАЛОН";
            if (vt323Font != null) titleTxt.font = vt323Font;
            titleTxt.fontSize = 24;
            titleTxt.fontStyle = FontStyles.Bold;
            titleTxt.alignment = TextAlignmentOptions.Center;
            titleTxt.color = new Color(0.2f, 0.18f, 0.15f);

            // Департамент
            GameObject slipSubtitle = new GameObject("Subtitle");
            slipSubtitle.transform.SetParent(accessSlip.transform, false);
            RectTransform subRt = slipSubtitle.AddComponent<RectTransform>();
            subRt.anchorMin = new Vector2(0f, 1f);
            subRt.anchorMax = new Vector2(1f, 1f);
            subRt.pivot = new Vector2(0.5f, 1f);
            subRt.anchoredPosition3D = new Vector3(0f, -42f, 0f);
            subRt.sizeDelta = new Vector2(-10f, 20f);
            subRt.localScale = Vector3.one;

            TextMeshProUGUI subTxt = slipSubtitle.AddComponent<TextMeshProUGUI>();
            subTxt.text = "ДЕПАРТАМЕНТ КОНТРОЛЯ";
            if (vt323Font != null) subTxt.font = vt323Font;
            subTxt.fontSize = 13;
            subTxt.fontStyle = FontStyles.Normal;
            subTxt.alignment = TextAlignmentOptions.Center;
            subTxt.color = new Color(0.4f, 0.38f, 0.35f);

            // Линия-разделитель
            GameObject slipLine = new GameObject("Line");
            slipLine.transform.SetParent(accessSlip.transform, false);
            RectTransform slipLineRt = slipLine.AddComponent<RectTransform>();
            slipLineRt.anchorMin = new Vector2(0.1f, 1f);
            slipLineRt.anchorMax = new Vector2(0.9f, 1f);
            slipLineRt.anchoredPosition3D = new Vector3(0f, -60f, 0f);
            slipLineRt.sizeDelta = new Vector2(0f, 2f);
            slipLineRt.localScale = Vector3.one;
            Image slipLineImg = slipLine.AddComponent<Image>();
            slipLineImg.color = new Color(0.35f, 0.32f, 0.28f, 0.4f);

            // Декоративная плашка статуса решения
            GameObject statusText = new GameObject("StatusText");
            statusText.transform.SetParent(accessSlip.transform, false);
            RectTransform statusRt = statusText.AddComponent<RectTransform>();
            statusRt.anchorMin = new Vector2(0f, 1f);
            statusRt.anchorMax = new Vector2(1f, 1f);
            statusRt.pivot = new Vector2(0.5f, 1f);
            statusRt.anchoredPosition3D = new Vector3(15f, -80f, 0f);
            statusRt.sizeDelta = new Vector2(-30f, 30f);
            statusRt.localScale = Vector3.one;

            TextMeshProUGUI statTxt = statusText.AddComponent<TextMeshProUGUI>();
            statTxt.text = "РЕШЕНИЕ:";
            if (vt323Font != null) statTxt.font = vt323Font;
            statTxt.fontSize = 18;
            statTxt.fontStyle = FontStyles.Bold;
            statTxt.alignment = TextAlignmentOptions.Left;
            statTxt.color = new Color(0.25f, 0.22f, 0.18f);

            // --- ЗОНА "МЕСТО ДЛЯ ПЕЧАТИ" (С пунктирной границей) ---
            GameObject stampArea = new GameObject("StampAreaBox");
            stampArea.transform.SetParent(accessSlip.transform, false);

            RectTransform areaRt = stampArea.AddComponent<RectTransform>();
            areaRt.anchorMin = new Vector2(0.5f, 0.5f);
            areaRt.anchorMax = new Vector2(0.5f, 0.5f);
            areaRt.pivot = new Vector2(0.5f, 0.5f);
            areaRt.anchoredPosition3D = new Vector3(0f, -45f, 0f); // По центру нижней части талона
            areaRt.sizeDelta = new Vector2(200f, 100f);
            areaRt.localScale = Vector3.one;

            Image areaImg = stampArea.AddComponent<Image>();
            // Пунктирная текстура рамки
            Texture2D areaTex = new Texture2D(200, 100);
            areaTex.filterMode = FilterMode.Point;
            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 200; x++)
                {
                    Color c = Color.clear;
                    bool isBorder = (x < 2 || x > 197 || y < 2 || y > 97);
                    if (isBorder)
                    {
                        // Рисуем пунктир: пропускаем пиксели по сетке
                        if ((x / 6) % 2 == 0 && (y / 6) % 2 == 0)
                        {
                            c = new Color(0.42f, 0.38f, 0.35f, 0.6f);
                        }
                    }
                    areaTex.SetPixel(x, y, c);
                }
            }
            areaTex.Apply();
            areaImg.sprite = Sprite.Create(areaTex, new Rect(0, 0, 200, 100), new Vector2(0.5f, 0.5f));
            areaImg.color = Color.white;

            // Текст внутри пунктирной рамки "МЕСТО ДЛЯ ПЕЧАТИ"
            GameObject areaTxtObj = new GameObject("Label");
            areaTxtObj.transform.SetParent(stampArea.transform, false);
            RectTransform areaTxtRt = areaTxtObj.AddComponent<RectTransform>();
            areaTxtRt.anchorMin = Vector2.zero;
            areaTxtRt.anchorMax = Vector2.one;
            areaTxtRt.offsetMin = Vector2.zero;
            areaTxtRt.offsetMax = Vector2.zero;
            areaTxtRt.localScale = Vector3.one;

            TextMeshProUGUI areaTxt = areaTxtObj.AddComponent<TextMeshProUGUI>();
            areaTxt.text = "МЕСТО ДЛЯ ПЕЧАТИ\n(STAMP AREA)";
            if (vt323Font != null) areaTxt.font = vt323Font;
            areaTxt.fontSize = 16;
            areaTxt.fontStyle = FontStyles.Normal;
            areaTxt.alignment = TextAlignmentOptions.Center;
            areaTxt.color = new Color(0.42f, 0.38f, 0.35f, 0.65f);
            areaTxt.lineSpacing = -5f;
        }

        GameObject stampDrawer = GameObject.Find("StampDrawer");
        if (stampDrawer != null) Object.DestroyImmediate(stampDrawer);

        stampDrawer = new GameObject("StampDrawer");
        stampDrawer.transform.SetParent(canvas.transform, false);

        RectTransform drawerRt = stampDrawer.AddComponent<RectTransform>();
        drawerRt.anchorMin = new Vector2(0.5f, 0.5f);
        drawerRt.anchorMax = new Vector2(0.5f, 0.5f);
        drawerRt.pivot = new Vector2(0.5f, 0.0f);
        drawerRt.anchoredPosition3D = new Vector3(520f, -485f, 0f); // Скрыт за столом по умолчанию
        drawerRt.sizeDelta = new Vector2(340f, 200f);
        drawerRt.localScale = Vector3.one;

        Image drawerImg = stampDrawer.AddComponent<Image>();
        // Процедурная текстура ящика (темное дерево с заклепками)
        Texture2D drawerTex = new Texture2D(340, 200);
        drawerTex.filterMode = FilterMode.Point;
        for (int y = 0; y < 200; y++)
        {
            for (int x = 0; x < 340; x++)
            {
                Color c = new Color(0.18f, 0.12f, 0.08f); // Темное дерево
                if (x < 6 || x > 333 || y < 6 || y > 193) c *= 0.6f; // Темная каемка
                else if (x % 32 == 0 || y % 32 == 0) c *= 0.85f; // Волокны дерева
                drawerTex.SetPixel(x, y, c);
            }
        }
        drawerTex.Apply();
        drawerImg.sprite = Sprite.Create(drawerTex, new Rect(0, 0, 340, 200), new Vector2(0.5f, 0.5f));
        drawerImg.color = Color.white;

        // Добавляем рамку
        Outline drawerOutline = stampDrawer.AddComponent<Outline>();
        drawerOutline.effectColor = new Color(0.1f, 0.07f, 0.05f, 0.8f);
        drawerOutline.effectDistance = new Vector2(3f, -3f);

        // Создаем ручку выдвижения (DrawerHandle)
        GameObject handleObj = new GameObject("DrawerHandle");
        handleObj.transform.SetParent(stampDrawer.transform, false);

        RectTransform handleRt = handleObj.AddComponent<RectTransform>();
        handleRt.anchorMin = new Vector2(0.5f, 1f);
        handleRt.anchorMax = new Vector2(0.5f, 1f);
        handleRt.pivot = new Vector2(0.5f, 0f);
        handleRt.anchoredPosition3D = new Vector3(0f, -2f, 0f); // Слегка выглядывает из стола
        handleRt.sizeDelta = new Vector2(180f, 40f);
        handleRt.localScale = Vector3.one;

        Image handleImg = handleObj.AddComponent<Image>();
        // Текстура ручки (полированная медь/металл)
        Texture2D handleTex = new Texture2D(180, 40);
        handleTex.filterMode = FilterMode.Point;
        for (int y = 0; y < 40; y++)
        {
            for (int x = 0; x < 180; x++)
            {
                Color c = new Color(0.45f, 0.35f, 0.15f); // Состаренная латунь
                if (x < 3 || x > 176 || y < 3 || y > 36) c *= 0.5f;
                else if (y > 30) c *= 1.3f; // Блик сверху
                handleTex.SetPixel(x, y, c);
            }
        }
        handleTex.Apply();
        handleImg.sprite = Sprite.Create(handleTex, new Rect(0, 0, 180, 40), new Vector2(0.5f, 0.5f));
        handleImg.color = Color.white;
        handleObj.AddComponent<Button>();

        // Текст на ручке "ШТАМПЫ"
        GameObject handleTxtObj = new GameObject("Text");
        handleTxtObj.transform.SetParent(handleObj.transform, false);
        RectTransform handleTxtRt = handleTxtObj.AddComponent<RectTransform>();
        handleTxtRt.anchorMin = Vector2.zero;
        handleTxtRt.anchorMax = Vector2.one;
        handleTxtRt.offsetMin = Vector2.zero;
        handleTxtRt.offsetMax = Vector2.zero;
        handleTxtRt.localScale = Vector3.one;

        TextMeshProUGUI handleTxt = handleTxtObj.AddComponent<TextMeshProUGUI>();
        handleTxt.text = "ШТАМПЫ";
        if (vt323Font != null) handleTxt.font = vt323Font;
        handleTxt.fontSize = 22;
        handleTxt.fontStyle = FontStyles.Bold;
        handleTxt.alignment = TextAlignmentOptions.Center;
        handleTxt.color = new Color(0.12f, 0.08f, 0.05f, 1f);

        // Создаем два посадочных места (контейнера) для штампов внутри ящика
        GameObject slotApprove = new GameObject("Slot_Approve");
        slotApprove.transform.SetParent(stampDrawer.transform, false);
        RectTransform slotApproveRt = slotApprove.AddComponent<RectTransform>();
        slotApproveRt.anchoredPosition3D = new Vector3(-80f, -20f, 0f);
        slotApproveRt.sizeDelta = new Vector2(120f, 120f);
        slotApproveRt.localScale = Vector3.one;

        GameObject slotReject = new GameObject("Slot_Reject");
        slotReject.transform.SetParent(stampDrawer.transform, false);
        RectTransform slotRejectRt = slotReject.AddComponent<RectTransform>();
        slotRejectRt.anchoredPosition3D = new Vector3(80f, -20f, 0f);
        slotRejectRt.sizeDelta = new Vector2(120f, 120f);
        slotRejectRt.localScale = Vector3.one;

        // 1. Создаем штамп APPROVED
        GameObject stampApprove = new GameObject("StampTool_Approve");
        stampApprove.transform.SetParent(slotApprove.transform, false);
        RectTransform stampApproveRt = stampApprove.AddComponent<RectTransform>();
        stampApproveRt.anchoredPosition3D = Vector3.zero;
        stampApproveRt.sizeDelta = new Vector2(100f, 100f);
        stampApproveRt.localScale = Vector3.one;

        Image stampApproveImg = stampApprove.AddComponent<Image>();
        Texture2D stampApproveTex = CreateStampPixelTexture(100, 100, new Color(0.15f, 0.65f, 0.2f));
        stampApproveImg.sprite = Sprite.Create(stampApproveTex, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
        stampApproveImg.color = Color.white;

        GrabbableStamp grApprove = stampApprove.AddComponent<GrabbableStamp>();
        grApprove.isApproveStamp = true;
        grApprove.slotAnchoredPosition = Vector2.zero;
        grApprove.vt323Font = vt323Font;
        if (accessSlip != null) grApprove.passportArea = accessSlip.GetComponent<RectTransform>();
        else if (trayContainer != null) grApprove.passportArea = trayContainer.GetComponent<RectTransform>();

        // 2. Создаем штамп REJECT
        GameObject stampReject = new GameObject("StampTool_Reject");
        stampReject.transform.SetParent(slotReject.transform, false);
        RectTransform stampRejectRt = stampReject.AddComponent<RectTransform>();
        stampRejectRt.anchoredPosition3D = Vector3.zero;
        stampRejectRt.sizeDelta = new Vector2(100f, 100f);
        stampRejectRt.localScale = Vector3.one;

        Image stampRejectImg = stampReject.AddComponent<Image>();
        Texture2D stampRejectTex = CreateStampPixelTexture(100, 100, new Color(0.85f, 0.15f, 0.15f));
        stampRejectImg.sprite = Sprite.Create(stampRejectTex, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
        stampRejectImg.color = Color.white;

        GrabbableStamp grReject = stampReject.AddComponent<GrabbableStamp>();
        grReject.isApproveStamp = false;
        grReject.slotAnchoredPosition = Vector2.zero;
        grReject.vt323Font = vt323Font;
        if (accessSlip != null) grReject.passportArea = accessSlip.GetComponent<RectTransform>();
        else if (trayContainer != null) grReject.passportArea = trayContainer.GetComponent<RectTransform>();

        // Инициализируем контроллер ящика
        StampDrawerController drawerCtrl = stampDrawer.AddComponent<StampDrawerController>();
        drawerCtrl.drawerRt = drawerRt;
        drawerCtrl.handleButton = handleObj.GetComponent<Button>();

        // Отключаем старые экранные кнопки "ПРОПУСТИТЬ" и "ИЗОЛИРОВАТЬ"
        GameObject approveBase = GameObject.Find("ApproveBase");
        if (approveBase != null) approveBase.SetActive(false);

        GameObject rejectBase = GameObject.Find("RejectBase");
        if (rejectBase != null) rejectBase.SetActive(false);

        // ==========================================
        // 5. КЛИКАБЕЛЬНОСТЬ ПЕРСОНАЖА ДЛЯ ДОПРОСА
        // ==========================================
        // Уничтожаем любые старые дублирующиеся кнопки вопросов
        string[] namesToDestroy = new string[] {
            "InterrogateBtn", "InterrogateButton", "QuestionBtn", "QuestionButton", 
            "InterrogationBtn", "InterrogationButton", "Вопрос", "Допрос", 
            "КнопкаВопроса", "КнопкаДопроса", "Question"
        };
        foreach (GameObject obj in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
        {
            foreach (string dName in namesToDestroy)
            {
                if (obj != null && obj.name.Equals(dName, System.StringComparison.OrdinalIgnoreCase))
                {
                    Object.DestroyImmediate(obj);
                    break;
                }
            }
        }

        Button interrogateBtnComp = null;
        if (gm != null && gm.visitorImageDisplay != null)
        {
            gm.visitorImageDisplay.raycastTarget = true;
            interrogateBtnComp = gm.visitorImageDisplay.gameObject.GetComponent<Button>();
            if (interrogateBtnComp == null)
            {
                interrogateBtnComp = gm.visitorImageDisplay.gameObject.AddComponent<Button>();
            }

            interrogateBtnComp.transition = Selectable.Transition.None; // Без изменения цветов
            interrogateBtnComp.onClick.RemoveAllListeners();
            interrogateBtnComp.onClick.AddListener(gm.OnInterrogateClicked);
            gm.interrogateBtn = interrogateBtnComp;
        }

        // ==========================================
        // 6. СТИЛИЗАЦИЯ ПАНЕЛИ ВОПРОСОВ (QuestionsPanel)
        // ==========================================
        Transform existingPanel = canvas.transform.Find("QuestionsPanel");
        if (existingPanel != null) Object.DestroyImmediate(existingPanel.gameObject);

        GameObject panelObj = new GameObject("QuestionsPanel");
        panelObj.transform.SetParent(canvas.transform, false);

        RectTransform panelRt = panelObj.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax = new Vector2(0.5f, 0.5f);
        panelRt.pivot = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition3D = new Vector3(-600f, 150f, 0f); // Слева от окна, Z = 0
        panelRt.sizeDelta = new Vector2(360f, 320f);
        panelRt.localScale = Vector3.one;

        Image panelImg = panelObj.AddComponent<Image>();
        panelImg.color = new Color(0.04f, 0.06f, 0.04f, 0.98f); // Глубокий темно-зеленый ЭЛТ

        Outline panelOutline = panelObj.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0f, 0.7f, 0.2f, 0.8f); // Зеленый неон
        panelOutline.effectDistance = new Vector2(3f, -3f);

        // Заголовок СИСТЕМА ДОПРОСА
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(panelObj.transform, false);
        RectTransform titleRt = titleObj.AddComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0, 1);
        titleRt.anchorMax = new Vector2(1, 1);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition3D = new Vector3(0, -15, 0);
        titleRt.sizeDelta = new Vector2(-20, 40);
        titleRt.localScale = Vector3.one;

        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "СИСТЕМА ДОПРОСА";
        if (vt323Font != null) titleText.font = vt323Font;
        titleText.fontSize = 28;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0f, 0.9f, 0.3f, 1f);

        // Линия-разделитель
        GameObject lineObj = new GameObject("DividerLine");
        lineObj.transform.SetParent(panelObj.transform, false);
        RectTransform lineRt = lineObj.AddComponent<RectTransform>();
        lineRt.anchorMin = new Vector2(0.05f, 1f);
        lineRt.anchorMax = new Vector2(0.95f, 1f);
        lineRt.anchoredPosition3D = new Vector3(0, -50, 0);
        lineRt.sizeDelta = new Vector2(0, 2);
        lineRt.localScale = Vector3.one;
        Image lineImg = lineObj.AddComponent<Image>();
        lineImg.color = new Color(0f, 0.7f, 0.2f, 0.4f);

        // Контейнер кнопок
        GameObject containerObj = new GameObject("ButtonsContainer");
        containerObj.transform.SetParent(panelObj.transform, false);
        RectTransform containerRt = containerObj.AddComponent<RectTransform>();
        containerRt.anchorMin = new Vector2(0, 0);
        containerRt.anchorMax = new Vector2(1, 1);
        containerRt.pivot = new Vector2(0.5f, 0.5f);
        containerRt.offsetMin = new Vector2(15, 15);
        containerRt.offsetMax = new Vector2(-15, -60);
        containerRt.localScale = Vector3.one;

        VerticalLayoutGroup layout = containerObj.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 10;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;

        string[] questionTexts = new string[] {
            "1. Несовпадение Имени / ID",
            "2. Несоответствие Глаз",
            "3. Истек Срок Паспорта"
        };
        string[] btnNames = new string[] { "NameQuestionBtn", "EyesQuestionBtn", "DateQuestionBtn" };

        for (int i = 0; i < 3; i++)
        {
            GameObject btnObj = new GameObject(btnNames[i]);
            btnObj.transform.SetParent(containerObj.transform, false);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.08f, 0.12f, 0.08f, 1f); // Военно-зеленая кнопка

            Outline btnOutline = btnObj.AddComponent<Outline>();
            btnOutline.effectColor = new Color(0f, 0.5f, 0.15f, 0.6f);
            btnOutline.effectDistance = new Vector2(1.5f, -1.5f);

            Button b = btnObj.AddComponent<Button>();

            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            if (txtRt != null)
            {
                txtRt.anchorMin = Vector2.zero;
                txtRt.anchorMax = Vector2.one;
                txtRt.offsetMin = new Vector2(10, 0);
                txtRt.offsetMax = new Vector2(-10, 0);
                txtRt.localScale = Vector3.one;
            }

            TextMeshProUGUI txtTmp = txtObj.AddComponent<TextMeshProUGUI>();
            txtTmp.text = questionTexts[i];
            if (vt323Font != null) txtTmp.font = vt323Font;
            txtTmp.fontSize = 22;
            txtTmp.fontStyle = FontStyles.Bold;
            txtTmp.alignment = TextAlignmentOptions.Center;
            txtTmp.color = new Color(0.8f, 1f, 0.8f, 1f);
            txtTmp.textWrappingMode = TextWrappingModes.Normal;
        }

        if (gm != null)
        {
            gm.questionsPanel = panelObj;
            Button[] qBtns = panelObj.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < qBtns.Length; i++)
            {
                int index = i;
                qBtns[i].onClick.RemoveAllListeners();
                qBtns[i].onClick.AddListener(() => gm.AskQuestion(index));
            }
        }
        panelObj.SetActive(false); // По умолчанию скрыта

        // ==========================================
        // 7. СТИЛИЗАЦИЯ ДИАЛОГОВОЙ ПАНЕЛИ (DialoguePanel)
        // ==========================================
        GameObject diagPanel = GameObject.Find("DialoguePanel");
        if (diagPanel != null)
        {
            Image diagImg = diagPanel.GetComponent<Image>();
            if (diagImg == null) diagImg = diagPanel.AddComponent<Image>();

            // Проверяем, назначен ли уже кастомный спрайт (например, свиток пергамента)
            bool hasCustomSprite = diagImg.sprite != null && 
                                   diagImg.sprite.name != "UISprite" && 
                                   diagImg.sprite.name != "Background" && 
                                   diagImg.sprite.name != "Square" &&
                                   diagImg.sprite.name != "Default-Particle";

            if (!hasCustomSprite)
            {
                // Создаем роскошный процедурный фон для ЭЛТ-монитора диалога (БЕЗ РАМКИ!)
                Texture2D panelTex = CreateRetroCRTTexture(512, 128, new Color(0.04f, 0.05f, 0.04f, 0.95f), new Color(0f, 0.7f, 0.2f, 1f), false);
                Sprite panelSprite = Sprite.Create(panelTex, new Rect(0, 0, panelTex.width, panelTex.height), new Vector2(0.5f, 0.5f));
                diagImg.sprite = panelSprite;
                diagImg.type = Image.Type.Simple;
            }
            // Всегда сбрасываем цвет на белый, чтобы кастомная текстура свитка не затемнялась
            diagImg.color = Color.white;

            RectTransform panelRT = diagPanel.GetComponent<RectTransform>();
            if (panelRT != null)
            {
                panelRT.anchorMin = new Vector2(0.5f, 0f);
                panelRT.anchorMax = new Vector2(0.5f, 0f);
                panelRT.pivot = new Vector2(0.5f, 0f);
                panelRT.anchoredPosition3D = new Vector3(0f, 50f, 0f); // Парит над столом, Z = 0
                panelRT.sizeDelta = new Vector2(1200f, 240f);
                panelRT.localScale = Vector3.one;
            }

            // Удаляем рамку-обводку с диалоговой панели
            Outline panelOutline2 = diagPanel.GetComponent<Outline>();
            if (panelOutline2 != null) Object.DestroyImmediate(panelOutline2);

            // Настройка Portrait
            Transform portraitTr = diagPanel.transform.Find("Portrait");
            if (portraitTr != null)
            {
                RectTransform portRT = portraitTr.GetComponent<RectTransform>();
                if (portRT != null)
                {
                    portRT.anchorMin = new Vector2(0f, 0.5f);
                    portRT.anchorMax = new Vector2(0f, 0.5f);
                    portRT.pivot = new Vector2(0f, 0.5f);
                    portRT.anchoredPosition3D = new Vector3(30f, 0f, 0f);
                    portRT.sizeDelta = new Vector2(180f, 180f);
                    portRT.localScale = Vector3.one;
                }

                // Удаляем рамку-обводку с портрета
                Outline portOutline = portraitTr.gameObject.GetComponent<Outline>();
                if (portOutline != null) Object.DestroyImmediate(portOutline);
            }

            // Настройка плашки имени NameLabel
            Transform nameLabelTr = null;
            // Ищем NameLabel рекурсивно по всей иерархии DialoguePanel
            foreach (Transform child in diagPanel.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == "NameLabel")
                {
                    nameLabelTr = child;
                    break;
                }
            }

            if (nameLabelTr != null)
            {
                // Принудительно переносим NameLabel в прямые потомки DialoguePanel для правильного независимого позиционирования
                nameLabelTr.SetParent(diagPanel.transform, false);

                Image labelImg = nameLabelTr.GetComponent<Image>();
                if (labelImg == null) labelImg = nameLabelTr.gameObject.AddComponent<Image>();

                bool hasCustomLabelSprite = labelImg.sprite != null && 
                                            labelImg.sprite.name != "UISprite" && 
                                            labelImg.sprite.name != "Background" && 
                                            labelImg.sprite.name != "Square" &&
                                            labelImg.sprite.name != "Default-Particle";

                if (!hasCustomLabelSprite)
                {
                    if (hasCustomSprite)
                    {
                        // Если диалог на пергаменте, делаем фон имени полностью прозрачным
                        labelImg.sprite = null;
                        labelImg.color = Color.clear;
                    }
                    else
                    {
                        Texture2D labelTex = CreateRetroCRTTexture(128, 64, new Color(0.08f, 0.05f, 0.03f, 0.95f), new Color(1f, 0.5f, 0.1f, 1f));
                        Sprite labelSprite = Sprite.Create(labelTex, new Rect(0, 0, 128, 64), new Vector2(0.5f, 0.5f));
                        labelImg.sprite = labelSprite;
                        labelImg.type = Image.Type.Simple;
                        labelImg.color = Color.white;
                    }
                }
                else
                {
                    labelImg.color = Color.white;
                }

                RectTransform labelRT = nameLabelTr.GetComponent<RectTransform>();
                if (labelRT != null)
                {
                    labelRT.anchorMin = new Vector2(0f, 1f);
                    labelRT.anchorMax = new Vector2(0f, 1f);
                    labelRT.pivot = new Vector2(0f, 0.5f);
                    labelRT.anchoredPosition3D = new Vector3(30f, 0f, 0f);
                    labelRT.sizeDelta = new Vector2(300f, 45f); // Расширяем плашку, чтобы имя помещалось в одну строку!
                    labelRT.localScale = Vector3.one;
                }

                TextMeshProUGUI nameTxt = nameLabelTr.GetComponentInChildren<TextMeshProUGUI>(true);
                if (nameTxt != null)
                {
                    if (vt323Font != null) nameTxt.font = vt323Font;
                    nameTxt.fontSize = 16; // Сверхкомпактный изящный размер
                    nameTxt.fontStyle = FontStyles.Bold;
                    nameTxt.enableWordWrapping = false; // Запрещаем перенос слов для предотвращения разбиения имени

                    if (hasCustomSprite)
                    {
                        // На пергаменте используем красивый чернильный/темно-коричневый текст без неона
                        nameTxt.color = new Color(0.15f, 0.1f, 0.05f, 1f);
                        nameTxt.alignment = TextAlignmentOptions.Left;

                        Outline nameOutline = nameTxt.gameObject.GetComponent<Outline>();
                        if (nameOutline != null) Object.DestroyImmediate(nameOutline);
                    }
                    else
                    {
                        nameTxt.color = new Color(1f, 0.6f, 0.1f); // Янтарный ЭЛТ
                        nameTxt.alignment = TextAlignmentOptions.Center;

                        Outline nameOutline = nameTxt.gameObject.GetComponent<Outline>();
                        if (nameOutline == null) nameOutline = nameTxt.gameObject.AddComponent<Outline>();
                        nameOutline.effectColor = new Color(1f, 0.5f, 0.1f, 0.5f);
                        nameOutline.effectDistance = new Vector2(1.5f, -1.5f);
                    }
                }
            }

            // Настройка текста диалога DialogueContent
            Transform contentTr = diagPanel.transform.Find("DialogueContent");
            if (contentTr != null)
            {
                TextMeshProUGUI contentTxt = contentTr.GetComponent<TextMeshProUGUI>();
                if (contentTxt != null)
                {
                    if (vt323Font != null) contentTxt.font = vt323Font;
                    contentTxt.fontSize = 24; // Уменьшаем размер с 44 до 24 для идеального рендеринга!
                    contentTxt.alignment = TextAlignmentOptions.TopLeft;

                    if (hasCustomSprite)
                    {
                        // На пергаменте используем темно-коричневый чернильный цвет
                        contentTxt.color = new Color(0.2f, 0.15f, 0.1f, 1f);

                        Outline contentOutline = contentTr.gameObject.GetComponent<Outline>();
                        if (contentOutline != null) Object.DestroyImmediate(contentOutline);
                    }
                    else
                    {
                        contentTxt.color = new Color(0.2f, 1f, 0.3f); // Люминофорный зеленый
                        
                        Outline contentOutline = contentTr.gameObject.GetComponent<Outline>();
                        if (contentOutline == null) contentOutline = contentTr.gameObject.AddComponent<Outline>();
                        contentOutline.effectColor = new Color(0f, 0.8f, 0.2f, 0.4f);
                        contentOutline.effectDistance = new Vector2(2f, -2f);
                    }

                    contentTxt.characterSpacing = 0.5f;
                    contentTxt.lineSpacing = 5f;

                    RectTransform contentRT = contentTr.GetComponent<RectTransform>();
                    if (contentRT != null)
                    {
                        contentRT.anchorMin = Vector2.zero;
                        contentRT.anchorMax = Vector2.one;
                        contentRT.pivot = new Vector2(0.5f, 0.5f);
                        contentRT.offsetMin = new Vector2(220f, 25f); // Отступ от портрета
                        contentRT.offsetMax = new Vector2(-50f, -50f);
                        contentRT.localScale = Vector3.one;
                    }
                }
            }
        }

        // ==========================================
        // 8. ГЛОБАЛЬНЫЙ ПОРЯДОК СОРТИРОВКИ ДЛЯ ВСЕХ ЭЛЕМЕНТОВ КАНВАСА
        // ==========================================
        string[] canvasChildrenSorted = new string[] {
            "WallBackground",
            "OutsideBg",
            "WindowFrame",
            "Desk",
            "DeskEdge",
            "StampDrawer",
            "DocumentTray",
            "ApproveBase",
            "RejectBase",
            "PhysicalMonitor",
            "PhoneButton",
            "GlassCracks",
            "BloodOverlay",
            "QuestionsPanel",
            "DialoguePanel",
            "VictoryPanel",
            "GameOverPanel",
            "PausePanel",
            "PauseBtn",
            "RagCursor",
            "CRT_Overlay_Safe",
            "ScreenFlash"
        };

        int canvasIdx = 0;
        foreach (string uiName in canvasChildrenSorted)
        {
            GameObject ui = GameObject.Find(uiName);
            if (ui != null)
            {
                ui.transform.SetParent(canvas.transform, false);
                ui.transform.SetSiblingIndex(canvasIdx);
                canvasIdx++;
            }
        }

        try
        {
            InspectScene.Inspect();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Antigravity] Error calling InspectScene.Inspect: " + ex.Message);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>[Antigravity] ОКНО И ДИАЛОГОВАЯ ПАНЕЛЬ НАСТРОЕНЫ ИДЕАЛЬНО!</color>");
    }

    // Создает потрясающую текстуру в стиле терминала Fallout / ЭЛТ-экранов с зеленой/оранжевой рамкой и фоновой сеткой
    private static Texture2D CreateRetroCRTTexture(int width, int height, Color bgColor, Color borderColor, bool drawBorder = true)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = bgColor;

                // Эффект выпуклости (темнеет к краям)
                float dx = Mathf.Abs(x - width / 2f) / (width / 2f);
                float dy = Mathf.Abs(y - height / 2f) / (height / 2f);
                float dist = Mathf.Max(dx, dy);
                c = Color.Lerp(c, c * 0.4f, dist * dist);

                // Строки развертки (scanlines)
                if (y % 4 == 0)
                {
                    c = Color.Lerp(c, Color.black, 0.15f);
                }

                // Сетка (subtle grid)
                if ((x % 16 == 0 || y % 16 == 0) && x > 6 && x < width - 6 && y > 6 && y < height - 6)
                {
                    c = Color.Lerp(c, borderColor, 0.04f);
                }

                // Скругленные углы и светящаяся рамка
                bool isBorder = (x < 5 || x > width - 6 || y < 5 || y > height - 6);
                if (isBorder)
                {
                    bool isCorner = ((x < 12 && y < 12) || (x > width - 13 && y < 12) || (x < 12 && y > height - 13) || (x > width - 13 && y > height - 13));
                    if (isCorner)
                    {
                        float cx = (x < 12) ? 12 : width - 13;
                        float cy = (y < 12) ? 12 : height - 13;
                        float r = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        
                        if (r > 8f)
                        {
                            c = new Color(0, 0, 0, 0); // Прозрачный скругленный угол
                        }
                        else if (r > 6f)
                        {
                            c = drawBorder ? borderColor : bgColor;
                        }
                    }
                    else
                    {
                        c = drawBorder ? borderColor : bgColor;
                    }
                }

                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
        return texture;
    }

    private static Texture2D CreateStampPixelTexture(int w, int h, Color accentColor)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                Color c = Color.clear;

                // Процедурный плоский 2D пиксель-арт штамп:
                // 1. Ручка штампа (дерево) по центру вверху
                bool isHandle = (x >= 40 && x <= 60 && y >= 50 && y <= 90);
                // 2. Металлический воротник посередине
                bool isCollar = (x >= 30 && x <= 70 && y >= 30 && y < 50);
                // 3. Цветная подошва печати внизу
                bool isBase = (x >= 15 && x <= 85 && y >= 10 && y < 30);

                if (isHandle)
                {
                    c = new Color(0.4f, 0.25f, 0.15f); // Коричневое дерево ручки
                    // Годовые кольца или блик ручки
                    if (x == 45 || x == 46) c *= 1.25f;
                    else if (x < 43 || x > 57 || y > 83) c *= 0.7f;
                }
                else if (isCollar)
                {
                    c = new Color(0.35f, 0.38f, 0.42f); // Серый металл
                    if (x == 35 || x == 36) c *= 1.3f; // Блик на металле
                    else if (x < 33 || x > 67) c *= 0.7f;
                }
                else if (isBase)
                {
                    c = accentColor; // Яркий зеленый/красный
                    // Каемка
                    if (x < 18 || x > 82 || y < 13 || y > 27) c *= 0.7f;
                }

                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        return tex;
    }

    private static void DumpGameObject(GameObject go, string indent, System.Text.StringBuilder sb)
    {
        sb.AppendLine($"{indent}- {go.name} (Active: {go.activeInHierarchy})");
        for (int i = 0; i < go.transform.childCount; i++)
        {
            DumpGameObject(go.transform.GetChild(i).gameObject, indent + "  ", sb);
        }
    }
}
