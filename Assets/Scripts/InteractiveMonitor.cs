using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InteractiveMonitor : MonoBehaviour
{
    public RectTransform monitorRect;
    
    // Позиция на столе (которую мы так долго искали)
    public Vector2 deskPosition = new Vector2(650, -90);
    public Vector2 deskSize = new Vector2(350, 280); 
    
    public Vector2 centerPosition = new Vector2(0, 0);
    public Vector2 centerSize = new Vector2(1600, 900);
    
    public GameObject contentPanel; 
    public GameObject blinkText;    
    
    private bool isZoomed = false;
    private bool isAnimating = false;

    void Awake()
    {
        // Игнорируем то, что застряло в инспекторе, и жестко прописываем координаты тут!
        deskPosition = new Vector2(650, -90);

        // Автоматически подписываемся на клик, чтобы не зависеть от инспектора
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnMonitorClicked);
            Debug.Log("[InteractiveMonitor] Кнопка найдена и подключена!");
        }
        else
        {
            Debug.LogError("[InteractiveMonitor] ОШИБКА: Компонент Button не найден на объекте!");
        }
    }

    void Start()
    {
        if (monitorRect == null) monitorRect = GetComponent<RectTransform>();
        
        monitorRect.anchoredPosition = deskPosition;
        monitorRect.sizeDelta = deskSize;
        
        if (contentPanel != null) contentPanel.SetActive(false);
        if (blinkText != null) blinkText.SetActive(true);
        
        StartCoroutine(BlinkRoutine());
    }

    public void OnMonitorClicked()
    {
        Debug.Log("--- [InteractiveMonitor] Клик зарегистрирован! ---");
        if (isAnimating || isZoomed) return;
        StartCoroutine(AnimateMonitor(deskPosition, deskSize, centerPosition, centerSize, true));
    }

    public void OnCloseClicked()
    {
        Debug.Log("--- [InteractiveMonitor] Нажата кнопка ВЫКЛ ---");
        if (isAnimating || !isZoomed) return;
        StartCoroutine(AnimateMonitor(centerPosition, centerSize, deskPosition, deskSize, false));
    }

    private IEnumerator AnimateMonitor(Vector2 startPos, Vector2 startSize, Vector2 endPos, Vector2 endSize, bool zoomingIn)
    {
        isAnimating = true;
        
        if (zoomingIn)
        {
            if (blinkText != null) blinkText.SetActive(false);
            transform.SetAsLastSibling(); 
        }
        else
        {
            if (contentPanel != null) contentPanel.SetActive(false);
        }

        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = 1f - Mathf.Pow(1f - t, 3f);
            
            monitorRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            monitorRect.sizeDelta = Vector2.Lerp(startSize, endSize, t);
            yield return null;
        }

        monitorRect.anchoredPosition = endPos;
        monitorRect.sizeDelta = endSize;

        if (zoomingIn)
        {
            if (contentPanel != null) contentPanel.SetActive(true);
            isZoomed = true;
        }
        else
        {
            if (blinkText != null) blinkText.SetActive(true);
            isZoomed = false;
        }
        
        isAnimating = false;
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            if (!isZoomed && blinkText != null)
            {
                blinkText.SetActive(!blinkText.activeSelf);
            }
            yield return new WaitForSeconds(0.8f);
        }
    }
}
