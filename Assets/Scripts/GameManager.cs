using System.Collections;
using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("База данных уровней")]
    public ShiftData[] shiftsDatabase; 
    private ShiftData currentShift;
    private int currentShiftIndex = 0;
    private int currentVisitorIndex = 0;

    [Header("Интерфейс (UI)")]
    public Image visitorImageDisplay;
    public TextMeshProUGUI directiveTextDisplay;
    public DatabaseFolderUI databaseFolder; // Папка с базой данных

    [Header("Новая Анимация (Стиль Окна)")]
    public RectTransform windowShutter; // Металлическая шторка на окне (если есть)
    public RectTransform documentTray;  // Лоток/папка с документами (выезжает снизу)
    public RectTransform guardLeft;     // Охранник слева
    public RectTransform guardRight;    // Охранник справа
    [Header("Спрайты Охранников (Анимации)")]
    public Sprite guardStandingSprite;  // Стоит / Говорит
    public Sprite guardWalkingSprite;   // Идет
    public Sprite guardHoldingSprite;   // Держит монстра (изоляция)
    
    private Vector3 originalShutterPos;
    private Vector3 originalTrayPos;
    private Vector3 guardLeftStartPos;
    private Vector3 guardRightStartPos;
    private Vector3 originalGuardRightScale = Vector3.one;
    
    [Header("Досье (База Данных)")]
    public TextMeshProUGUI dossierNameText;
    public TextMeshProUGUI dossierLastNameText;
    public TextMeshProUGUI dossierIdText;
    public TextMeshProUGUI dossierEyesText;
    public Image dossierPhotoDisplay;

    [Header("Паспорт (Документ)")]
    public TextMeshProUGUI passportNameText;
    public TextMeshProUGUI passportLastNameText;
    public TextMeshProUGUI passportIdText;
    public TextMeshProUGUI passportEyesText;
    public TextMeshProUGUI passportExpDateText; // Новое поле для даты
    
    [Header("Правила игры")]
    public TextMeshProUGUI strikesTextDisplay; // Теперь это штрафы, а не лояльность
    public TextMeshProUGUI quotaTextDisplay;   // Очередь вместо таймера
    public int strikes = 0;                    // 3 штрафа = увольнение
    private bool isShiftActive = true;
    private bool isAnimating = false;          // Блокировка кнопок во время анимации
    private Sprite defaultGuardSprite;         // Обычная картинка охранника

    [Header("Экраны завершения")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverReasonText;
    public GameObject victoryPanel;
    public TextMeshProUGUI victoryStatsText;

    [Header("Система Печатей")]
    public GameObject stampObject;
    public TextMeshProUGUI stampText;
    public UnityEngine.UI.Outline stampOutline;

    [Header("Диалоги (Аниме стиль)")]
    public GameObject dialoguePanel;
    public Image dialoguePortrait;
    public Sprite dispatcherSprite; // Спрайт для Службы безопасности (Диспетчера)
    public TextMeshProUGUI dialogueNameText;
    public TextMeshProUGUI dialogueContentText;

    [Header("Звуки")]
    public AudioSource sfxAudioSource;     // Источник звуков (SFX)
    public AudioClip shutterCloseSound;    // Звук падения шторки
    public AudioClip shutterOpenSound;     // Звук открытия шторки (необязательно)

    [Header("Пауза и Меню")]
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button exitButton;
    private bool isPaused = false;

    [Header("Кнопка Допроса")]
    public Button interrogateBtn;
    public GameObject questionsPanel; // Панель с вариантами вопросов

    [Header("Настройки")]
    public GameObject settingsPanel;
    public UnityEngine.UI.Slider musicSlider;
    public UnityEngine.UI.Slider sfxSlider;


    [Header("Телефон (Сюжет)")]
    public Button phoneButton;
    public string[] shiftPhoneMessages; 
    public AudioClip phoneRingSound;
    public AudioClip phonePickupSound;
    private bool isPhoneRinging = false;
    private bool phoneAnswered = false;

    [Header("Защита (Шокер)")]
    public GameObject glassCracksOverlay;
    public GameObject stunGunDrawer;
    public Button stunGunButton;
    public Image screenFlashOverlay;
    public int maxStunCharges = 1;
    private int currentStunCharges = 1;

    [Header("Кровь и Тряпка")]
    public CanvasGroup bloodOverlay;
    public RectTransform ragCursor;
    private bool isBloodOnGlass = false;
    private float cleaningProgress = 0f;
    private Vector2 lastMousePos;
    private bool isBloodNext = false;

    [Header("Дезинфекция")]
    public CanvasGroup deconGasOverlay;
    public AudioClip deconGasSound;

    [Header("Монстры: Трейты")]
    public AudioClip glassKnockSound;
    private bool isVisitorWaiting = false;
    private float visitorTimer = 0f;
    private int knockStage = 0;

    // Данные для анимации
    private Vector3 originalVisitorPos;

    void Start()
    {
        // Создание текста для охранника удалено, так как теперь он использует общую панель диалогов.

        // Читаем из сохранений, на каком мы дне
        currentShiftIndex = PlayerPrefs.GetInt("CurrentShift", 0);
        currentStunCharges = maxStunCharges;

        // Прячем экраны при старте
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);

        // Гарантируем, что оверлеи не будут блокировать мышь на протяжении всей игры
        DisableRaycastOnOverlay("DecontaminationGas");
        DisableRaycastOnOverlay("BloodOverlay");
        DisableRaycastOnOverlay("GlassCracks");
        DisableRaycastOnOverlay("ScreenFlash");
        DisableRaycastOnOverlay("CRT_Overlay_Safe");
        DisableRaycastOnOverlay("GlobalDarkness");

        if (shiftsDatabase == null || shiftsDatabase.Length == 0)
        {
            Debug.LogError("База Смен пуста! Добавьте смены в GameManager.");
            return;
        }

        if (currentShiftIndex >= shiftsDatabase.Length)
        {
            // Игрок прошел все уровни! 
            // Чтобы он не застревал на экране победы навсегда, сбросим прогресс
            // и дадим возможность играть снова с 1-й смены!
            PlayerPrefs.SetInt("CurrentShift", 0);
            PlayerPrefs.Save();
            currentShiftIndex = 0;
            Debug.Log("[GameManager] Прогресс сброшен на 1 смену для повторной игры.");
        }

        currentShift = shiftsDatabase[currentShiftIndex];
        if (currentShift == null)
        {
            Debug.LogError("ОШИБКА: Смена " + currentShiftIndex + " не назначена в GameManager (пустое поле)! Пожалуйста, перетащите файл смены в массив Shifts Database.");
            return;
        }
        
        if (directiveTextDisplay != null) directiveTextDisplay.text = "ЗАМЕТКА УПРАВДОМА:\n" + currentShift.directiveText;

        // Загружаем граждан в папку
        if (databaseFolder != null)
            databaseFolder.LoadShiftCitizens(currentShift.shiftVisitors);
        Debug.Log("Загружена: " + currentShift.shiftName + ". Директива: " + currentShift.directiveText);

        if (visitorImageDisplay != null)
        {
            originalVisitorPos = visitorImageDisplay.rectTransform.anchoredPosition;
            // Делаем посетителя всегда полностью видимым по прозрачности
            Color c = visitorImageDisplay.color;
            c.a = 1f;
            visitorImageDisplay.color = c;
        }
        
        if (windowShutter != null) originalShutterPos = windowShutter.anchoredPosition;
        if (documentTray != null) originalTrayPos = documentTray.anchoredPosition;
        if (guardLeft != null) 
        {
            guardLeftStartPos = guardLeft.anchoredPosition;
            defaultGuardSprite = guardStandingSprite != null ? guardStandingSprite : guardLeft.GetComponent<Image>().sprite;
            if (guardStandingSprite != null) guardLeft.GetComponent<Image>().sprite = guardStandingSprite;
        }
        if (guardRight != null) 
        {
            guardRightStartPos = guardRight.anchoredPosition;
            originalGuardRightScale = guardRight.localScale;
            if (guardStandingSprite != null) guardRight.GetComponent<Image>().sprite = guardStandingSprite;
        }

        UpdateUI();
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        
        // Проверяем, есть ли сообщение от босса для этой смены
        if (shiftPhoneMessages != null && currentShiftIndex < shiftPhoneMessages.Length && !string.IsNullOrEmpty(shiftPhoneMessages[currentShiftIndex]))
        {
            StartCoroutine(PhoneRingRoutine());
        }
        else
        {
            ShowVisitor(currentVisitorIndex);
        }
    }

    void UpdateUI()
    {
        // Если у нас в инспекторе все еще висит старый текст лояльности/таймера,
        // мы можем временно использовать их под новые нужды.
        if (strikesTextDisplay != null)
            strikesTextDisplay.text = "Штрафы: " + strikes + "/3";
            
        if (quotaTextDisplay != null && currentShift != null)
        {
            int left = currentShift.shiftVisitors.Length - currentVisitorIndex;
            quotaTextDisplay.text = "Очередь: " + Mathf.Max(0, left);
        }
    }

    void ShowVisitor(int index)
    {
        if (currentShift == null || currentShift.shiftVisitors.Length == 0) return;

        if (index >= currentShift.shiftVisitors.Length)
        {
            // Прошли уровень успешно! Сохраняем прогресс (переход на следующий день)
            PlayerPrefs.SetInt("CurrentShift", currentShiftIndex + 1);
            PlayerPrefs.Save();
            
            if (quotaTextDisplay != null) quotaTextDisplay.text = "Очередь: 0";
            
            EndShift("Смена окончена! Все проверены.");
            return;
        }

        VisitorData currentVisitor = currentShift.shiftVisitors[index];

        if (visitorImageDisplay != null)
        {
            visitorImageDisplay.sprite = currentVisitor.visitorSprite;
            visitorImageDisplay.color = new Color(1f, 1f, 1f, 1f); // Возвращаем видимость!
        }
        
        // Убираем Досье (2-я информация), чтобы не дублировать
        if (dossierNameText != null) dossierNameText.text = "";
        if (dossierLastNameText != null) dossierLastNameText.text = "";
        if (dossierIdText != null) dossierIdText.text = "";
        if (dossierEyesText != null) dossierEyesText.text = "";
        if (dossierPhotoDisplay != null) 
        {
            dossierPhotoDisplay.sprite = currentVisitor.dossierSprite;
            dossierPhotoDisplay.color = currentVisitor.dossierSprite == null ? new Color(1,1,1,0) : Color.white;
        }

        // Заполняем только Паспорт (ИМЯ и ФАМИЛИЯ разделены)
        string pName = currentVisitor.passportName;
        string pFirst = pName;
        string pLast = "";
        if (!string.IsNullOrEmpty(pName))
        {
            int spaceIdx = pName.IndexOf(' ');
            if (spaceIdx > 0)
            {
                pFirst = pName.Substring(0, spaceIdx);
                pLast = pName.Substring(spaceIdx + 1);
            }
        }
        
        if (passportNameText != null) passportNameText.text = pFirst;
        if (passportLastNameText != null) passportLastNameText.text = pLast;
        if (passportIdText != null) passportIdText.text = "ID:\n" + currentVisitor.passportId;
        
        // Показываем глаза только если это Смена 2 или выше (Индекс 1+)
        if (passportEyesText != null) 
        {
            if (currentShiftIndex >= 1) passportEyesText.text = currentVisitor.passportEyes;
            else passportEyesText.text = ""; // Прячем для первой смены
        }
        
        // Показываем срок действия всегда на бумажке
        if (passportExpDateText != null) 
        {
            string exp = currentVisitor.passportExpDate;
            if (string.IsNullOrEmpty(exp)) exp = "12.2084"; // Заглушка, если дата не указана в файле
            passportExpDateText.text = exp;
        }
        
        // В новом стиле посетитель не выезжает слева. Он просто стоит на месте за закрытой шторкой.
        if (visitorImageDisplay != null)
        {
            visitorImageDisplay.rectTransform.anchoredPosition = originalVisitorPos;
        }
        
        UpdateUI();
        
        if (isBloodNext)
        {
            isBloodNext = false;
            isBloodOnGlass = true;
            cleaningProgress = 0f;
            if (bloodOverlay != null)
            {
                bloodOverlay.gameObject.SetActive(true);
                bloodOverlay.alpha = 1f;
            }
            lastMousePos = Input.mousePosition;
        }
        
        // Запускаем анимацию входа
        StartCoroutine(AnimateVisitorWalkIn());
    }

    void Update()
    {
        // Синхронизируем громкость эффектов в реальном времени
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        }

        VisitorData currentVisitor = (isShiftActive && currentShift != null && currentVisitorIndex < currentShift.shiftVisitors.Length) 
            ? currentShift.shiftVisitors[currentVisitorIndex] : null;

        // Анимация дыхания (когда персонаж стоит на месте)
        if (visitorImageDisplay != null && isShiftActive && !isAnimating && currentVisitor != null)
        {
            float breathSpeed = currentVisitor.isMimic ? 8f : 2f;
            float breathAmp = currentVisitor.isMimic ? 0.05f : 0.015f;
            float breathScale = 1f + Mathf.Sin(Time.time * breathSpeed) * breathAmp;
            visitorImageDisplay.transform.localScale = new Vector3(1f, breathScale, 1f);
        }
        else if (visitorImageDisplay != null)
        {
            visitorImageDisplay.transform.localScale = Vector3.one;
        }

        if (isVisitorWaiting && !isAnimating && !isPaused && isShiftActive && currentVisitor != null)
        {
            visitorTimer += Time.deltaTime;
            
            if (currentVisitor.isImpatient)
            {
                if (visitorTimer > 10f && knockStage == 0)
                {
                    knockStage = 1;
                    StartCoroutine(KnockRoutine(1));
                }
                else if (visitorTimer > 18f && knockStage == 1)
                {
                    knockStage = 2;
                    StartCoroutine(KnockRoutine(2));
                }
                else if (visitorTimer > 25f && knockStage == 2)
                {
                    knockStage = 3;
                    StartCoroutine(KnockRoutine(3));
                    if (glassCracksOverlay != null) glassCracksOverlay.SetActive(true); // Появляются трещины
                }
                else if (visitorTimer > 30f && knockStage == 3)
                {
                    knockStage = 4;
                    isVisitorWaiting = false;
                    // Автоматическая смерть! Монстр пробивает стекло из-за долгого ожидания
                    StartCoroutine(MonsterAttackRoutine()); 
                }
            }
        }

        // Логика протирания стекла от крови
        if (isBloodOnGlass)
        {
            if (Input.GetMouseButton(0))
            {
                if (ragCursor != null)
                {
                    ragCursor.gameObject.SetActive(true);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(ragCursor.parent as RectTransform, Input.mousePosition, null, out Vector2 localPoint);
                    ragCursor.localPosition = localPoint;
                }

                float delta = Vector2.Distance(Input.mousePosition, lastMousePos);
                cleaningProgress += delta * 0.0003f; // Скорость стирания
                
                if (bloodOverlay != null)
                {
                    bloodOverlay.alpha = 1f - cleaningProgress;
                }

                if (cleaningProgress >= 1f)
                {
                    isBloodOnGlass = false;
                    if (bloodOverlay != null) bloodOverlay.gameObject.SetActive(false);
                    if (ragCursor != null) ragCursor.gameObject.SetActive(false);
                }
            }
            else
            {
                // Возвращаем тряпку на стол, если отпустили кнопку
                if (ragCursor != null) 
                {
                    ragCursor.gameObject.SetActive(true);
                    ragCursor.anchoredPosition = new Vector2(250, 100); // Позиция на столе
                }
            }
            lastMousePos = Input.mousePosition;
        }
    }

    private IEnumerator KnockRoutine(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (sfxAudioSource != null && glassKnockSound != null) sfxAudioSource.PlayOneShot(glassKnockSound);
            
            if (visitorImageDisplay != null)
            {
                Vector3 origPos = originalVisitorPos;
                visitorImageDisplay.rectTransform.anchoredPosition = origPos + new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), 0);
                yield return new WaitForSeconds(0.1f);
                visitorImageDisplay.rectTransform.anchoredPosition = origPos;
            }
            yield return new WaitForSeconds(0.4f);
        }
    }

    void GameOver(string reason)
    {
        isShiftActive = false;
        Debug.LogError("ИГРА ОКОНЧЕНА! Причина: " + reason);
        
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            if (gameOverReasonText != null) gameOverReasonText.text = reason;
        }
    }

    void EndShift(string message)
    {
        isShiftActive = false;
        Debug.Log("ПОБЕДА! " + message + " Ошибок: " + strikes);

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (victoryStatsText != null) 
            {
                if (currentShiftIndex >= shiftsDatabase.Length)
                {
                    victoryStatsText.text = "ВЫ ПРОШЛИ ИГРУ!\n\nВсе смены завершены.\nМонстры не прошли.";
                    
                    // Прячем кнопку "Продолжить смену"
                    Button[] btns = victoryPanel.GetComponentsInChildren<Button>(true);
                    foreach(var b in btns) {
                        if(b.name == "NextShiftBtn") b.gameObject.SetActive(false);
                    }
                }
                else
                {
                    victoryStatsText.text = "СМЕНА ОКОНЧЕНА\n\nОшибок: " + strikes + "/3\nЖильцы дома в безопасности.";
                }
            }
        }
    }

    public void OnApproveClicked()
    {
        if (!isShiftActive || isAnimating || currentShift == null) return; 

        VisitorData currentVisitor = currentShift.shiftVisitors[currentVisitorIndex];
        
        if (currentVisitor.isMonster == true)
        {
            // Очень редкий шанс (20%), что монстр решит напасть на стекло
            if (Random.value < 0.20f)
            {
                // Нападение! (Нужен шокер)
                StartCoroutine(StampRoutine(true, false, "", true));
            }
            else
            {
                // В 80% случаев монстр просто проходит внутрь. Мы мгновенно проигрываем!
                StartCoroutine(StampRoutine(true, true, "Вы впустили монстра в здание. Жильцы мертвы. ИГРА ОКОНЧЕНА.", false));
            }
            return;
        }

        StartCoroutine(StampRoutine(true, false, "", false));
    }

    public void OnRejectClicked()
    {
        if (!isShiftActive || isAnimating || currentShift == null) return; 

        VisitorData currentVisitor = currentShift.shiftVisitors[currentVisitorIndex];
        
        if (currentVisitor.isMonster == false)
        {
            strikes++;
            UpdateUI();
            
            if (strikes >= 3) { 
                StartCoroutine(StampRoutine(false, true, "Слишком много ложных обвинений. Вы уволены.", false));
                return; 
            }
        }
        else
        {
            // Мы успешно изолировали монстра! Готовим кровь на стекло
            isBloodNext = true;
        }

        StartCoroutine(StampRoutine(false, false, "", false));
    }

    private IEnumerator StampRoutine(bool isApprove, bool isGameOver, string gameOverReason, bool isMonsterAttack)
    {
        isAnimating = true;
        isVisitorWaiting = false;

        // Прячем старую экранную печать и пропускаем ее анимацию, так как у нас есть реальные тактильные 2D-штампы
        if (stampObject != null) stampObject.SetActive(false);
        
        // Небольшая пауза, чтобы игрок мог увидеть печать на паспорте перед тем, как тот уедет
        yield return new WaitForSeconds(0.8f);

        if (isMonsterAttack)
        {
            StartCoroutine(MonsterAttackRoutine());
            yield break;
        }

        if (isGameOver)
        {
            GameOver(gameOverReason);
            yield break;
        }

        // После печати запускаем стандартную анимацию ухода
        if (isApprove)
        {
            StartCoroutine(AnimateApproveAndLoadNext());
        }
        else
        {
            StartCoroutine(AnimateRejectAndLoadNext());
        }
    }

    // Флаг, чтобы знать, как вводить следующего посетителя
    private bool lastActionWasApprove = true;

    // Анимация ПРОПУСКА (человек уходит вправо)
    private IEnumerator AnimateApproveAndLoadNext()
    {
        isAnimating = true;
        lastActionWasApprove = true;
        
        float duration = 1.0f; // Увеличил время, чтобы он уходил медленнее
        float elapsed = 0f;
        
        Vector3 trayOpenPos = originalTrayPos;
        Vector3 trayClosedPos = originalTrayPos + new Vector3(0f, -600f, 0f);
        
        Vector3 visitorStart = originalVisitorPos;
        Vector3 visitorTarget = originalVisitorPos + new Vector3(800f, 0f, 0f); // Уходит вправо

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; 
            
            if (visitorImageDisplay != null) visitorImageDisplay.rectTransform.anchoredPosition = Vector3.Lerp(visitorStart, visitorTarget, t);
            if (documentTray != null) documentTray.anchoredPosition = Vector3.Lerp(trayOpenPos, trayClosedPos, t);
            
            yield return null; 
        }

        currentVisitorIndex++;
        ShowVisitor(currentVisitorIndex);
    }

    // Анимация ИЗОЛЯЦИИ (закрывается железная шторка)
    private IEnumerator AnimateRejectAndLoadNext()
    {
        isAnimating = true;
        lastActionWasApprove = false;
        
        float duration = 1.0f; // Шторка падает медленнее, создавая ощущение тяжести
        float elapsed = 0f;
        
        Vector3 shutterOpenPos = originalShutterPos + new Vector3(0f, 800f, 0f);
        Vector3 shutterClosedPos = originalShutterPos; 
        
        Vector3 trayOpenPos = originalTrayPos;
        Vector3 trayClosedPos = originalTrayPos + new Vector3(0f, -600f, 0f);
        
        // 1. Сначала убираем лоток и выводим охранников
        Vector3 guardLeftTarget = guardLeftStartPos + new Vector3(400f, 0f, 0f); // Едет дальше вправо (для широкого окна)
        Vector3 guardRightTarget = guardRightStartPos + new Vector3(-400f, 0f, 0f); // Едет дальше влево
        
        if (guardLeft != null && guardWalkingSprite != null) guardLeft.GetComponent<Image>().sprite = guardWalkingSprite;
        if (guardRight != null && guardWalkingSprite != null) 
        {
            guardRight.GetComponent<Image>().sprite = guardWalkingSprite;
            guardRight.localScale = new Vector3(-Mathf.Abs(originalGuardRightScale.x), originalGuardRightScale.y, originalGuardRightScale.z); // Поворачиваем налево
        }

        float guardDuration = 0.5f;
        while (elapsed < guardDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / guardDuration; 
            
            if (documentTray != null) documentTray.anchoredPosition = Vector3.Lerp(trayOpenPos, trayClosedPos, t);
            if (guardLeft != null) guardLeft.anchoredPosition = Vector3.Lerp(guardLeftStartPos, guardLeftTarget, t);
            if (guardRight != null) guardRight.anchoredPosition = Vector3.Lerp(guardRightStartPos, guardRightTarget, t);
            
            yield return null; 
        }

        // Схватили монстра! Меняем на фото "Держит"
        if (guardLeft != null && guardHoldingSprite != null) guardLeft.GetComponent<Image>().sprite = guardHoldingSprite;
        if (guardRight != null && guardHoldingSprite != null) guardRight.GetComponent<Image>().sprite = guardHoldingSprite;

        // 2. Затем падает шторка
        elapsed = 0f;
        
        // Воспроизводим звук падения железной двери!
        if (sfxAudioSource != null && shutterCloseSound != null)
        {
            sfxAudioSource.PlayOneShot(shutterCloseSound);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration; 
            
            if (windowShutter != null) windowShutter.anchoredPosition = Vector3.Lerp(shutterOpenPos, shutterClosedPos, t);
            
            yield return null; 
        }

        // Шторка упала. Делаем паузу, чтобы создать напряжение! (Монстра забирают)
        yield return new WaitForSeconds(1.5f);
        
        // Возвращаем охранников на места за кадром (их не видно, т.к. шторка закрыта)
        if (guardLeft != null) 
        {
            guardLeft.anchoredPosition = guardLeftStartPos;
            if (defaultGuardSprite != null) guardLeft.GetComponent<Image>().sprite = defaultGuardSprite;
        }
        if (guardRight != null) 
        {
            guardRight.anchoredPosition = guardRightStartPos;
            guardRight.localScale = originalGuardRightScale; // Возвращаем оригинальный масштаб
            if (defaultGuardSprite != null) guardRight.GetComponent<Image>().sprite = defaultGuardSprite;
        }

        currentVisitorIndex++;
        ShowVisitor(currentVisitorIndex);
    }

    // Анимация ВХОДА (Зависит от прошлого действия)
    private IEnumerator AnimateVisitorWalkIn()
    {
        isAnimating = true;
        float duration = 1.0f; // Плавное открытие и приход
        float elapsed = 0f;
        
        Vector3 shutterOpenPos = originalShutterPos + new Vector3(0f, 800f, 0f);
        Vector3 shutterClosedPos = originalShutterPos; 
        
        Vector3 trayOpenPos = originalTrayPos;
        Vector3 trayClosedPos = originalTrayPos + new Vector3(0f, -600f, 0f);

        Vector3 visitorStart = originalVisitorPos + new Vector3(-800f, 0f, 0f); // Восстанавливаем переменную!
        Vector3 visitorTarget = originalVisitorPos;

        // Прячем печать от предыдущего посетителя!
        if (stampObject != null) stampObject.SetActive(false);

        // Уничтожаем все динамические отпечатки штампов на паспорте и въездном талоне (DocumentTray)
        if (documentTray != null)
        {
            foreach (Transform child in documentTray.GetComponentsInChildren<Transform>(true))
            {
                if (child != null && child.name == "DynamicStampMark")
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }

        // Автоматически закрываем выдвижной ящик со штампами при смене посетителя
        StampDrawerController drawer = Object.FindAnyObjectByType<StampDrawerController>();
        if (drawer != null) drawer.ForceClose();

        if (lastActionWasApprove)
        {
            // Если прошлого мы пропустили (шторка уже открыта) -> новый заходит слева
            if (windowShutter != null) windowShutter.anchoredPosition = shutterOpenPos; 
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration; 
                float easeT = 1f - Mathf.Pow(1f - t, 3f); 
                
                if (visitorImageDisplay != null) visitorImageDisplay.rectTransform.anchoredPosition = Vector3.Lerp(visitorStart, visitorTarget, easeT);
                if (documentTray != null) documentTray.anchoredPosition = Vector3.Lerp(trayClosedPos, trayOpenPos, easeT);
                
                yield return null; 
            }
        }
        else
        {
            // 1. Сначала поднимается шторка (окно пустое).
            if (visitorImageDisplay != null) visitorImageDisplay.rectTransform.anchoredPosition = visitorStart;
            
            // Воспроизводим звук открытия железной двери!
            if (sfxAudioSource != null && shutterOpenSound != null)
            {
                sfxAudioSource.PlayOneShot(shutterOpenSound);
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration; 
                float easeT = 1f - Mathf.Pow(1f - t, 3f); 
                if (windowShutter != null) windowShutter.anchoredPosition = Vector3.Lerp(shutterClosedPos, shutterOpenPos, easeT);
                yield return null; 
            }
            if (windowShutter != null) windowShutter.anchoredPosition = shutterOpenPos;

            // 2. Охранник (левый) выходит в центр окна
            if (guardLeft != null)
            {
                guardLeft.anchoredPosition = guardLeftStartPos;
                if (guardWalkingSprite != null) guardLeft.GetComponent<Image>().sprite = guardWalkingSprite;

                elapsed = 0f;
                float walkDuration = 0.5f;
                while (elapsed < walkDuration)
                {
                    elapsed += Time.deltaTime;
                    guardLeft.anchoredPosition = Vector3.Lerp(guardLeftStartPos, originalVisitorPos, elapsed / walkDuration);
                    yield return null;
                }
                guardLeft.anchoredPosition = originalVisitorPos;
            }

            // 3. Охранник говорит текст (меняем картинку на "стоящего/говорящего")
            if (guardLeft != null && guardStandingSprite != null)
            {
                guardLeft.GetComponent<Image>().sprite = guardStandingSprite;
            }
            else if (guardLeft != null && defaultGuardSprite != null)
            {
                guardLeft.GetComponent<Image>().sprite = defaultGuardSprite;
            }

            StartGuardDialogue("УГРОЗА ИЗОЛИРОВАНА. ПРОДОЛЖАЙ В ТОМ ЖЕ ДУХЕ.");
            while (dialoguePanel != null && dialoguePanel.activeSelf)
            {
                yield return null;
            }

            // 4. Охранник уходит вправо
            if (guardLeft != null)
            {
                if (guardWalkingSprite != null) guardLeft.GetComponent<Image>().sprite = guardWalkingSprite;

                Vector3 guardExitPos = originalVisitorPos + new Vector3(800f, 0f, 0f);
                elapsed = 0f;
                float walkDuration = 0.5f;
                while (elapsed < walkDuration)
                {
                    elapsed += Time.deltaTime;
                    guardLeft.anchoredPosition = Vector3.Lerp(originalVisitorPos, guardExitPos, elapsed / walkDuration);
                    yield return null;
                }
                guardLeft.anchoredPosition = guardLeftStartPos; // возвращаем его на базу слева
                if (defaultGuardSprite != null) guardLeft.GetComponent<Image>().sprite = defaultGuardSprite; // возвращаем в обычное состояние
            }

            // 5. Теперь новый посетитель заходит слева, а лоток выезжает на стол
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration; 
                float easeT = 1f - Mathf.Pow(1f - t, 3f); 
                
                if (visitorImageDisplay != null) visitorImageDisplay.rectTransform.anchoredPosition = Vector3.Lerp(visitorStart, visitorTarget, easeT);
                if (documentTray != null) documentTray.anchoredPosition = Vector3.Lerp(trayClosedPos, trayOpenPos, easeT);
                
                yield return null; 
            }
        }

        if (windowShutter != null) windowShutter.anchoredPosition = shutterOpenPos;
        if (documentTray != null) documentTray.anchoredPosition = trayOpenPos;
        if (visitorImageDisplay != null) visitorImageDisplay.rectTransform.anchoredPosition = originalVisitorPos;
        
        // --- ДЕЗИНФЕКЦИЯ (Пшик газом с ДВУХ СТОРОН) ---
        if (deconGasOverlay == null)
        {
            GameObject gasObj = GameObject.Find("DecontaminationGas");
            if (gasObj != null) deconGasOverlay = gasObj.GetComponent<CanvasGroup>();
        }

        if (deconGasOverlay != null) 
        {
            // ФОРСИРУЕМ ДИНАМИЧЕСКИ: Находим шторку и ставим газ ровно перед ней в Иерархии!
            if (windowShutter != null)
            {
                deconGasOverlay.transform.SetParent(windowShutter.parent, false);
                deconGasOverlay.transform.SetSiblingIndex(windowShutter.GetSiblingIndex() + 1);
            }

            deconGasOverlay.gameObject.SetActive(true);
            deconGasOverlay.alpha = 1f;
            if (sfxAudioSource != null && deconGasSound != null) sfxAudioSource.PlayOneShot(deconGasSound);
            
            if (deconGasOverlay.transform.childCount >= 2)
            {
                RectTransform leftSpray = deconGasOverlay.transform.GetChild(0).GetComponent<RectTransform>();
                RectTransform rightSpray = deconGasOverlay.transform.GetChild(1).GetComponent<RectTransform>();
                
                Vector2 leftStart = new Vector2(-300f, 0f);
                Vector2 leftEnd = new Vector2(0f, 0f);
                Vector2 rightStart = new Vector2(300f, 0f);
                Vector2 rightEnd = new Vector2(0f, 0f);
                
                // 1. Резкий впрыск к центру (0.3 сек)
                float gasElapsed = 0f;
                while(gasElapsed < 0.3f)
                {
                    gasElapsed += Time.deltaTime;
                    float t = gasElapsed / 0.3f;
                    float easeT = 1f - Mathf.Pow(1f - t, 3f);
                    if (leftSpray != null) leftSpray.anchoredPosition = Vector2.Lerp(leftStart, leftEnd, easeT);
                    if (rightSpray != null) rightSpray.anchoredPosition = Vector2.Lerp(rightStart, rightEnd, easeT);
                    yield return null;
                }
                
                // 2. Медленное растворение (1.2 сек)
                gasElapsed = 0f;
                while(gasElapsed < 1.2f) 
                {
                    gasElapsed += Time.deltaTime;
                    deconGasOverlay.alpha = 1f - (gasElapsed / 1.2f);
                    // Немного продолжают ползти вперед
                    if (leftSpray != null) leftSpray.anchoredPosition = Vector2.Lerp(leftEnd, new Vector2(100f, 0f), gasElapsed / 1.2f);
                    if (rightSpray != null) rightSpray.anchoredPosition = Vector2.Lerp(rightEnd, new Vector2(-100f, 0f), gasElapsed / 1.2f);
                    yield return null;
                }
            }
            else
            {
                // Запасной старый вариант (если нет 2 детей)
                float gasElapsed = 0f;
                while(gasElapsed < 1.0f)
                {
                    gasElapsed += Time.deltaTime;
                    deconGasOverlay.alpha = 1f - (gasElapsed / 1.0f);
                    yield return null;
                }
            }
            
            deconGasOverlay.alpha = 0f;
            deconGasOverlay.gameObject.SetActive(false);
        }
        
        // ЗАПУСКАЕМ ДИАЛОГ после входа и дезинфекции
        StartDialogue(currentShift.shiftVisitors[currentVisitorIndex]);

        isVisitorWaiting = true;
        visitorTimer = 0f;
        knockStage = 0;

        isAnimating = false; 
    }

    void Awake()
    {
        // 0. Жестко фиксируем слои интерфейса на старте игры (НАХОДИМ ДАЖЕ ВЫКЛЮЧЕННЫЕ ПАНЕЛИ)
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            if (screenFlashOverlay != null) { screenFlashOverlay.transform.SetParent(canvas.transform, false); screenFlashOverlay.transform.SetAsLastSibling(); }
            if (dialoguePanel != null) { dialoguePanel.transform.SetParent(canvas.transform, false); dialoguePanel.transform.SetAsLastSibling(); }
            if (victoryPanel != null) { victoryPanel.transform.SetParent(canvas.transform, false); victoryPanel.transform.SetAsLastSibling(); }
            if (gameOverPanel != null) { gameOverPanel.transform.SetParent(canvas.transform, false); gameOverPanel.transform.SetAsLastSibling(); }
            if (pausePanel != null) { pausePanel.transform.SetParent(canvas.transform, false); pausePanel.transform.SetAsLastSibling(); }
            
            Transform pBtn = canvas.transform.Find("WindowFrame/PauseBtn");
            if (pBtn == null) pBtn = canvas.transform.Find("PauseBtn");
            if (pBtn != null) { pBtn.SetParent(canvas.transform, false); pBtn.SetAsLastSibling(); }
            
            Transform crt = canvas.transform.Find("WindowFrame/CRT_Overlay_Safe");
            if (crt == null) crt = canvas.transform.Find("CRT_Overlay_Safe");
            if (crt != null) { crt.SetParent(canvas.transform, false); crt.SetAsLastSibling(); }
            
            // Если фото досье не привязано в инспекторе, ищем его
            if (dossierPhotoDisplay == null)
            {
                Transform photoT = canvas.transform.Find("WindowFrame/Desk/DocumentTray/DossierPhoto");
                if (photoT != null) dossierPhotoDisplay = photoT.GetComponent<Image>();
            }
        }

        // 1. Автоматически ищем и привязываем основные кнопки геймплея
        // Ищем на всем Canvas кнопки по именам, которые дает наш GameSceneBuilder
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (var btn in allButtons)
        {
            if (btn.name == "ApproveBtn") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnApproveClicked); }
            if (btn.name == "RejectBtn") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnRejectClicked); }
            if (btn.name == "NextShiftBtn") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(LoadNextShift); }
            if (btn.name == "InterrogateBtn") { btn.onClick.RemoveAllListeners(); btn.onClick.AddListener(OnInterrogateClicked); }
        }

        if (interrogateBtn != null) 
        {
            interrogateBtn.onClick.RemoveAllListeners();
            interrogateBtn.onClick.AddListener(OnInterrogateClicked);
        }

        if (questionsPanel != null)
        {
            Button[] qBtns = questionsPanel.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < qBtns.Length; i++)
            {
                int index = i; // capture index
                qBtns[i].onClick.RemoveAllListeners();
                qBtns[i].onClick.AddListener(() => AskQuestion(index));
            }
        }

        if (phoneButton != null)
        {
            phoneButton.onClick.RemoveAllListeners();
            phoneButton.onClick.AddListener(OnPhoneClicked);
        }

        // Если авто-поиск не сработал из-за несовпадения имен, используем прямые ссылки из инспектора
        // (Для паузы ссылки теперь только в Inspector, поэтому код закомментирован, чтобы избежать двойного клика)
        // if (pauseButton != null) { pauseButton.onClick.RemoveAllListeners(); pauseButton.onClick.AddListener(TogglePause); }
        // if (resumeButton != null) { resumeButton.onClick.RemoveAllListeners(); resumeButton.onClick.AddListener(TogglePause); }
        // if (exitButton != null) { exitButton.onClick.RemoveAllListeners(); exitButton.onClick.AddListener(ReturnToMainMenu); }

        if (dialoguePanel != null)
        {
            Image panelImage = dialoguePanel.GetComponent<Image>();
            if (panelImage == null) 
            {
                panelImage = dialoguePanel.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0); // Прозрачный фон, если его нет
            }
            Button dialogueBtn = dialoguePanel.GetComponent<Button>();
            if (dialogueBtn == null) dialogueBtn = dialoguePanel.AddComponent<Button>();
            dialogueBtn.onClick.RemoveAllListeners();
            dialogueBtn.onClick.AddListener(OnDialogueClicked);
        }

        Debug.Log("[GameManager] Все кнопки найдены и подключены автоматически.");
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("Переход в Главное Меню...");
        Time.timeScale = 1f;
        
        // Проверяем, есть ли сцена в билде
        if (Application.CanStreamedLevelBeLoaded("MainMenu"))
        {
            SceneManager.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("ОШИБКА: Сцена 'MainMenu' не добавлена в Build Settings! Нажми File -> Build Settings и перетащи туда сцену MainMenu.");
        }
    }

    // Алиас для обратной совместимости со старыми скриптами генерации (EndScreenBuilder и т.д.)
    public void ReturnToMenu()
    {
        ReturnToMainMenu();
    }

    public void LoadNextShift()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); // Перезагружаем текущую сцену для новой смены
    }

    public void TogglePause()
    {
        if (!isShiftActive && !isPaused) return; 

        isPaused = !isPaused;
        if (pausePanel != null) 
        {
            if (isPaused) pausePanel.transform.SetAsLastSibling(); // Гарантирует, что панель будет ПОВЕРХ монитора и всего остального!
            pausePanel.SetActive(isPaused);
        }

        // При закрытии паузы закрываем и настройки
        if (!isPaused && settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            PlayerPrefs.Save();
        }

        Time.timeScale = isPaused ? 0f : 1f;
        Debug.Log(isPaused ? "Пауза ВКЛ" : "Пауза ВЫКЛ");
    }

    // ==========================================
    // ЛОГИКА НАСТРОЕК ЗВУКА
    // ==========================================
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            settingsPanel.transform.SetAsLastSibling(); // Поверх всего!
            
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

            if (musicSlider != null) musicSlider.value = musicVol;
            if (sfxSlider != null) sfxSlider.value = sfxVol;
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
        PlayerPrefs.Save();
    }

    public void OnMusicVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
    }

    public void OnSFXVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        if (sfxAudioSource != null)
        {
            sfxAudioSource.volume = value;
        }
    }
    private bool isTypingDialogue = false;
    private string fullDialogueText = "";
    private Coroutine typewriterCoroutine;

    private IEnumerator TypewriterRoutine(string text)
    {
        isTypingDialogue = true;
        dialogueContentText.text = "";
        foreach (char c in text.ToCharArray())
        {
            dialogueContentText.text += c;
            yield return new WaitForSeconds(0.03f); // Скорость печати
        }
        isTypingDialogue = false;
    }

    private void StartGuardDialogue(string text)
    {
        if (dialoguePanel == null) return;
        
        dialoguePanel.SetActive(true);
        if (dialoguePortrait != null)
        {
            dialoguePortrait.sprite = guardStandingSprite != null ? guardStandingSprite : defaultGuardSprite;
            dialoguePortrait.gameObject.SetActive(dialoguePortrait.sprite != null);
        }
        if (dialogueNameText != null) dialogueNameText.text = "Служба зачистки";
        
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        
        fullDialogueText = text;
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(fullDialogueText));
    }

    private void StartDialogue(VisitorData visitor)
    {
        if (dialoguePanel == null) return;
        
        dialoguePanel.SetActive(true);
        if (dialoguePortrait != null)
        {
            dialoguePortrait.sprite = visitor.visitorSprite;
            dialoguePortrait.gameObject.SetActive(dialoguePortrait.sprite != null);
        }
        if (dialogueNameText != null) dialogueNameText.text = visitor.passportName.Split('\n')[0]; // Берем только имя
        
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        
        fullDialogueText = string.IsNullOrEmpty(visitor.welcomeSpeech) ? "..." : visitor.welcomeSpeech;
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(fullDialogueText));
    }

    public void OnDialogueClicked()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf) return;

        if (isTypingDialogue)
        {
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            dialogueContentText.text = fullDialogueText;
            isTypingDialogue = false;
        }
        else
        {
            dialoguePanel.SetActive(false);

            if (phoneAnswered)
            {
                phoneAnswered = false;
                ShowVisitor(currentVisitorIndex); // Исправил ошибку
            }
        }
    }

    private IEnumerator PhoneRingRoutine()
    {
        isPhoneRinging = true;
        phoneAnswered = false;
        
        if (phoneButton != null)
        {
            AudioSource phoneAudio = phoneButton.GetComponent<AudioSource>();
            if (phoneAudio == null) phoneAudio = phoneButton.gameObject.AddComponent<AudioSource>();
            
            float ringTimer = 0f;
            Vector3 origPos = phoneButton.transform.localPosition;
            
            while(isPhoneRinging)
            {
                // Проигрываем звук звонка и устанавливаем таймер (длина звука + 1.5 сек тишины)
                if (ringTimer <= 0f)
                {
                    if (phoneAudio != null && phoneRingSound != null) 
                    {
                        phoneAudio.PlayOneShot(phoneRingSound);
                        ringTimer = phoneRingSound.length + 1.5f; 
                    }
                    else
                    {
                        ringTimer = 2.0f; // Запасной таймер
                    }
                }
                
                ringTimer -= Time.deltaTime;

                // Трясем телефон только во время звучания самого звонка
                if (phoneRingSound != null && ringTimer > 1.5f)
                {
                    phoneButton.transform.localPosition = origPos + new Vector3(Mathf.Sin(Time.time * 30f) * 10f, 0, 0);
                }
                else if (phoneRingSound == null)
                {
                    phoneButton.transform.localPosition = origPos + new Vector3(Mathf.Sin(Time.time * 30f) * 10f, 0, 0);
                }
                else
                {
                    phoneButton.transform.localPosition = origPos; // Тишина - не трясется
                }

                yield return null;
            }
            
            phoneButton.transform.localPosition = origPos;
        }
    }

    public void OnPhoneClicked()
    {
        if (!isPhoneRinging) return;
        
        isPhoneRinging = false;
        phoneAnswered = true;
        
        if (phoneButton != null)
        {
            AudioSource phoneAudio = phoneButton.GetComponent<AudioSource>();
            if (phoneAudio != null)
            {
                phoneAudio.Stop(); // Мгновенно останавливаем звонок
                
                if (phonePickupSound != null)
                {
                    phoneAudio.PlayOneShot(phonePickupSound); // Проигрываем звук снятия трубки
                }
            }
        }
        
        if (dialoguePanel == null) return;
        
        dialoguePanel.SetActive(true);
        if (dialoguePortrait != null)
        {
            if (dispatcherSprite != null)
            {
                dialoguePortrait.sprite = dispatcherSprite;
                dialoguePortrait.gameObject.SetActive(true);
            }
            else
            {
                dialoguePortrait.sprite = null;
                dialoguePortrait.gameObject.SetActive(false);
            }
        }
        if (dialogueNameText != null) dialogueNameText.text = "СЛУЖБА БЕЗОПАСНОСТИ"; 
        
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        
        fullDialogueText = shiftPhoneMessages[currentShiftIndex];
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(fullDialogueText));
    }

    public void OnInterrogateClicked()
    {
        if (!isShiftActive || isAnimating || currentShift == null) return;
        
        if (questionsPanel != null)
        {
            questionsPanel.SetActive(!questionsPanel.activeSelf);
        }
    }

    public void AskQuestion(int questionType)
    {
        if (!isShiftActive || isAnimating || currentShift == null) return;
        if (questionsPanel != null) questionsPanel.SetActive(false); // Прячем кнопки вопросов
        
        VisitorData currentVisitor = currentShift.shiftVisitors[currentVisitorIndex];
        string response = "...";
        
        if (currentVisitor.isMimic)
        {
            // Мимик повторяет вопросы искаженным образом
            if (questionType == 0) response = "П-почему... не с-совпадает ИМЯ?";
            else if (questionType == 1) response = "Ч-что с... м-моими ГЛАЗАМИ?";
            else if (questionType == 2) response = "М-мой ПАСПОРТ... просрочен?";
        }
        else
        {
            // Выбираем случайный индекс для разнообразия ответов
            int randIndex = Random.Range(0, 3);
            
            if (questionType == 0) // Имя или ID
            {
                if (!string.IsNullOrEmpty(currentVisitor.responseName) && 
                    currentVisitor.responseName != "С моим именем всё нормально." && 
                    currentVisitor.responseName != "С моими данными всё в порядке.")
                {
                    response = currentVisitor.responseName;
                }
                else
                {
                    if (currentVisitor.isMonster)
                    {
                        string[] monsterReplies = new string[] {
                            "Имя... моё имя... это просто имя. Оно правильное. Пропустите меня.",
                            "В базе данных старая версия меня... Я изменился... Пропустите.",
                            "Я спешу. Не задавайте глупых вопросов. Я живу здесь."
                        };
                        response = monsterReplies[randIndex];
                    }
                    else
                    {
                        string[] humanReplies = new string[] {
                            "Ой, в паспортном столе опечатались... Мне обещали исправить на следующей неделе.",
                            "Это моя девичья фамилия, я совсем недавно вышла замуж!",
                            "О господи, опять принтер смазал буквы? Это точно цифра 8, а не 3."
                        };
                        response = humanReplies[randIndex];
                    }
                }
            }
            else if (questionType == 1) // Глаза
            {
                if (!string.IsNullOrEmpty(currentVisitor.responseEyes) && 
                    currentVisitor.responseEyes != "Обычные глаза." && 
                    currentVisitor.responseEyes != "Это контактные линзы.")
                {
                    response = currentVisitor.responseEyes;
                }
                else
                {
                    if (currentVisitor.isMonster)
                    {
                        string[] monsterReplies = new string[] {
                            "Мои глаза... видят вас. Они нормальные. Смотрите на них.",
                            "Свет... здесь слишком яркий свет. Мои зрачки в порядке.",
                            "Это просто контактные линзы... человеческие линзы... да..."
                        };
                        response = monsterReplies[randIndex];
                    }
                    else
                    {
                        string[] humanReplies = new string[] {
                            "Ой, я сегодня забыл надеть линзы... или наоборот, надел цветные!",
                            "У меня сильная аллергия на тополиный пух, глаза ужасно опухли.",
                            "Я просто очень сильно не выспался... работаю на трех работах."
                        };
                        response = humanReplies[randIndex];
                    }
                }
            }
            else if (questionType == 2) // Срок действия паспорта
            {
                if (!string.IsNullOrEmpty(currentVisitor.responseDate) && 
                    currentVisitor.responseDate != "С датами всё верно." && 
                    currentVisitor.responseDate != "Я просто забыл его поменять.")
                {
                    response = currentVisitor.responseDate;
                }
                else
                {
                    if (currentVisitor.isMonster)
                    {
                        string[] monsterReplies = new string[] {
                            "Дата... это просто цифры на бумаге. Время не имеет значения.",
                            "Паспорт свежий... он пахнет человеком... он не просрочен.",
                            "Я должен войти. Моя семья ждет меня внутри. Дата верна."
                        };
                        response = monsterReplies[randIndex];
                    }
                    else
                    {
                        string[] humanReplies = new string[] {
                            "О нет! Я совсем закрутился с работой и забыл продлить... Пожалуйста, пропустите!",
                            "Я уже подал документы на замену, вот справка... Ой, я забыл её дома.",
                            "Черт, неужели уже 2084 год? Как быстро летит время..."
                        };
                        response = humanReplies[randIndex];
                    }
                }
            }
        }
        
        if (dialoguePanel == null) return;
        
        dialoguePanel.SetActive(true);
        if (dialoguePortrait != null)
        {
            dialoguePortrait.sprite = currentVisitor.visitorSprite;
            dialoguePortrait.gameObject.SetActive(dialoguePortrait.sprite != null);
        }
        if (dialogueNameText != null) dialogueNameText.text = currentVisitor.passportName.Split('\n')[0]; 
        
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        
        fullDialogueText = response;
        typewriterCoroutine = StartCoroutine(TypewriterRoutine(fullDialogueText));
    }

    private IEnumerator MonsterAttackRoutine()
    {
        isAnimating = true;
        if (stampObject != null) stampObject.SetActive(false);
        
        // Резко увеличиваем монстра и красим в красный
        Vector3 origScale = Vector3.one;
        if (visitorImageDisplay != null) 
        {
            origScale = visitorImageDisplay.transform.localScale;
            visitorImageDisplay.transform.localScale = new Vector3(1.4f, 1.4f, 1.4f);
            visitorImageDisplay.color = new Color(1f, 0.5f, 0.5f, 1f);
        }
        
        // Показываем трещины на стекле и отключаем на них блокировку мыши
        if (glassCracksOverlay != null)
        {
            glassCracksOverlay.SetActive(true);
            Image img = glassCracksOverlay.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;
            foreach (var child in glassCracksOverlay.GetComponentsInChildren<Image>(true))
            {
                child.raycastTarget = false;
            }
        }

        // Отключаем блокировку кликов мыши на оверлее крови
        if (bloodOverlay != null)
        {
            bloodOverlay.blocksRaycasts = false;
            Image img = bloodOverlay.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;
            foreach (var child in bloodOverlay.GetComponentsInChildren<Image>(true))
            {
                child.raycastTarget = false;
            }
        }
        
        // Выдвигаем ящик с шокером и форсируем его поверх трещин/крови в иерархии
        if (stunGunDrawer != null)
        {
            stunGunDrawer.SetActive(true);
            stunGunDrawer.transform.SetAsLastSibling();
            CanvasGroup cg = stunGunDrawer.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = true;
        }
        
        // Автоматически находим кнопку шокера, если она не привязана в инспекторе
        if (stunGunButton == null && stunGunDrawer != null)
        {
            stunGunButton = stunGunDrawer.GetComponentInChildren<Button>(true);
        }

        float timeLeft = 2.0f;
        bool shocked = false;
        
        if (stunGunButton != null)
        {
            TextMeshProUGUI bTxt = stunGunButton.GetComponentInChildren<TextMeshProUGUI>(true);
            
            if (currentStunCharges > 0)
            {
                if (bTxt != null) bTxt.text = "УДАРИТЬ ШОКЕРОМ!";
                stunGunButton.onClick.RemoveAllListeners();
                stunGunButton.onClick.AddListener(() => { 
                    shocked = true; 
                    currentStunCharges--; // Тратим заряд
                });
            }
            else
            {
                if (bTxt != null) bTxt.text = "ШОКЕР РАЗРЯЖЕН!";
                stunGunButton.onClick.RemoveAllListeners(); // Ничего не делает, игрок обречен
            }
        }

        Vector3 origVisitorPos = visitorImageDisplay != null ? visitorImageDisplay.rectTransform.anchoredPosition : Vector3.zero;

        // Ждем 2 секунды, трясем монстра
        while(timeLeft > 0)
        {
            if (shocked) break;
            
            timeLeft -= Time.deltaTime;
            
            if (visitorImageDisplay != null)
            {
                visitorImageDisplay.rectTransform.anchoredPosition = origVisitorPos + new Vector3(Random.Range(-20f, 20f), Random.Range(-20f, 20f), 0);
            }
            
            yield return null;
        }
        
        if (visitorImageDisplay != null) visitorImageDisplay.rectTransform.anchoredPosition = origVisitorPos;
        
        if (shocked)
        {
            // Игрок успел ударить шокером!
            if (screenFlashOverlay != null)
            {
                screenFlashOverlay.gameObject.SetActive(true);
                screenFlashOverlay.color = new Color(0.5f, 0.8f, 1f, 1f); // Синяя вспышка тока
            }
            
            if (glassCracksOverlay != null) glassCracksOverlay.SetActive(false);
            if (stunGunDrawer != null) stunGunDrawer.SetActive(false);
            
            // Монстр оглушен и обуглен! Оставляем его видимым, чтобы охранники могли его утащить.
            if (visitorImageDisplay != null) 
            {
                visitorImageDisplay.transform.localScale = origScale;
                visitorImageDisplay.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Темный (обгоревший)
            }
            
            // Плавное затухание вспышки
            float t = 1f;
            while(t > 0)
            {
                t -= Time.deltaTime * 2f;
                if (screenFlashOverlay != null) screenFlashOverlay.color = new Color(0.5f, 0.8f, 1f, t);
                yield return null;
            }
            if (screenFlashOverlay != null) screenFlashOverlay.gameObject.SetActive(false);
            
            // Штраф за ошибку (игрок ведь изначально нажал Одобрить)
            strikes++;
            UpdateUI();
            
            if (strikes >= 3) 
            {
                GameOver("Вы оглушили монстра, но до этого допустили слишком много ошибок. Вы уволены.");
                yield break;
            }
            
            // Даем игроку секунду посмотреть на пустое окно, чтобы насладиться победой
            yield return new WaitForSeconds(1.0f);
            
            isBloodNext = true; // После шокера стекло тоже в крови монстра!
            
            // Переходим к следующему посетителю
            StartCoroutine(AnimateRejectAndLoadNext());
        }
        else
        {
            // Время вышло
            if (glassCracksOverlay != null) glassCracksOverlay.SetActive(false);
            if (stunGunDrawer != null) stunGunDrawer.SetActive(false);
            
            GameOver("ВЫ НЕ УСПЕЛИ ДОСТАТЬ ШОКЕР. МОНСТР ВЫБИЛ СТЕКЛО.");
        }
    }

    private void DisableRaycastOnOverlay(string objName)
    {
        GameObject obj = GameObject.Find(objName);
        if (obj != null)
        {
            Image img = obj.GetComponent<Image>();
            if (img != null) img.raycastTarget = false;
            
            foreach (var child in obj.GetComponentsInChildren<Image>(true))
            {
                child.raycastTarget = false;
            }

            CanvasGroup cg = obj.GetComponent<CanvasGroup>();
            if (cg != null) cg.blocksRaycasts = false;
        }
    }
}
