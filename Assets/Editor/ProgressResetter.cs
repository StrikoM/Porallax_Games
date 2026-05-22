using UnityEngine;
using UnityEditor;

public class ProgressResetter : EditorWindow
{
    [MenuItem("Parallax/Сбросить сохранение (Начать с 1 дня)")]
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        EditorUtility.DisplayDialog("Сброс", "Прогресс сброшен. Игра начнется с 1 дня.", "OK");
    }
}
