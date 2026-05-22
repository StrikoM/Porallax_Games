using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;

// [InitializeOnLoad]
public class AutoRestoreZip
{
    static AutoRestoreZip()
    {
        EditorApplication.delayCall += RunOnce;
    }

    [MenuItem("Parallax/RESTORE FROM BACKUP")]
    static void RunOnce()
    {
        if (Application.isPlaying) return;
        if (EditorPrefs.GetBool("AutoRestoreZip_v1", false)) return;
        EditorPrefs.SetBool("AutoRestoreZip_v1", true);

        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string zipPath = Path.Combine(projectPath, "ParallaxGame_Backup.zip");

        if (!File.Exists(zipPath))
        {
            UnityEngine.Debug.LogError("[Antigravity] ОШИБКА: Файл ParallaxGame_Backup.zip не найден! Вы удалили его?");
            return;
        }

        UnityEngine.Debug.Log("[Antigravity] ВОССТАНАВЛИВАЮ ПРОЕКТ ИЗ ZIP БЭКАПА...");

        string batPath = Path.Combine(projectPath, "restore_backup.bat");
        
        // Используем PowerShell для распаковки с заменой файлов
        string script = "@echo off\n" +
                        "echo =======================================\n" +
                        "echo ANTIGRAVITY: ВОССТАНОВЛЕНИЕ ИЗ БЭКАПА...\n" +
                        "echo Пожалуйста, подождите 1-2 минуты!\n" +
                        "echo =======================================\n" +
                        "cd /d \"" + projectPath + "\"\n" +
                        "powershell -Command \"Expand-Archive -Path 'ParallaxGame_Backup.zip' -DestinationPath '.' -Force\"\n" +
                        "echo.\n" +
                        "echo Готово! Unity сейчас перезагрузит файлы.\n" +
                        "pause\n";

        File.WriteAllText(batPath, script);

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = batPath;
        psi.WorkingDirectory = projectPath;
        psi.UseShellExecute = true;

        Process process = Process.Start(psi);
        process.EnableRaisingEvents = true;
        process.Exited += (sender, e) =>
        {
            if (File.Exists(batPath)) File.Delete(batPath);
        };
    }

    [MenuItem("Parallax/RELOAD SCENE FROM DISK (DISCARD CHANGES)")]
    public static void ForceReloadScene()
    {
        if (Application.isPlaying) return;
        string scenePath = "Assets/Scenes/GameScene.unity";
        if (File.Exists(scenePath))
        {
            bool confirm = EditorUtility.DisplayDialog(
                "Сбросить изменения в сцене?",
                "Вы действительно хотите перезагрузить сцену с диска?\n\nВсе не сохраненные в памяти изменения будут сброшены, и загрузится чистая версия из бэкапа.",
                "Да, перезагрузить!",
                "Отмена"
            );
            if (confirm)
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath, UnityEditor.SceneManagement.OpenSceneMode.Single);
                UnityEngine.Debug.Log("[Antigravity] Сцена успешно перезагружена с диска!");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("[Antigravity] Ошибка: файл сцены " + scenePath + " не найден!");
        }
    }
}
