using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GrabbableStamp : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Настройки Штампа")]
    public bool isApproveStamp = true; // true = ОДОБРЕНО, false = ОТКАЗ
    public RectTransform passportArea;  // Область для отпускания (DocumentTray)
    public TMP_FontAsset vt323Font; // Шрифт для печати на паспорте
    
    
    // Начальное положение в ящике
    [HideInInspector] public Vector2 slotAnchoredPosition;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private GameManager gameManager;
    
    private Transform originalParent;
    private Vector3 originalScale;
    private GameObject shadowObj;
    private RectTransform shadowRt;
    
    private bool isReturning = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        gameManager = Object.FindAnyObjectByType<GameManager>();
        originalParent = transform.parent;
        originalScale = transform.localScale;
        slotAnchoredPosition = rectTransform.anchoredPosition;

        // Создаем процедурную 2D-тень для полета в воздухе
        CreateFlatShadow();
    }

    private void CreateFlatShadow()
    {
        shadowObj = new GameObject(name + "_Shadow");
        shadowObj.transform.SetParent(transform, false);
        shadowObj.transform.SetAsFirstSibling(); // Помещаем позади штампа

        Image parentImg = GetComponent<Image>();
        Image shadowImg = shadowObj.AddComponent<Image>();
        shadowImg.color = new Color(0f, 0f, 0f, 0.35f); // Полупрозрачный черный
        if (parentImg != null && parentImg.sprite != null)
        {
            shadowImg.sprite = parentImg.sprite;
            shadowImg.type = parentImg.type;
        }

        shadowRt = shadowObj.GetComponent<RectTransform>();
        shadowRt.anchorMin = Vector2.zero;
        shadowRt.anchorMax = Vector2.one;
        shadowRt.sizeDelta = Vector2.zero;
        shadowRt.localScale = Vector3.one;

        // По умолчанию тень плотно прижата к штампу (на столе)
        shadowRt.anchoredPosition = Vector2.zero;
        shadowObj.SetActive(false);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isReturning || !gameManager.enabled) return;

        // При поднятии штампа вытаскиваем его на корень Canvas, чтобы он рендерился поверх всего
        transform.SetParent(canvas.transform, true);
        canvasGroup.blocksRaycasts = false;

        // Эффект 2D-подъема: увеличиваем масштаб и активируем смещенную тень
        transform.localScale = originalScale * 1.22f;
        if (shadowObj != null)
        {
            shadowObj.SetActive(true);
            // Тень уходит глубже вниз-влево, показывая высоту
            shadowRt.anchoredPosition = new Vector2(-15f, -22f);
        }

        // Ретро-звук щелчка / взятия штампа
        PlaySound(0.5f, 900f, 0.05f); // Короткий высокочастотный щелчок
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isReturning) return;
        
        // Перемещение с учетом масштаба Canvas (разрешения экрана)
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isReturning) return;

        // Сбрасываем 2D-эффект полета
        transform.localScale = originalScale;
        if (shadowObj != null)
        {
            shadowRt.anchoredPosition = Vector2.zero;
            shadowObj.SetActive(false);
        }

        canvasGroup.blocksRaycasts = true;

        // Проверяем, попали ли на паспорт (DocumentTray)
        bool droppedOnPassport = false;
        if (passportArea != null && passportArea.gameObject.activeInHierarchy)
        {
            droppedOnPassport = RectTransformUtility.RectangleContainsScreenPoint(passportArea, eventData.position, eventData.pressEventCamera);
        }

        if (droppedOnPassport)
        {
            // Ставим 2D-печать на паспорт!
            ApplyStampMark(eventData.position, eventData.pressEventCamera);
            
            // Возвращаем инструмент в ящик
            StartCoroutine(ReturnToSlot());
            
            // Вызываем логику GameManager
            if (isApproveStamp)
            {
                gameManager.OnApproveClicked();
            }
            else
            {
                gameManager.OnRejectClicked();
            }
        }
        else
        {
            // Промахнулся: плавно возвращаем обратно в ящик
            StartCoroutine(ReturnToSlot());
            // Легкий щелчок неудачи
            PlaySound(0.2f, 300f, 0.08f);
        }
    }

    private void ApplyStampMark(Vector2 screenPos, Camera cam)
    {
        // 1. Создаем объект отпечатка чернил
        GameObject markObj = new GameObject("DynamicStampMark");
        markObj.transform.SetParent(passportArea, false);
        
        RectTransform markRt = markObj.AddComponent<RectTransform>();
        markRt.anchorMin = new Vector2(0.5f, 0.5f);
        markRt.anchorMax = new Vector2(0.5f, 0.5f);
        markRt.pivot = new Vector2(0.5f, 0.5f);
        markRt.sizeDelta = new Vector2(200f, 75f);
        markRt.localScale = Vector3.one;

        // Конвертируем экранные координаты мыши в локальные координаты паспорта
        RectTransformUtility.ScreenPointToLocalPointInRectangle(passportArea, screenPos, cam, out Vector2 localPoint);
        markRt.anchoredPosition = localPoint;

        // Добавляем случайный наклон для живости и реализма ручной печати
        markRt.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-12f, 12f));

        // 2. Рамка отпечатка (2D гранж рамка)
        Image borderImg = markObj.AddComponent<Image>();
        borderImg.color = isApproveStamp ? new Color(0.12f, 0.65f, 0.22f, 0.85f) : new Color(0.85f, 0.15f, 0.15f, 0.85f);
        
        // Создаем пиксельную текстуру рамки
        Texture2D borderTex = CreateStampBorderTexture(200, 75, borderImg.color);
        borderImg.sprite = Sprite.Create(borderTex, new Rect(0, 0, 200, 75), new Vector2(0.5f, 0.5f));

        // 3. Текст внутри печати
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(markObj.transform, false);
        
        RectTransform textRt = textObj.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.localScale = Vector3.one;

        TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
        tmp.text = isApproveStamp ? "ОДОБРЕНО" : "ОТКАЗАНО";
        tmp.fontSize = 26;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = borderImg.color;
        tmp.enableWordWrapping = false;

        // Устанавливаем шрифт VT323
        if (vt323Font != null) tmp.font = vt323Font;

        // 4. Играем сочный механический удар
        PlayHeavyThudSound();
    }

    private Texture2D CreateStampBorderTexture(int w, int h, Color color)
    {
        Texture2D tex = new Texture2D(w, h);
        tex.filterMode = FilterMode.Point;
        
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                // Рисуем рамку толщиной 4 пикселя с гранж-вырезами
                bool isBorder = (x < 4 || x > w - 5 || y < 4 || y > h - 5);
                
                if (isBorder)
                {
                    // Рандомный гранж-эффект (шум чернил)
                    if (Random.value > 0.18f)
                    {
                        tex.SetPixel(x, y, color);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
                else
                {
                    // Внутри рамки пусто (или легкие брызги чернил)
                    if (Random.value < 0.015f)
                    {
                        tex.SetPixel(x, y, new Color(color.r, color.g, color.b, 0.4f));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
        }
        tex.Apply();
        return tex;
    }

    private IEnumerator ReturnToSlot()
    {
        isReturning = true;

        // Re-parent to the original slot parent first, keeping the world position intact
        transform.SetParent(originalParent, true);
        
        // Ensure local scale returns to its exact original scale
        transform.localScale = originalScale;

        Vector2 startPos = rectTransform.anchoredPosition;
        float elapsed = 0f;
        float duration = 0.28f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Плавная кубическая интерполяция
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, slotAnchoredPosition, easeT);
            yield return null;
        }

        rectTransform.anchoredPosition = slotAnchoredPosition;
        isReturning = false;
    }

    private void PlayHeavyThudSound()
    {
        // Механический глухой металлический удар
        if (gameManager != null && gameManager.sfxAudioSource != null)
        {
            if (gameManager.shutterCloseSound != null)
            {
                // Проигрываем оригинальный звук шторки с повышенной мощностью и случайным питчем для живости
                float origPitch = gameManager.sfxAudioSource.pitch;
                gameManager.sfxAudioSource.pitch = Random.Range(0.7f, 0.85f); // Ниже тон = тяжелее звук
                gameManager.sfxAudioSource.PlayOneShot(gameManager.shutterCloseSound, 0.95f);
                
                // Возвращаем питч обратно через задержку
                gameManager.StartCoroutine(ResetPitch(origPitch, 0.5f));
            }
            else
            {
                // Запасной процедурный синтез
                PlaySound(0.8f, 120f, 0.25f);
            }
        }
    }

    private IEnumerator ResetPitch(float pitch, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (gameManager != null && gameManager.sfxAudioSource != null)
        {
            gameManager.sfxAudioSource.pitch = pitch;
        }
    }

    // Процедурный синтезатор звуков кликов / ударов через динамик
    private void PlaySound(float vol, float freq, float len)
    {
        GameObject audioObj = new GameObject("ProceduralSound");
        AudioSource src = audioObj.AddComponent<AudioSource>();
        src.volume = vol;
        src.pitch = freq / 440f;
        
        // Создаем микро-клик
        AudioClip clip = AudioClip.Create("click", 4000, 1, 44000, false);
        float[] samples = new float[4000];
        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / 44000f;
            if (t < len)
            {
                samples[i] = Mathf.Sin(2f * Mathf.PI * 440f * t) * (1f - t / len);
            }
            else
            {
                samples[i] = 0f;
            }
        }
        clip.SetData(samples, 0);
        src.PlayOneShot(clip);
        Destroy(audioObj, len + 0.5f);
    }
}
