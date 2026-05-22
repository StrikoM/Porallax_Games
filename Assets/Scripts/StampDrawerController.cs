using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StampDrawerController : MonoBehaviour
{
    [Header("Настройки Анимации")]
    public RectTransform drawerRt;
    public Button handleButton;
    public Vector2 closedAnchoredPosition = new Vector2(520f, -485f); // Скрыт за краем экрана
    public Vector2 openAnchoredPosition = new Vector2(520f, -340f);   // Полностью выдвинут
    public float slideDuration = 0.32f;

    private bool isOpen = false;
    private bool isMoving = false;
    private GameManager gameManager;

    void Awake()
    {
        if (drawerRt == null) drawerRt = GetComponent<RectTransform>();
        gameManager = Object.FindAnyObjectByType<GameManager>();

        // Устанавливаем в закрытое положение по умолчанию
        drawerRt.anchoredPosition = closedAnchoredPosition;

        if (handleButton != null)
        {
            handleButton.onClick.RemoveAllListeners();
            handleButton.onClick.AddListener(ToggleDrawer);
        }
    }

    public void ToggleDrawer()
    {
        if (isMoving || !enabled) return;
        StartCoroutine(SlideDrawer(!isOpen));
    }

    public void ForceClose()
    {
        if (isOpen && !isMoving)
        {
            StartCoroutine(SlideDrawer(false, true)); // Тихое принудительное закрытие
        }
    }

    private IEnumerator SlideDrawer(bool open, bool silent = false)
    {
        isMoving = true;
        Vector2 startPos = drawerRt.anchoredPosition;
        Vector2 targetPos = open ? openAnchoredPosition : closedAnchoredPosition;
        float elapsed = 0f;

        if (!silent)
        {
            // Проигрываем процедурный скрипучий звук выдвижения ящика стола!
            PlaySlideSound(open);
        }

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            // Плавное затухание скорости (Sine Ease Out)
            float easeT = Mathf.Sin(t * Mathf.PI * 0.5f);
            
            drawerRt.anchoredPosition = Vector2.Lerp(startPos, targetPos, easeT);
            yield return null;
        }

        drawerRt.anchoredPosition = targetPos;
        isOpen = open;
        isMoving = false;
    }

    private void PlaySlideSound(bool open)
    {
        if (gameManager != null && gameManager.sfxAudioSource != null)
        {
            // Если есть оригинальный звук телефона/шторки, мы можем сымитировать шорох
            if (gameManager.shutterOpenSound != null)
            {
                gameManager.sfxAudioSource.PlayOneShot(gameManager.shutterOpenSound, 0.35f);
            }
            else
            {
                // Процедурный синтез деревянного трения (низкочастотный рокочущий свист)
                GameObject audioObj = new GameObject("DrawerSlideSound");
                AudioSource src = audioObj.AddComponent<AudioSource>();
                src.volume = 0.28f;
                
                AudioClip clip = AudioClip.Create("slide", 8000, 1, 44000, false);
                float[] samples = new float[8000];
                System.Random rand = new System.Random();
                
                for (int i = 0; i < samples.Length; i++)
                {
                    float t = i / 44000f;
                    // Смешиваем низкочастотную волну и фильтрованный белый шум для эффекта трения дерева
                    float noise = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.15f;
                    float wave = Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.5f;
                    
                    float envelope = 1f - t / 0.18f;
                    if (envelope < 0) envelope = 0;
                    
                    samples[i] = (wave + noise) * envelope;
                }
                clip.SetData(samples, 0);
                src.PlayOneShot(clip);
                Destroy(audioObj, 1f);
            }
        }
    }
}
