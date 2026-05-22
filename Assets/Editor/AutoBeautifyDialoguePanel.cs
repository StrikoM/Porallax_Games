using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

[InitializeOnLoad]
public class AutoBeautifyDialoguePanel
{
    static AutoBeautifyDialoguePanel()
    {
        EditorApplication.delayCall += RunOnce;
    }

    [MenuItem("Parallax/BEAUTIFY DIALOGUE PANEL")]
    public static void ManualBeautify()
    {
        EditorPrefs.DeleteKey("AutoBeautifyDialoguePanel_v5");
        RunOnce();
    }

    static void RunOnce()
    {
        if (Application.isPlaying) return;
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "GameScene") return;
        if (EditorPrefs.GetBool("AutoBeautifyDialoguePanel_v5", false)) return;
        EditorPrefs.SetBool("AutoBeautifyDialoguePanel_v5", true);

        GameObject diagPanel = GameObject.Find("DialoguePanel");
        if (diagPanel == null)
        {
            Debug.LogWarning("[Antigravity] Панель DialoguePanel не найдена на сцене!");
            return;
        }

        // Загружаем премиальный ретро-пиксельный шрифт VT323 из ресурсов проекта
        TMP_FontAsset vt323Font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Visitors/VT323-Regular SDF.asset");
        if (vt323Font == null)
        {
            Debug.LogWarning("[Antigravity] Шрифт VT323-Regular SDF не найден по пути Assets/Visitors/VT323-Regular SDF.asset!");
        }

        // 1. СТИЛИЗАЦИЯ ГЛАВНОЙ ПАНЕЛИ ДИАЛОГА (DialoguePanel)
        Image panelImage = diagPanel.GetComponent<Image>();
        if (panelImage == null) panelImage = diagPanel.AddComponent<Image>();
        
        // Создаем роскошный процедурный фон для ЭЛТ-монитора диалога (глубокий темно-зеленый с сеткой и светящейся границей)
        Texture2D panelTex = CreateRetroCRTTexture(512, 128, new Color(0.04f, 0.05f, 0.04f, 0.95f), new Color(0f, 0.7f, 0.2f, 1f));
        Sprite panelSprite = Sprite.Create(panelTex, new Rect(0, 0, panelTex.width, panelTex.height), new Vector2(0.5f, 0.5f));
        panelImage.sprite = panelSprite;
        panelImage.type = Image.Type.Simple;
        panelImage.color = Color.white;

        RectTransform panelRT = diagPanel.GetComponent<RectTransform>();
        if (panelRT != null)
        {
            panelRT.anchorMin = new Vector2(0.5f, 0f);
            panelRT.anchorMax = new Vector2(0.5f, 0f);
            panelRT.pivot = new Vector2(0.5f, 0f);
            panelRT.anchoredPosition = new Vector2(0, 50); // Чуть приподнимем над краем для парящего вида
            panelRT.sizeDelta = new Vector2(1200, 240);
        }

        // Добавим стильное ретро-свечение (Outline)
        Outline panelOutline = diagPanel.GetComponent<Outline>();
        if (panelOutline == null) panelOutline = diagPanel.AddComponent<Outline>();
        panelOutline.effectColor = new Color(0f, 0.7f, 0.2f, 0.4f);
        panelOutline.effectDistance = new Vector2(4, -4);

        // 2. СТИЛИЗАЦИЯ РАМКИ ПОРТРЕТА (Portrait)
        Transform portraitTr = diagPanel.transform.Find("Portrait");
        if (portraitTr != null)
        {
            Image portImg = portraitTr.GetComponent<Image>();
            if (portImg != null)
            {
                // Настраиваем красивую рамку видоискателя для портрета посетителя
                Texture2D portTex = CreateScopeTexture(128, 128, new Color(0.06f, 0.08f, 0.06f, 0.8f), new Color(0f, 0.7f, 0.2f, 1f));
                Sprite portSprite = Sprite.Create(portTex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f));
                
                // Чтобы сам портрет был красивой иконкой внутри рамки, мы можем стилизовать рамку вокруг него
                // Но так как Portrait непосредственно содержит спрайт лица посетителя, мы создаем обводку
                Outline portOutline = portraitTr.gameObject.GetComponent<Outline>();
                if (portOutline == null) portOutline = portraitTr.gameObject.AddComponent<Outline>();
                portOutline.effectColor = new Color(0f, 0.7f, 0.2f, 0.8f);
                portOutline.effectDistance = new Vector2(3, -3);

                RectTransform portRT = portraitTr.GetComponent<RectTransform>();
                if (portRT != null)
                {
                    portRT.anchorMin = new Vector2(0f, 0.5f);
                    portRT.anchorMax = new Vector2(0f, 0.5f);
                    portRT.pivot = new Vector2(0f, 0.5f);
                    portRT.anchoredPosition = new Vector2(30, 0); // Сдвигаем вбок для аккуратности
                    portRT.sizeDelta = new Vector2(180, 180);
                }
            }
        }

        // 3. СТИЛИЗАЦИЯ ИМЕНИ (NameLabel)
        Transform nameLabelTr = diagPanel.transform.Find("NameLabel");
        if (nameLabelTr != null)
        {
            Image labelImg = nameLabelTr.GetComponent<Image>();
            if (labelImg == null) labelImg = nameLabelTr.gameObject.AddComponent<Image>();

            // Красивая плашка под имя (оранжевое ретро-свечение)
            Texture2D labelTex = CreateRetroCRTTexture(128, 64, new Color(0.08f, 0.05f, 0.03f, 0.95f), new Color(1f, 0.5f, 0.1f, 1f));
            Sprite labelSprite = Sprite.Create(labelTex, new Rect(0, 0, 128, 64), new Vector2(0.5f, 0.5f));
            labelImg.sprite = labelSprite;
            labelImg.type = Image.Type.Simple;
            labelImg.color = Color.white;

            RectTransform labelRT = nameLabelTr.GetComponent<RectTransform>();
            if (labelRT != null)
            {
                labelRT.anchorMin = new Vector2(0f, 1f);
                labelRT.anchorMax = new Vector2(0f, 1f);
                labelRT.pivot = new Vector2(0f, 0.5f);
                labelRT.anchoredPosition = new Vector2(30, 0); // Идеально ложится на левый верхний угол
                labelRT.sizeDelta = new Vector2(180, 45);
            }

            // Настраиваем сам текст имени (NameText)
            TextMeshProUGUI nameTxt = nameLabelTr.GetComponentInChildren<TextMeshProUGUI>(true);
            if (nameTxt != null)
            {
                if (vt323Font != null) nameTxt.font = vt323Font;
                nameTxt.fontSize = 28;
                nameTxt.alignment = TextAlignmentOptions.Center;
                nameTxt.color = new Color(1f, 0.6f, 0.1f); // Эмбер/янтарный цвет
                nameTxt.fontStyle = FontStyles.Bold;
                
                Outline nameOutline = nameTxt.gameObject.GetComponent<Outline>();
                if (nameOutline == null) nameOutline = nameTxt.gameObject.AddComponent<Outline>();
                nameOutline.effectColor = new Color(1f, 0.5f, 0.1f, 0.5f);
                nameOutline.effectDistance = new Vector2(1.5f, -1.5f);
            }
        }

        // 4. СТИЛИЗАЦИЯ СОДЕРЖИМОГО ДИАЛОГА (DialogueContent)
        Transform contentTr = diagPanel.transform.Find("DialogueContent");
        if (contentTr != null)
        {
            TextMeshProUGUI contentTxt = contentTr.GetComponent<TextMeshProUGUI>();
            if (contentTxt != null)
            {
                if (vt323Font != null) contentTxt.font = vt323Font;
                contentTxt.fontSize = 44; // Увеличиваем размер ретро-пикселей для превосходной читаемости
                contentTxt.alignment = TextAlignmentOptions.TopLeft;
                contentTxt.color = new Color(0.2f, 1f, 0.3f); // Ярко-зеленый люминофор CRT
                contentTxt.characterSpacing = 1.2f;
                contentTxt.lineSpacing = 10f;

                Outline contentOutline = contentTr.gameObject.GetComponent<Outline>();
                if (contentOutline == null) contentOutline = contentTr.gameObject.AddComponent<Outline>();
                contentOutline.effectColor = new Color(0f, 0.8f, 0.2f, 0.4f);
                contentOutline.effectDistance = new Vector2(2, -2);

                RectTransform contentRT = contentTr.GetComponent<RectTransform>();
                if (contentRT != null)
                {
                    contentRT.anchorMin = new Vector2(0f, 0f);
                    contentRT.anchorMax = new Vector2(1f, 1f);
                    contentRT.pivot = new Vector2(0.5f, 0.5f);
                    // Оставляем красивые отступы: слева 240 (чтобы не наезжать на фото), сверху 50, справа 40, снизу 20
                    contentRT.offsetMin = new Vector2(240, 25);
                    contentRT.offsetMax = new Vector2(-40, -45);
                }
            }
        }

        EditorUtility.SetDirty(diagPanel);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("<color=green>[Antigravity] Ретро-диалоговая ЭЛТ-панель успешно стилизована с премиальным пиксельным шрифтом VT323!</color>");
    }

    // Создает потрясающую текстуру в стиле терминала Fallout / ЭЛТ-экранов с зеленой/оранжевой рамкой и фоновой сеткой
    private static Texture2D CreateRetroCRTTexture(int width, int height, Color bgColor, Color borderColor)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Основной цвет фона
                Color c = bgColor;

                // Градиент к углам (эффект выпуклости ЭЛТ-экрана)
                float dx = Mathf.Abs(x - width / 2f) / (width / 2f);
                float dy = Mathf.Abs(y - height / 2f) / (height / 2f);
                float dist = Mathf.Max(dx, dy);
                c = Color.Lerp(c, c * 0.4f, dist * dist); // Темнеет к краям

                // Наложение ЭЛТ-строк развертки (scanlines)
                if (y % 4 == 0)
                {
                    c = Color.Lerp(c, Color.black, 0.15f);
                }

                // Внутренняя сетка интерфейса (subtle grid)
                if ((x % 16 == 0 || y % 16 == 0) && x > 6 && x < width - 6 && y > 6 && y < height - 6)
                {
                    c = Color.Lerp(c, borderColor, 0.04f);
                }

                // Скругленные углы и неоновая рамка
                bool isBorder = (x < 5 || x > width - 6 || y < 5 || y > height - 6);
                if (isBorder)
                {
                    // Делаем аккуратное скругление углов
                    bool isCorner = ((x < 12 && y < 12) || (x > width - 13 && y < 12) || (x < 12 && y > height - 13) || (x > width - 13 && y > height - 13));
                    if (isCorner)
                    {
                        // Вычисляем дистанцию до центра скругления
                        float cx = (x < 12) ? 12 : width - 13;
                        float cy = (y < 12) ? 12 : height - 13;
                        float r = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                        
                        if (r > 8f)
                        {
                            c = new Color(0, 0, 0, 0); // Прозрачный угол
                        }
                        else if (r > 6f)
                        {
                            c = borderColor; // Рамка на скруглении
                        }
                    }
                    else
                    {
                        c = borderColor; // Обычная рамка
                    }
                }

                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
        return texture;
    }

    // Создает текстуру видоискателя/рамки портрета
    private static Texture2D CreateScopeTexture(int width, int height, Color bgColor, Color borderColor)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color c = bgColor;

                // Добавляем ретро перекрестие по углам (Scope corners)
                bool isScope = (x < 15 && (y < 3 || y > height - 4)) || (x > width - 16 && (y < 3 || y > height - 4)) ||
                               (y < 15 && (x < 3 || x > width - 4)) || (y > height - 16 && (x < 3 || x > width - 4));
                               
                if (isScope)
                {
                    c = borderColor;
                }

                texture.SetPixel(x, y, c);
            }
        }

        texture.Apply();
        return texture;
    }
}
