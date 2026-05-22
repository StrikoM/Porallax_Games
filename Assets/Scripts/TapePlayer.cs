using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(AudioSource))]
public class TapePlayer : MonoBehaviour
{
    [Header("UI Ссылки")]
    public RectTransform spoolLeft;
    public RectTransform spoolRight;
    public Image ledIndicator;
    public TextMeshProUGUI channelTextDisplay;
    
    [Header("Настройки Громкости")]
    [Range(0f, 1f)] public float playerVolume = 0.35f;

    // Внутреннее состояние
    private bool isPlaying = false;
    private int currentChannel = 0;
    private float timeElapsed = 0f;
    private double phase = 0;
    private double sampleRate;
    private System.Random rand = new System.Random();

    // Переменные для синтезатора (Канал 0 - Синт-пад)
    private float[] currentChordFreqs = new float[3];
    private float[] targetChordFreqs = new float[3];
    private float chordTimer = 0f;
    private int chordIndex = 0;

    // Переменные для морзе (Канал 1 - Военный эфир)
    private float morseTimer = 0f;
    private bool morseActive = false;
    private int morseCharIndex = 0;
    private int morseSymbolIndex = 0;
    private float morseSymbolTimer = 0f;
    
    // Алфавит Морзе для сообщения: "S O S A N O M A L Y"
    private string[] morseWords = {
        "...",   // S
        "---",   // O
        "...",   // S
        ".-",    // A
        "-.",    // N
        "---",   // O
        "--",    // M
        ".-",    // A
        ".-..",  // L
        "-.--"   // Y
    };

    // Фильтры для шума
    private float lastNoiseVal = 0f;
    private float tapeCrackleChance = 0.0006f;

    void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;
        AudioSource src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.spatialBlend = 0f; // 2D звук вокруг игрока
        
        // Инициализируем частоты первого аккорда (A minor: A3, C4, E4)
        SetChord(0);
        for(int i=0; i<3; i++) currentChordFreqs[i] = targetChordFreqs[i];
    }

    void Update()
    {
        if (!isPlaying) return;

        timeElapsed += Time.deltaTime;

        // Вращаем катушки магнитофона
        if (spoolLeft != null) spoolLeft.Rotate(0, 0, -120f * Time.deltaTime);
        if (spoolRight != null) spoolRight.Rotate(0, 0, -120f * Time.deltaTime);

        // Индикатор (LED) пульсирует/мигает в такт воспроизведению
        if (ledIndicator != null)
        {
            if (currentChannel == 0) // Спокойный синт-пад
            {
                float ledBlink = 0.6f + Mathf.Sin(timeElapsed * 4f) * 0.4f;
                ledIndicator.color = new Color(0.9f * ledBlink, 0.1f, 0.1f, 1f); // Красный пульсирующий
            }
            else if (currentChannel == 1) // Военная морзянка
            {
                // Мигает синхронно с сигналами морзе
                ledIndicator.color = morseActive ? new Color(0.2f, 0.9f, 0.2f, 1f) : new Color(0.05f, 0.2f, 0.05f, 1f); // Зеленый LED
            }
            else // Мрачный гул
            {
                // Медленно дышит
                float ledBreath = 0.4f + Mathf.Sin(timeElapsed * 1.5f) * 0.3f;
                ledIndicator.color = new Color(0.9f * ledBreath, 0.6f * ledBreath, 0.1f * ledBreath, 1f); // Янтарный LED
            }
        }

        // Обновляем аккорды во времени
        if (currentChannel == 0)
        {
            chordTimer += Time.deltaTime;
            if (chordTimer >= 5.0f) // Меняем аккорд каждые 5 секунд
            {
                chordTimer = 0f;
                chordIndex = (chordIndex + 1) % 4;
                SetChord(chordIndex);
            }
            // Плавное интерполирование частот для glide-эффекта
            for(int i = 0; i < 3; i++)
            {
                currentChordFreqs[i] = Mathf.Lerp(currentChordFreqs[i], targetChordFreqs[i], Time.deltaTime * 1.5f);
            }
        }

        // Логика Морзе во времени
        if (currentChannel == 1)
        {
            morseSymbolTimer += Time.deltaTime;
            string currentWord = morseWords[morseCharIndex];
            
            if (morseSymbolIndex < currentWord.Length)
            {
                char symbol = currentWord[morseSymbolIndex];
                float symbolDuration = (symbol == '-') ? 0.3f : 0.1f;
                
                if (morseSymbolTimer < symbolDuration)
                {
                    morseActive = true;
                }
                else if (morseSymbolTimer < symbolDuration + 0.1f) // Пауза между знаками
                {
                    morseActive = false;
                }
                else
                {
                    morseSymbolTimer = 0f;
                    morseSymbolIndex++;
                }
            }
            else // Пауза между буквами
            {
                morseActive = false;
                if (morseSymbolTimer >= 0.4f)
                {
                    morseSymbolTimer = 0f;
                    morseSymbolIndex = 0;
                    morseCharIndex = (morseCharIndex + 1) % morseWords.Length;
                }
            }
        }
    }

    public void TogglePlay()
    {
        isPlaying = !isPlaying;
        AudioSource src = GetComponent<AudioSource>();

        if (isPlaying)
        {
            src.Play();
            PlayClickSound();
            UpdateChannelText();
        }
        else
        {
            src.Stop();
            PlayClickSound();
            if (ledIndicator != null) ledIndicator.color = new Color(0.15f, 0.15f, 0.15f, 1f); // Выключен
        }
        Debug.Log("[TapePlayer] Воспроизведение: " + isPlaying);
    }

    public void NextChannel()
    {
        if (!isPlaying) return;
        
        currentChannel = (currentChannel + 1) % 3;
        PlayClickSound();
        UpdateChannelText();
        
        // Сброс индексов при смене каналов
        if (currentChannel == 1)
        {
            morseCharIndex = 0;
            morseSymbolIndex = 0;
            morseSymbolTimer = 0f;
        }
    }

    private void PlayClickSound()
    {
        // Воспроизводит олдскульный механический щелчок переключателя через системный динамик
        AudioSource src = GetComponent<AudioSource>();
        if (src != null)
        {
            src.PlayOneShot(src.clip, 0.4f); // Можно использовать любой короткий клип, если назначен
        }
    }

    private void UpdateChannelText()
    {
        if (channelTextDisplay == null) return;
        
        switch (currentChannel)
        {
            case 0:
                channelTextDisplay.text = "CH1: COYUZ-FM\n<size=18><color=#dd2222>* REVERB *</color></size>";
                break;
            case 1:
                channelTextDisplay.text = "CH2: MILITARY\n<size=18><color=#22dd22>* ENCRYPTED *</color></size>";
                break;
            case 2:
                channelTextDisplay.text = "CH3: DARK DUST\n<size=18><color=#dd8822>* SUB BASS *</color></size>";
                break;
        }
    }

    private void SetChord(int index)
    {
        switch (index)
        {
            case 0: // A minor (A3, C4, E4)
                targetChordFreqs[0] = 220.00f; targetChordFreqs[1] = 261.63f; targetChordFreqs[2] = 329.63f;
                break;
            case 1: // F major (F3, A3, C4)
                targetChordFreqs[0] = 174.61f; targetChordFreqs[1] = 220.00f; targetChordFreqs[2] = 261.63f;
                break;
            case 2: // D minor (D3, F3, A3)
                targetChordFreqs[0] = 146.83f; targetChordFreqs[1] = 174.61f; targetChordFreqs[2] = 220.00f;
                break;
            case 3: // E major (E3, G#3, B3)
                targetChordFreqs[0] = 164.81f; targetChordFreqs[1] = 207.65f; targetChordFreqs[2] = 246.94f;
                break;
        }
    }

    // --- ПРОЦЕДУРНЫЙ СИНТЕЗАТОР ЗВУКА ---
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (!isPlaying) return;

        double sampleDuration = 1.0 / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += 2.0 * Mathf.PI * sampleDuration;
            if (phase > 2.0 * Mathf.PI) phase -= 2.0 * Mathf.PI;

            float sample = 0f;

            // ГЕНЕРАЦИЯ ЛЕНТОЧНОГО ШУМА (Шорох старой кассеты)
            float white = (float)(rand.NextDouble() * 2.0 - 1.0);
            lastNoiseVal = (lastNoiseVal + 0.05f * white) / 1.05f; // Низкочастотный фильтр шума
            float tapeHiss = lastNoiseVal * 0.05f;

            // Единичные потрескивания ленты
            float crackle = 0f;
            if (rand.NextDouble() < tapeCrackleChance)
            {
                crackle = (float)(rand.NextDouble() * 2.0 - 1.0) * 0.7f;
            }

            // РЕЖИМ 0: Ретро-Пад (Аналоговый синтезатор)
            if (currentChannel == 0)
            {
                // Нестабильность скорости пленки (Wow & Flutter)
                float wowFlutter = Mathf.Sin((float)phase * 0.05f) * 0.005f;
                
                float wave = 0f;
                for (int c = 0; c < 3; c++)
                {
                    float freq = currentChordFreqs[c] * (1f + wowFlutter);
                    // Смесь треугольной и синусоидальной волны для мягкого теплого тембра
                    float phaseOffset = (float)(phase * freq);
                    float sinWave = Mathf.Sin(phaseOffset);
                    float triWave = Mathf.PingPong(phaseOffset / Mathf.PI, 1f) * 2f - 1f;
                    
                    wave += Mathf.Lerp(sinWave, triWave, 0.4f) * 0.25f;
                }
                
                sample = (wave + tapeHiss + crackle) * playerVolume;
            }
            // РЕЖИМ 1: Военный Коротковолновый Приемник & Морзянка
            else if (currentChannel == 1)
            {
                // Постоянное шипение радио
                float radioStatic = white * 0.2f;

                // Писк морзянки (800 Гц)
                float morseTone = 0f;
                if (morseActive)
                {
                    morseTone = Mathf.Sin((float)(phase * 800.0)) * 0.2f;
                }

                sample = (morseTone + radioStatic + crackle) * playerVolume;
            }
            // РЕЖИМ 2: Мрачный Подземный Гул
            else if (currentChannel == 2)
            {
                // Тяжелый низкочастотный гул (35 Гц + 70 Гц + 105 Гц)
                float bassFreq = 35.0f;
                float bass1 = Mathf.Sin((float)(phase * bassFreq)) * 0.6f;
                float bass2 = Mathf.Sin((float)(phase * bassFreq * 2f)) * 0.25f;
                float bass3 = Mathf.Sin((float)(phase * bassFreq * 3f)) * 0.15f;

                // Медленные атмосферные биения
                float beat = 0.5f + Mathf.Sin((float)phase * 0.1f) * 0.5f;

                sample = ((bass1 + bass2 + bass3) * beat * 0.7f + tapeHiss * 0.3f) * playerVolume;
            }

            // Вывод в левый и правый каналы
            for (int c = 0; c < channels; c++)
            {
                data[i + c] += sample;
            }
        }
    }
}
