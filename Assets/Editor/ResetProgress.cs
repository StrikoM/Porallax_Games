using UnityEditor;
using UnityEngine;

public class ResetProgress : Editor
{
    [MenuItem("Parallax/Сбросить прогресс (Начать заново)")]
    public static void ResetGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("Прогресс сброшен! Теперь вы снова на 1 дне.");
    }
}
