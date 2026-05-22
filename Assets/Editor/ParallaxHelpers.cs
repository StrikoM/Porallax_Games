using UnityEngine;
using UnityEngine.EventSystems;
using UnityEditor;

public class ParallaxHelpers : Editor
{
    [MenuItem("Parallax/1. ИСПРАВИТЬ ОШИБКУ КЛИКА (Радикально)")]
    public static void FixInput()
    {
        // 1. Пытаемся заменить модуль на сцене
        var es = Object.FindAnyObjectByType<EventSystem>();
        if (es != null)
        {
            var standalone = es.GetComponent<StandaloneInputModule>();
            if (standalone != null)
            {
                Undo.DestroyObjectImmediate(standalone);
                
                System.Type newType = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
                if (newType != null)
                {
                    Undo.AddComponent(es.gameObject, newType);
                }
            }
        }

        // 2. Радикальное решение: Меняем настройки проекта на "Both" (и старая и новая система)
        SerializedObject projectSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
        SerializedProperty activeInputHandling = projectSettings.FindProperty("activeInputHandler");
        if (activeInputHandling != null)
        {
            activeInputHandling.intValue = 2; // 0 = Old, 1 = New, 2 = Both
            projectSettings.ApplyModifiedProperties();
            Debug.Log("<color=green><b>НАСТРОЙКИ ПРОЕКТА ИЗМЕНЕНЫ!</b> Теперь конфликта кнопок не будет.</color>");
        }
        else
        {
            Debug.LogWarning("Не удалось найти свойство activeInputHandler в настройках.");
        }

        EditorUtility.DisplayDialog("Готово", "Ошибка ввода исправлена в настройках проекта.\n\nВнимание: если Unity предложит перезапустить редактор (Restart Editor), обязательно нажмите Apply и перезапустите!", "ОК");
    }

    [MenuItem("Parallax/2. СБРОСИТЬ ПРОГРЕСС (Начать с 1 дня)")]
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("<color=yellow><b>ПРОГРЕСС СБРОШЕН!</b> Вы снова начнете с 1-й смены.</color>");
    }
}
