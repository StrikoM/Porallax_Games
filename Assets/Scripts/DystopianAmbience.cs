using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DystopianAmbience : MonoBehaviour
{
    [Header("Настройки Атмосферы")]
    [Range(0f, 1f)] public float mainVolume = 0.2f;    // Общая громкость
    public float droneFrequency = 55f;                 // Частота гула (55 Гц - гул старого трансформатора/монитора)
    [Range(0f, 1f)] public float acNoiseLevel = 0.15f; // Громкость вентиляции (коричневый шум)

    private double phase;
    private double sampleRate;
    private System.Random rand = new System.Random();
    private float lastNoise = 0f;
    private float time;

    void Awake()
    {
        sampleRate = AudioSettings.outputSampleRate;

        AudioSource src = GetComponent<AudioSource>();
        src.playOnAwake = true;
        src.loop = true;
        src.spatialBlend = 0f; // Звук везде (2D)
        
        // Чтобы скрипт генерировал звук, AudioSource должен быть включен
        if (!src.isPlaying) src.Play();
    }

    // Эта магическая функция Unity позволяет нам СОЗДАВАТЬ звук прямо из кода (без mp3 файлов!)
    void OnAudioFilterRead(float[] data, int channels)
    {
        double phaseIncrement = (droneFrequency * 2.0 * Mathf.PI) / sampleRate;
        
        for (int i = 0; i < data.Length; i += channels)
        {
            time += 1f / (float)sampleRate;
            phase += phaseIncrement;
            if (phase > 2.0 * Mathf.PI) phase -= 2.0 * Mathf.PI;

            // 1. Низкий электрический гул (Имитация старого оборудования)
            // Медленная пульсация для создания чувства тревоги (Дыхание)
            float droneLfo = (Mathf.Sin(time * 0.5f) + 1f) * 0.5f; 
            // Смешиваем базовую частоту и гармоники, чтобы звук был плотным
            float drone = (Mathf.Sin((float)phase) * 0.6f + 
                           Mathf.Sin((float)phase * 2f) * 0.2f + 
                           Mathf.Sin((float)phase * 3f) * 0.1f) * (0.6f + 0.4f * droneLfo);

            // 2. Вентиляция / Кондиционер (Коричневый шум)
            float whiteNoise = (float)(rand.NextDouble() * 2.0 - 1.0);
            // Простейший фильтр для смягчения шума (срезает резкие высокие частоты)
            lastNoise = (lastNoise + (0.02f * whiteNoise)) / 1.02f;
            float acNoise = lastNoise * acNoiseLevel * 10f; 

            // Смешиваем гул и вентиляцию
            float sample = (drone * 0.5f + acNoise) * mainVolume;

            // Применяем звук на оба уха (левое и правое)
            for (int c = 0; c < channels; c++)
            {
                data[i + c] += sample;
            }
        }
    }
}
