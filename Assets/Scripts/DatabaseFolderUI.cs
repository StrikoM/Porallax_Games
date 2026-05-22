using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// Этот скрипт управляет окном "Папка Базы Данных"
// Он показывает список настоящих граждан (только людей, НЕ монстров)
public class DatabaseFolderUI : MonoBehaviour
{
    [Header("Корневая панель (скрыть/показать)")]
    public GameObject folderPanel;

    [Header("Отображение карточки")]
    public Image folderVisitorImage;
    public TextMeshProUGUI folderNameText;
    public TextMeshProUGUI folderIdText;
    public TextMeshProUGUI folderEyesText;
    public TextMeshProUGUI folderExpDateText; // Срок действия в БД
    public TextMeshProUGUI pageCounterText;  // "Запись 1 из 5"

    [Header("Кнопки листания")]
    public Button prevButton;
    public Button nextButton;

    // Внутренние данные
    private List<VisitorData> registeredCitizens = new List<VisitorData>();
    private int currentPage = 0;

    void Awake()
    {
        if (folderPanel != null)
            folderPanel.SetActive(false);
    }

    // Загружаем только ЛЮДЕЙ из текущей смены
    public void LoadShiftCitizens(VisitorData[] shiftVisitors)
    {
        registeredCitizens.Clear();
        foreach (var v in shiftVisitors)
        {
            if (v != null)
                registeredCitizens.Add(v);
        }
        currentPage = 0;
    }

    private bool isAnimating = false;

    // Открыть/закрыть окно папки
    public void ToggleFolder()
    {
        if (folderPanel == null || isAnimating) return;
        bool willOpen = !folderPanel.activeSelf;
        StartCoroutine(AnimateFolder(willOpen));
    }

    public void CloseFolder()
    {
        if (folderPanel != null && folderPanel.activeSelf && !isAnimating) 
        {
            StartCoroutine(AnimateFolder(false));
        }
    }

    // Следующая запись
    public void NextPage()
    {
        if (registeredCitizens.Count == 0 || isAnimating) return;
        currentPage = (currentPage + 1) % registeredCitizens.Count;
        StartCoroutine(AnimatePageTurn());
    }

    // Предыдущая запись
    public void PrevPage()
    {
        if (registeredCitizens.Count == 0 || isAnimating) return;
        currentPage = (currentPage - 1 + registeredCitizens.Count) % registeredCitizens.Count;
        StartCoroutine(AnimatePageTurn());
    }

    // Анимация открытия/закрытия папки (масштабирование)
    private System.Collections.IEnumerator AnimateFolder(bool open)
    {
        isAnimating = true;
        RectTransform rect = folderPanel.GetComponent<RectTransform>();
        
        if (open)
        {
            folderPanel.SetActive(true);
            ShowCurrentPage();
            rect.localScale = new Vector3(0f, 1f, 1f); // Начинаем с плоского состояния
        }
        
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = open ? new Vector3(0f, 1f, 1f) : Vector3.one;
        Vector3 targetScale = open ? Vector3.one : new Vector3(0f, 1f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rect.localScale = Vector3.Lerp(startScale, targetScale, elapsed / duration);
            yield return null;
        }

        rect.localScale = targetScale;
        
        if (!open)
        {
            folderPanel.SetActive(false);
        }
        isAnimating = false;
    }

    // Анимация перелистывания страницы (схлопывание и раскрытие)
    private System.Collections.IEnumerator AnimatePageTurn()
    {
        isAnimating = true;
        RectTransform rect = folderPanel.GetComponent<RectTransform>();
        
        float duration = 0.1f; // Очень быстрое схлопывание ЭЛТ
        float elapsed = 0f;
        
        // 1. Схлопываем по ВЕРТИКАЛИ (эффект потери синхронизации/выключения)
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rect.localScale = Vector3.Lerp(Vector3.one, new Vector3(1f, 0.05f, 1f), elapsed / duration);
            
            // Глитч: текст мигает белым!
            if (folderNameText != null) folderNameText.color = Random.value > 0.5f ? Color.white : Color.green;
            yield return null;
        }
        
        // В середине анимации меняем текст и картинки на новые
        ShowCurrentPage();
        
        // 2. Раскрываем с небольшим отскоком (восстановление сигнала)
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float y = Mathf.Lerp(0.05f, 1.05f, t); // Прыжок чуть больше 1
            rect.localScale = new Vector3(1f, y, 1f);
            
            if (folderNameText != null) folderNameText.color = Color.green; // Возвращаем цвет
            yield return null;
        }
        
        rect.localScale = Vector3.one;
        isAnimating = false;
    }

    void ShowCurrentPage()
    {
        if (registeredCitizens.Count == 0)
        {
            if (folderNameText) folderNameText.text = "БД ПУСТА";
            if (pageCounterText) pageCounterText.text = "0 из 0";
            return;
        }

        VisitorData citizen = registeredCitizens[currentPage];

        if (folderVisitorImage != null)
        {
            // Сначала пытаемся загрузить фото из базы данных (нормальное)
            if (citizen.dossierSprite != null)
            {
                folderVisitorImage.sprite = citizen.dossierSprite;
                folderVisitorImage.color = Color.white; // Обязательно ставим белый цвет, чтобы не было черного силуэта
            }
            // Если его нет, грузим обычное фото
            else if (citizen.visitorSprite != null)
            {
                folderVisitorImage.sprite = citizen.visitorSprite;
                folderVisitorImage.color = Color.white;
            }
            else
            {
                folderVisitorImage.color = new Color(1, 1, 1, 0); // Делаем прозрачным, если фото вообще нет
            }
        }

        // Генерируем красивую процедурную дополнительную информацию для CRT-монитора
        string gender = (citizen.dossierName.EndsWith("a") || citizen.dossierName.EndsWith("а") || citizen.dossierName.EndsWith("инична") || citizen.dossierName.EndsWith("овна")) ? "ЖЕНСКИЙ" : "МУЖСКОЙ";
        string accessCategory = (citizen.dossierName.Length % 3 == 0) ? "КАТЕГОРИЯ A (ПРИОРИТЕТНАЯ)" : ((citizen.dossierName.Length % 3 == 1) ? "КАТЕГОРИЯ B (РАБОЧАЯ)" : "КАТЕГОРИЯ C (ВРЕМЕННАЯ)");
        string bioScan = "ПРОЙДЕНА [OK]";
        
        string region = "СЕКТОР " + (Mathf.Abs(citizen.dossierName.GetHashCode()) % 9 + 1) + "-A";
        string threatLevel = (citizen.dossierName.Length % 4 == 0) ? "НИЗКИЙ (БЕЗОПАСЕН)" : ((citizen.dossierName.Length % 4 == 1) ? "УМЕРЕННЫЙ (СТАНДАРТ)" : "ВЫСОКИЙ (ПОВЫШЕННЫЙ КОНТРОЛЬ)");
        
        int height = 160 + (Mathf.Abs(citizen.dossierName.GetHashCode()) % 35);
        int weight = 50 + (height - 150) + (Mathf.Abs(citizen.dossierName.GetHashCode() + 7) % 20) - 10;
        string purpose = (citizen.dossierName.Length % 2 == 0) ? "ТРАНЗИТ / РАБОТА" : "ВОЗВРАЩЕНИЕ ДОМОЙ";
        
        int loyalty = 70 + (Mathf.Abs(citizen.dossierName.GetHashCode() + 13) % 31);
        string[] bloodTypes = { "O(I) Rh+", "A(II) Rh+", "B(III) Rh+", "AB(IV) Rh+", "O(I) Rh-", "A(II) Rh-", "B(III) Rh-", "AB(IV) Rh-" };
        string blood = bloodTypes[Mathf.Abs(citizen.dossierName.GetHashCode()) % bloodTypes.Length];
        
        if (folderNameText != null) 
            folderNameText.text = "ИМЯ: " + citizen.dossierName + "\n<size=22><color=#007700>ПОЛ: " + gender + "   |   КЛАСС: " + accessCategory + "\nБИОМЕТРИЯ: " + bioScan + "</color></size>";
            
        if (folderIdText != null) 
            folderIdText.text = "ID: " + citizen.dossierId + "\n<size=22><color=#007700>РЕГИОН: " + region + "   |   УГРОЗА: " + threatLevel + "</color></size>";
        
        int currentShiftIndex = PlayerPrefs.GetInt("CurrentShift", 0);
        
        if (folderEyesText != null) 
        {
            if (currentShiftIndex >= 1) 
                folderEyesText.text = "ГЛАЗА: " + citizen.dossierEyes + "\n<size=22><color=#007700>РОСТ: " + height + " см   |   ВЕС: " + weight + " кг\nЦЕЛЬ: " + purpose + "</color></size>";
            else 
                folderEyesText.text = "<size=22><color=#005500>[ДАННЫЕ ГЛАЗ/БИОМЕТРИИ: ДОСТУПНО С 1-Й СМЕНЫ]</color></size>";
        }
        
        if (folderExpDateText != null) 
        {
            if (currentShiftIndex >= 2) 
                folderExpDateText.text = "ГОДЕН ДО: " + citizen.dossierExpDate + "\n<size=22><color=#007700>ГРУППА КРОВИ: " + blood + "   |   ЛОЯЛЬНОСТЬ: " + loyalty + "%</color></size>";
            else 
                folderExpDateText.text = "<size=22><color=#005500>[ДАТА ГОДНОСТИ/МЕД.КАРТА: ДОСТУПНО СО 2-Й СМЕНЫ]</color></size>";
        }
        
        if (pageCounterText != null) pageCounterText.text = "ЗАПИСЬ " + (currentPage + 1) + " ИЗ " + registeredCitizens.Count;
    }
}
