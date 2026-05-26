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

        if (!force && EditorPrefs.GetBool("AutoFixWindowAndDialogue_v18", false)) return;
        EditorPrefs.SetBool("AutoFixWindowAndDialogue_v18", true);

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

        // Исправляем возможные проблемы импорта встроенного fallback-шрифта TMP
        string fallbackPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF - Fallback.asset";
        if (System.IO.File.Exists(fallbackPath))
        {
            AssetDatabase.ImportAsset(fallbackPath, ImportAssetOptions.ForceUpdate);
        }

        // Добавляем полноценный кириллический шрифт в качестве fallback для пиксельного шрифта
        TMP_FontAsset defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (vt323Font != null && defaultFont != null)
        {
            if (vt323Font.fallbackFontAssetTable == null)
            {
                vt323Font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
            }
            if (!vt323Font.fallbackFontAssetTable.Contains(defaultFont))
            {
                vt323Font.fallbackFontAssetTable.Add(defaultFont);
                EditorUtility.SetDirty(vt323Font);
                AssetDatabase.SaveAssets();
            }
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
        Image frameImg = windowFrame.GetComponent<Image>();
        if (frameImg != null) frameImg.raycastTarget = false;

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
            outsideImg.raycastTarget = false;
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
            if (phoneObj.transform.parent != canvas.transform)
            {
                phoneObj.transform.SetParent(canvas.transform, true);
            }
        }

        GameObject trayContainer = GameObject.Find("DocumentTray");
        GameObject accessSlip = null;
        if (trayContainer != null)
        {
            // --- РЕПОЗИЦИОНИРОВАНИЕ И СТИЛИЗАЦИЯ ПАСПОРТНЫХ ДАННЫХ (на левую сторону папки) ---
            string[] passportFieldNames = new string[] { "PassportName", "PassportLastNameText", "PassportID", "PassportExpDateText", "PassportEyes" };
            Vector3[] passportPositions = new Vector3[] {
                new Vector3(80f, 40f, 0f),    // PassportName (Имя) - Поднимаем на линию GIVEN NAMES (было 15)
                new Vector3(80f, 90f, 0f),    // PassportLastNameText (Фамилия) - Поднимаем на линию SURNAME (было 65)
                new Vector3(80f, -10f, 0f),   // PassportID - Поднимаем на третью строчку (было -30)
                new Vector3(140f, -110f, 0f), // PassportExpDateText - Опускаем точно в рамку EXPIRY DATE (было -85)
                new Vector3(-20f, -110f, 0f)  // PassportEyes - Опускаем точно в рамку EYE COLOR (было -85)
            };

            for (int i = 0; i < passportFieldNames.Length; i++)
            {
                // Принудительно ищем объекты по всей сцене (даже в корне иерархии!) и возвращаем их внутрь DocumentTray!
                GameObject fieldObj = GameObject.Find(passportFieldNames[i]);
                if (fieldObj != null)
                {
                    RectTransform looseRt = fieldObj.GetComponent<RectTransform>();
                    if (looseRt != null)
                    {
                        // Сохраняем мировую позицию при перепривязке родителя, чтобы координаты не сбивались!
                        fieldObj.transform.SetParent(trayContainer.transform, true);
                        passportPositions[i] = looseRt.anchoredPosition3D;
                    }
                    else
                    {
                        fieldObj.transform.SetParent(trayContainer.transform, false);
                    }
                }

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
                        fieldRt.sizeDelta = new Vector2(200f, 40f); // Ограничиваем ширину, чтобы текст не вылезал за границы
                        fieldRt.localScale = Vector3.one;
                    }

                    TextMeshProUGUI fieldTmp = fieldTr.GetComponent<TextMeshProUGUI>();
                    if (fieldTmp != null)
                    {
                        if (vt323Font != null) fieldTmp.font = vt323Font;
                        fieldTmp.fontSize = 28; // Делаем размер больше (было 22)
                        fieldTmp.fontStyle = FontStyles.Bold; // Делаем текст жирным для четкости
                        fieldTmp.color = new Color(0.04f, 0.04f, 0.04f, 1f); // Насыщенный угольно-черный цвет чернил
                        fieldTmp.alignment = TextAlignmentOptions.Left;
                    }
                }
            }

            // --- ПРЕСЕРВАЦИЯ И НАСТРОЙКА ВЪЕЗДНОГО ТАЛОНА (ПОЛНОСТЬЮ IN-PLACE) ---
            Transform slipTr = null;
            foreach (Transform t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include))
            {
                if (t.name == "AccessSlip")
                {
                    slipTr = t;
                    break;
                }
            }
            accessSlip = slipTr != null ? slipTr.gameObject : null;

            if (accessSlip == null)
            {
                // Если талона вообще нет на сцене, создаем его с нуля по умолчанию
                accessSlip = new GameObject("AccessSlip");
                accessSlip.transform.SetParent(trayContainer.transform, false);

                RectTransform slipRt = accessSlip.AddComponent<RectTransform>();
                slipRt.anchorMin = new Vector2(0.5f, 0.5f);
                slipRt.anchorMax = new Vector2(0.5f, 0.5f);
                slipRt.pivot = new Vector2(0.5f, 0.5f);
                slipRt.anchoredPosition3D = new Vector3(160f, 0f, 0f); // Справа на папке
                slipRt.sizeDelta = new Vector2(240f, 310f);
                slipRt.localScale = Vector3.one;

                Image slipImg = accessSlip.AddComponent<Image>();
                Texture2D slipTex = new Texture2D(240, 310);
                slipTex.filterMode = FilterMode.Point;
                for (int y = 0; y < 310; y++)
                {
                    for (int x = 0; x < 240; x++)
                    {
                        Color c = new Color(0.96f, 0.94f, 0.88f); // Теплый пергамент
                        bool isBorder = (x < 4 || x > 235 || y < 4 || y > 305);
                        if (isBorder)
                        {
                            c = new Color(0.38f, 0.35f, 0.3f);
                        }
                        slipTex.SetPixel(x, y, c);
                    }
                }
                slipTex.Apply();
                slipImg.sprite = Sprite.Create(slipTex, new Rect(0, 0, 240, 310), new Vector2(0.5f, 0.5f));
                slipImg.color = Color.white;

                // Тень талона
                Outline slipOutline = accessSlip.AddComponent<Outline>();
                slipOutline.effectColor = new Color(0.1f, 0.08f, 0.05f, 0.3f);
                slipOutline.effectDistance = new Vector2(2f, -3f);

                // Создаем стандартные процедурные элементы (поскольку талона не было на сцене)
                
                // 1. Заголовок "ВЪЕЗДНОЙ ТАЛОН"
                GameObject slipTitle = new GameObject("Title");
                slipTitle.transform.SetParent(accessSlip.transform, false);
                RectTransform newTitleRt = slipTitle.AddComponent<RectTransform>();
                newTitleRt.anchorMin = new Vector2(0f, 1f);
                newTitleRt.anchorMax = new Vector2(1f, 1f);
                newTitleRt.pivot = new Vector2(0.5f, 1f);
                newTitleRt.anchoredPosition3D = new Vector3(0f, -15f, 0f);
                newTitleRt.sizeDelta = new Vector2(-10f, 30f);
                newTitleRt.localScale = Vector3.one;

                TextMeshProUGUI titleTxtComp = slipTitle.AddComponent<TextMeshProUGUI>();
                titleTxtComp.text = "ВЪЕЗДНОЙ ТАЛОН";
                if (vt323Font != null) titleTxtComp.font = vt323Font;
                titleTxtComp.fontSize = 24;
                titleTxtComp.alignment = TextAlignmentOptions.Center;
                titleTxtComp.color = new Color(0.2f, 0.18f, 0.15f);

                // 2. Департамент
                GameObject slipSubtitle = new GameObject("Subtitle");
                slipSubtitle.transform.SetParent(accessSlip.transform, false);
                RectTransform newSubRt = slipSubtitle.AddComponent<RectTransform>();
                newSubRt.anchorMin = new Vector2(0f, 1f);
                newSubRt.anchorMax = new Vector2(1f, 1f);
                newSubRt.pivot = new Vector2(0.5f, 1f);
                newSubRt.anchoredPosition3D = new Vector3(0f, -42f, 0f);
                newSubRt.sizeDelta = new Vector2(-10f, 20f);
                newSubRt.localScale = Vector3.one;

                TextMeshProUGUI subTxtComp = slipSubtitle.AddComponent<TextMeshProUGUI>();
                subTxtComp.text = "ДЕПАРТАМЕНТ КОНТРОЛЯ";
                if (vt323Font != null) subTxtComp.font = vt323Font;
                subTxtComp.fontSize = 13;
                subTxtComp.alignment = TextAlignmentOptions.Center;
                subTxtComp.color = new Color(0.4f, 0.38f, 0.35f);

                // 3. Линия-разделитель
                GameObject slipLine = new GameObject("Line");
                slipLine.transform.SetParent(accessSlip.transform, false);
                RectTransform newLineRt = slipLine.AddComponent<RectTransform>();
                newLineRt.anchorMin = new Vector2(0.1f, 1f);
                newLineRt.anchorMax = new Vector2(0.9f, 1f);
                newLineRt.anchoredPosition3D = new Vector3(0f, -60f, 0f);
                newLineRt.sizeDelta = new Vector2(0f, 2f);
                newLineRt.localScale = Vector3.one;
                Image lineImgComp = slipLine.AddComponent<Image>();
                lineImgComp.color = new Color(0.35f, 0.32f, 0.28f, 0.4f);

                // 4. Плашка статуса решения
                GameObject statusText = new GameObject("StatusText");
                statusText.transform.SetParent(accessSlip.transform, false);
                RectTransform newStatusRt = statusText.AddComponent<RectTransform>();
                newStatusRt.anchorMin = new Vector2(0f, 1f);
                newStatusRt.anchorMax = new Vector2(1f, 1f);
                newStatusRt.pivot = new Vector2(0.5f, 1f);
                newStatusRt.anchoredPosition3D = new Vector3(0f, -80f, 0f);
                newStatusRt.sizeDelta = new Vector2(-20f, 40f);
                newStatusRt.localScale = Vector3.one;

                TextMeshProUGUI statusTxtComp = statusText.AddComponent<TextMeshProUGUI>();
                statusTxtComp.text = "РЕШЕНИЕ:";
                if (vt323Font != null) statusTxtComp.font = vt323Font;
                statusTxtComp.fontSize = 24;
                statusTxtComp.alignment = TextAlignmentOptions.Center;
                statusTxtComp.color = new Color(0.25f, 0.22f, 0.18f);

                // 5. Зона "МЕСТО ДЛЯ ПЕЧАТИ"
                GameObject stampArea = new GameObject("StampAreaBox");
                stampArea.transform.SetParent(accessSlip.transform, false);
                RectTransform newAreaRt = stampArea.AddComponent<RectTransform>();
                newAreaRt.anchorMin = new Vector2(0.5f, 0.5f);
                newAreaRt.anchorMax = new Vector2(0.5f, 0.5f);
                newAreaRt.pivot = new Vector2(0.5f, 0.5f);
                newAreaRt.anchoredPosition3D = new Vector3(0f, -40f, 0f);
                newAreaRt.sizeDelta = new Vector2(200f, 100f);
                newAreaRt.localScale = Vector3.one;

                Image areaImg = stampArea.AddComponent<Image>();
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

                // 6. Текст внутри "МЕСТО ДЛЯ ПЕЧАТИ"
                GameObject areaTxtObj = new GameObject("Label");
                areaTxtObj.transform.SetParent(stampArea.transform, false);
                RectTransform newLabelRt = areaTxtObj.AddComponent<RectTransform>();
                newLabelRt.anchorMin = Vector2.zero;
                newLabelRt.anchorMax = Vector2.one;
                newLabelRt.offsetMin = Vector2.zero;
                newLabelRt.offsetMax = Vector2.zero;
                newLabelRt.localScale = Vector3.one;

                TextMeshProUGUI areaTxt = areaTxtObj.AddComponent<TextMeshProUGUI>();
                areaTxt.text = "МЕСТО ДЛЯ ПЕЧАТИ\n(STAMP AREA)";
                if (vt323Font != null) areaTxt.font = vt323Font;
                areaTxt.fontSize = 16;
                areaTxt.alignment = TextAlignmentOptions.Center;
                areaTxt.color = new Color(0.42f, 0.38f, 0.35f, 0.65f);
                areaTxt.lineSpacing = -5f;
            }
            else
            {
                // Если талон существует, мы НЕ ТРОГАЕМ его вообще, сохраняя все ручные изменения пользователя (цвет, размер, спрайт)!
                if (accessSlip.transform.parent != trayContainer.transform && accessSlip.transform.parent?.name != "Canvas")
                {
                    accessSlip.transform.SetParent(trayContainer.transform, true);
                }
            }
        }

        // --- НАСТРОЙКА ШКАФЧИКА ШТАМПОВ (В ПОЛНОСТЬЮ IN-PLACE) ---
        Transform drawerTr = canvas.transform.Find("StampDrawer");
        if (drawerTr == null)
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            {
                if (go.name == "StampDrawer") { drawerTr = go.transform; break; }
            }
        }
        GameObject stampDrawer = drawerTr != null ? drawerTr.gameObject : null;

        if (stampDrawer == null)
        {
            stampDrawer = new GameObject("StampDrawer");
            stampDrawer.transform.SetParent(canvas.transform, false);
            RectTransform drawerRt = stampDrawer.AddComponent<RectTransform>();
            drawerRt.anchorMin = new Vector2(0.5f, 0.5f);
            drawerRt.anchorMax = new Vector2(0.5f, 0.5f);
            drawerRt.pivot = new Vector2(0.5f, 0.0f);
            drawerRt.anchoredPosition3D = new Vector3(520f, -485f, 0f);
            drawerRt.sizeDelta = new Vector2(340f, 200f);
            drawerRt.localScale = Vector3.one;
        }

        RectTransform drawerRtComp = stampDrawer.GetComponent<RectTransform>();
        Image drawerImgComp = stampDrawer.GetComponent<Image>();
        if (drawerImgComp == null)
        {
            drawerImgComp = stampDrawer.AddComponent<Image>();
            Texture2D drawerTex = new Texture2D(340, 200);
            drawerTex.filterMode = FilterMode.Point;
            for (int y = 0; y < 200; y++)
            {
                for (int x = 0; x < 340; x++)
                {
                    Color c = new Color(0.18f, 0.12f, 0.08f); // Темное дерево
                    if (x < 6 || x > 333 || y < 6 || y > 193) c *= 0.6f;
                    else if (x % 32 == 0 || y % 32 == 0) c *= 0.85f;
                    drawerTex.SetPixel(x, y, c);
                }
            }
            drawerTex.Apply();
            drawerImgComp.sprite = Sprite.Create(drawerTex, new Rect(0, 0, 340, 200), new Vector2(0.5f, 0.5f));
            drawerImgComp.color = Color.white;
        }

        Outline drawerOutlineComp = stampDrawer.GetComponent<Outline>();
        if (drawerOutlineComp == null)
        {
            drawerOutlineComp = stampDrawer.AddComponent<Outline>();
            drawerOutlineComp.effectColor = new Color(0.1f, 0.07f, 0.05f, 0.8f);
            drawerOutlineComp.effectDistance = new Vector2(3f, -3f);
        }

        // Ручка выдвижения (DrawerHandle)
        Transform handleTr = stampDrawer.transform.Find("DrawerHandle");
        GameObject handleObj = handleTr != null ? handleTr.gameObject : null;
        if (handleObj == null)
        {
            handleObj = new GameObject("DrawerHandle");
            handleObj.transform.SetParent(stampDrawer.transform, false);
            RectTransform rt = handleObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition3D = new Vector3(0f, -2f, 0f);
            rt.sizeDelta = new Vector2(180f, 40f);
            rt.localScale = Vector3.one;

            Image handleImg = handleObj.AddComponent<Image>();
            Texture2D handleTex = new Texture2D(180, 40);
            handleTex.filterMode = FilterMode.Point;
            for (int y = 0; y < 40; y++)
            {
                for (int x = 0; x < 180; x++)
                {
                    Color c = new Color(0.45f, 0.35f, 0.15f);
                    if (x < 3 || x > 176 || y < 3 || y > 36) c *= 0.5f;
                    else if (y > 30) c *= 1.3f;
                    handleTex.SetPixel(x, y, c);
                }
            }
            handleTex.Apply();
            handleImg.sprite = Sprite.Create(handleTex, new Rect(0, 0, 180, 40), new Vector2(0.5f, 0.5f));
            handleImg.color = Color.white;
            handleObj.AddComponent<Button>();

            GameObject handleTxtObj = new GameObject("Text");
            handleTxtObj.transform.SetParent(handleObj.transform, false);
            RectTransform txtRt = handleTxtObj.AddComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.offsetMin = Vector2.zero;
            txtRt.offsetMax = Vector2.zero;
            txtRt.localScale = Vector3.one;

            TextMeshProUGUI handleTxt = handleTxtObj.AddComponent<TextMeshProUGUI>();
            handleTxt.text = "ШТАМПЫ";
            if (vt323Font != null) handleTxt.font = vt323Font;
            handleTxt.fontSize = 22;
            handleTxt.fontStyle = FontStyles.Bold;
            handleTxt.alignment = TextAlignmentOptions.Center;
            handleTxt.color = new Color(0.12f, 0.08f, 0.05f, 1f);
        }

        // Уничтожаем старый красный штамп и его слот
        GameObject oldRejectSlot = GameObject.Find("Slot_Reject");
        if (oldRejectSlot != null) Object.DestroyImmediate(oldRejectSlot);
        GameObject oldRejectStamp = GameObject.Find("StampTool_Reject");
        if (oldRejectStamp != null) Object.DestroyImmediate(oldRejectStamp);

        // Зеленый слот APPROVED СТРОГО ПО ЦЕНТРУ ящика!
        Transform slotApproveTr = stampDrawer.transform.Find("Slot_Approve");
        GameObject slotApprove = slotApproveTr != null ? slotApproveTr.gameObject : null;
        if (slotApprove == null)
        {
            slotApprove = new GameObject("Slot_Approve");
            slotApprove.transform.SetParent(stampDrawer.transform, false);
            RectTransform rt = slotApprove.AddComponent<RectTransform>();
            rt.anchoredPosition3D = new Vector3(0f, -20f, 0f);
            rt.sizeDelta = new Vector2(120f, 120f);
            rt.localScale = Vector3.one;
        }

        Transform stampApproveTr = slotApprove.transform.Find("StampTool_Approve");
        if (stampApproveTr == null) stampApproveTr = canvas.transform.Find("StampTool_Approve");
        if (stampApproveTr == null)
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            {
                if (go.name == "StampTool_Approve") { stampApproveTr = go.transform; break; }
            }
        }
        GameObject stampApprove = stampApproveTr != null ? stampApproveTr.gameObject : null;

        if (stampApprove == null)
        {
            stampApprove = new GameObject("StampTool_Approve");
            stampApprove.transform.SetParent(slotApprove.transform, false);
            RectTransform rt = stampApprove.AddComponent<RectTransform>();
            rt.anchoredPosition3D = Vector3.zero;
            rt.sizeDelta = new Vector2(100f, 100f);
            rt.localScale = Vector3.one;

            Image img = stampApprove.AddComponent<Image>();
            Texture2D tex = CreateStampPixelTexture(100, 100, new Color(0.15f, 0.65f, 0.2f));
            img.sprite = Sprite.Create(tex, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
            img.color = Color.white;
        }
        else
        {
            if (stampApprove.transform.parent != slotApprove.transform)
            {
                stampApprove.transform.SetParent(slotApprove.transform, false);
                RectTransform rt = stampApprove.GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition3D = Vector3.zero;
            }
        }

        GrabbableStamp grApprove = stampApprove.GetComponent<GrabbableStamp>();
        if (grApprove == null) grApprove = stampApprove.AddComponent<GrabbableStamp>();
        grApprove.isApproveStamp = true;
        grApprove.slotAnchoredPosition = Vector2.zero;
        grApprove.vt323Font = vt323Font;
        if (accessSlip != null) grApprove.passportArea = accessSlip.GetComponent<RectTransform>();
        else if (trayContainer != null) grApprove.passportArea = trayContainer.GetComponent<RectTransform>();

        // --- КНОПКА АВАРИЙНОЙ ИЗОЛЯЦИИ (Винтик на столе) ---
        Transform emergencyTr = canvas.transform.Find("EmergencyIsolateBtn");
        if (emergencyTr == null)
        {
            foreach (GameObject go in Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include))
            {
                if (go.name == "EmergencyIsolateBtn") { emergencyTr = go.transform; break; }
            }
        }
        GameObject emergencyBtn = emergencyTr != null ? emergencyTr.gameObject : null;

        if (emergencyBtn == null)
        {
            emergencyBtn = new GameObject("EmergencyIsolateBtn");
            emergencyBtn.transform.SetParent(canvas.transform, false);
            RectTransform rt = emergencyBtn.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 0f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition3D = new Vector3(230f, 50f, 0f);
            rt.sizeDelta = new Vector2(30f, 30f);
            rt.localScale = Vector3.one;

            Image img = emergencyBtn.AddComponent<Image>();
            img.raycastTarget = true;
            Texture2D rivetTex = new Texture2D(30, 30);
            rivetTex.filterMode = FilterMode.Point;
            for (int y = 0; y < 30; y++)
            {
                for (int x = 0; x < 30; x++)
                {
                    float dx = x - 15f;
                    float dy = y - 15f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist > 14f) rivetTex.SetPixel(x, y, Color.clear);
                    else
                    {
                        float factor = (14f - dist) / 14f;
                        Color metalColor = new Color(0.55f, 0.57f, 0.6f) * (0.6f + 0.4f * factor);
                        if (dx < -2f && dy > 2f) metalColor = Color.white * 0.9f;
                        if (Mathf.Abs(dx - dy) < 2f && dist < 9f) metalColor = new Color(0.15f, 0.15f, 0.16f);
                        rivetTex.SetPixel(x, y, metalColor);
                    }
                }
            }
            rivetTex.Apply();
            img.sprite = Sprite.Create(rivetTex, new Rect(0, 0, 30, 30), new Vector2(0.5f, 0.5f));
            img.color = Color.white;
        }

        // Подключаем слушатель в рантайме
        Button alarmBtnComp = emergencyBtn.GetComponent<Button>();
        if (alarmBtnComp == null) alarmBtnComp = emergencyBtn.AddComponent<Button>();
        
        Navigation noneNav = new Navigation();
        noneNav.mode = Navigation.Mode.None;
        alarmBtnComp.navigation = noneNav;
        
        ColorBlock colors = alarmBtnComp.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.pressedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        colors.selectedColor = Color.white;
        colors.fadeDuration = 0.05f;
        alarmBtnComp.colors = colors;

        alarmBtnComp.onClick.RemoveAllListeners();
        if (gm != null)
        {
            alarmBtnComp.onClick.AddListener(gm.OnRejectClicked);
        }

        // Инициализируем контроллер ящика
        StampDrawerController drawerCtrl = stampDrawer.GetComponent<StampDrawerController>();
        if (drawerCtrl == null) drawerCtrl = stampDrawer.AddComponent<StampDrawerController>();
        drawerCtrl.drawerRt = drawerRtComp;
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
                    labelRT.anchoredPosition3D = new Vector3(30f, -5f, 0f); // Исходное красивое положение на свитке
                    labelRT.sizeDelta = new Vector2(320f, 42f); // Фиксированный красивый и компактный размер плашки!
                    labelRT.localScale = Vector3.one;
                }

                // Удаляем HorizontalLayoutGroup и ContentSizeFitter, чтобы избежать бесконечного растяжения
                UnityEngine.UI.HorizontalLayoutGroup oldLayout = nameLabelTr.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
                if (oldLayout != null) Object.DestroyImmediate(oldLayout);

                UnityEngine.UI.ContentSizeFitter oldFitter = nameLabelTr.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (oldFitter != null) Object.DestroyImmediate(oldFitter);

                TextMeshProUGUI nameTxt = nameLabelTr.GetComponentInChildren<TextMeshProUGUI>(true);
                if (nameTxt != null)
                {
                    // Растягиваем текстовый компонент внутри плашки, чтобы центрирование работало со 100% точностью
                    RectTransform nameTxtRt = nameTxt.GetComponent<RectTransform>();
                    if (nameTxtRt != null)
                    {
                        nameTxtRt.anchorMin = Vector2.zero;
                        nameTxtRt.anchorMax = Vector2.one;
                        nameTxtRt.pivot = new Vector2(0.5f, 0.5f);
                        nameTxtRt.anchoredPosition3D = Vector3.zero; // Сбрасываем сдвиги
                        nameTxtRt.offsetMin = new Vector2(10f, 0f);  // Внутренний отступ слева
                        nameTxtRt.offsetMax = new Vector2(-10f, 0f); // Внутренний отступ справа
                        nameTxtRt.localScale = Vector3.one;
                    }

                    if (vt323Font != null) nameTxt.font = vt323Font;
                    nameTxt.fontSize = 20; // Идеальный размер шрифта 20
                    nameTxt.fontStyle = FontStyles.Bold;
                    nameTxt.enableWordWrapping = false; // Запрещаем перенос
                    nameTxt.alignment = TextAlignmentOptions.Center; // По центру

                    if (hasCustomSprite)
                    {
                        // На пергаменте с железной табличкой делаем текст ярким теплым белым для идеальной видимости!
                        nameTxt.color = new Color(0.95f, 0.95f, 0.95f, 1f);

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
                    contentTxt.fontSize = 27; // Увеличиваем размер до 27 (было 24) для идеального чтения без усталости глаз!
                    contentTxt.alignment = TextAlignmentOptions.TopLeft;

                    if (hasCustomSprite)
                    {
                        // На пергаменте делаем текст насыщенным темно-угольным чернильным для высокой контрастности!
                        contentTxt.color = new Color(0.06f, 0.05f, 0.04f, 1f);

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
            if (gm != null)
            {
                if (portraitTr != null)
                {
                    gm.dialoguePortrait = portraitTr.GetComponent<Image>();
                }
                
                // Принудительно конвертируем текстуру охраны в Sprite, если она импортирована как дефолтная
                string guardPath = "Assets/creepy_guard_asset_1778260414657.png";
                TextureImporter ti = AssetImporter.GetAtPath(guardPath) as TextureImporter;
                if (ti != null && ti.textureType != TextureImporterType.Sprite)
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.SaveAndReimport();
                }

                // Автоматически привязываем спрайт охраны/службы безопасности, если они пусты в GameManager
                if (gm.dispatcherSprite == null)
                {
                    gm.dispatcherSprite = AssetDatabase.LoadAssetAtPath<Sprite>(guardPath);
                }
                if (gm.guardStandingSprite == null)
                {
                    gm.guardStandingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(guardPath);
                }

                // Автоматически привязываем звуки открытия/закрытия двери, если они пусты
                if (gm.shutterCloseSound == null)
                {
                    gm.shutterCloseSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Visitors/закрытия дверя.ogg");
                }
                if (gm.shutterOpenSound == null)
                {
                    gm.shutterOpenSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Visitors/закрытия дверя.ogg");
                }

                // Автоматически привязываем оверлей газеты, если он есть
                GameObject npPanel = GameObject.Find("NewspaperPanel");
                if (npPanel != null)
                {
                    gm.newspaperPanel = npPanel;
                    Transform headTr = npPanel.transform.Find("NewspaperHeadline");
                    if (headTr != null) gm.newspaperHeadlineText = headTr.GetComponent<TMPro.TextMeshProUGUI>();
                    Transform bodyTr = npPanel.transform.Find("NewspaperBody");
                    if (bodyTr != null) gm.newspaperBodyText = bodyTr.GetComponent<TMPro.TextMeshProUGUI>();
                }

                EditorUtility.SetDirty(gm);
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
            "EmergencyIsolateBtn",
            "GlassCracks",
            "BloodOverlay",
            "QuestionsPanel",
            "DialoguePanel",
            "NewspaperPanel",
            "VictoryPanel",
            "GameOverPanel",
            "PausePanel",
            "SettingsPanel",
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

        // Автоматически восстанавливаем пустые dossierSprite для всех VisitorData в проекте (поддержка старых и новых генераций)
        try
        {
            string[] visitorGuids = AssetDatabase.FindAssets("t:VisitorData");
            int repairedCount = 0;
            foreach (string guid in visitorGuids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                VisitorData vd = AssetDatabase.LoadAssetAtPath<VisitorData>(path);
                if (vd != null && vd.dossierSprite == null && vd.visitorSprite != null)
                {
                    vd.dossierSprite = vd.visitorSprite;
                    EditorUtility.SetDirty(vd);
                    repairedCount++;
                }
            }
            if (repairedCount > 0)
            {
                AssetDatabase.SaveAssets();
                Debug.Log($"<color=cyan>[Antigravity] Успешно восстановлены фото досье/паспорта для {repairedCount} персонажей!</color>");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[Antigravity] Ошибка автовосстановления фото персонажей: " + ex.Message);
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
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Debug.Log("<color=green>[Antigravity] ОКНО И ДИАЛОГОВАЯ ПАНЕЛЬ НАСТРОЕНЫ ИДЕАЛЬНО И СОХРАНЕНЫ!</color>");
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
