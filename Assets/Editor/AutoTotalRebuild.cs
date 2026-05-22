using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;

[InitializeOnLoad]
public class AutoTotalRebuild
{
    static AutoTotalRebuild()
    {
        EditorApplication.delayCall += RunOnce;
    }

    [MenuItem("Parallax/TOTAL REBUILD")]
    public static void ManualRebuild()
    {
        // Сбрасываем флаг, чтобы запустить процедуру заново
        EditorPrefs.DeleteKey("AutoTotalRebuild_v1");
        RebuildInternal();
    }

    static void RunOnce()
    {
        if (Application.isPlaying) return;
        if (EditorPrefs.GetBool("AutoTotalRebuild_v1", false)) return;
        EditorPrefs.SetBool("AutoTotalRebuild_v1", true);

        RebuildInternal();
    }

    static void RebuildInternal()
    {
        UnityEngine.Debug.Log("<color=cyan>[Antigravity] НАЧИНАЮ БЕЗОПАСНОЕ ВОССТАНОВЛЕНИЕ ИГРЫ ИЗ БЭКАПА...</color>");

        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string zipPath = Path.Combine(projectPath, "ParallaxGame_Backup.zip");

        if (!File.Exists(zipPath))
        {
            EditorUtility.DisplayDialog("Ошибка", "Файл ParallaxGame_Backup.zip не найден! Пожалуйста, убедитесь, что он находится в корневой папке проекта.", "ОК");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog(
            "Подтверждение восстановления",
            "Вы действительно хотите полностью восстановить красивую физическую версию игры из ZIP-бэкапа?\n\nЭто вернет оригинальный деревянный стол, тяжелую стальную дверь, телефон и все остальные оригинальные элементы, а наша новая система стилизации диалогов автоматически украсит панель диалогов.",
            "Да, восстановить!",
            "Отмена"
        );

        if (!confirm)
        {
            UnityEngine.Debug.Log("[Antigravity] Восстановление отменено пользователем.");
            return;
        }

        string batPath = Path.Combine(projectPath, "restore_backup.bat");
        
        string script = "@echo off\n" +
                        "echo =======================================\n" +
                        "echo ANTIGRAVITY: БЕЗОПАСНОЕ ВОССТАНОВЛЕНИЕ...\n" +
                        "echo Распаковываем оригинальные физические ассеты стола...\n" +
                        "echo =======================================\n" +
                        "cd /d \"" + projectPath + "\"\n" +
                        "powershell -Command \"Expand-Archive -Path 'ParallaxGame_Backup.zip' -DestinationPath '.' -Force\"\n" +
                        "echo.\n" +
                        "echo Распаковка завершена! Теперь вернитесь в Unity.\n" +
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

        // Сбрасываем все флаги украшений и настроек сцены, чтобы они гарантированно сработали при следующей загрузке
        EditorPrefs.DeleteKey("AutoBeautifyDialoguePanel_v5");
        EditorPrefs.DeleteKey("AutoAddInterrogationUI_Active_v2");
        EditorPrefs.DeleteKey("AutoFixWindowAndDialogue_v1");
        EditorPrefs.DeleteKey("AutoFixWindowAndDialogue_v9");
        EditorPrefs.DeleteKey("AutoFixWindowAndDialogue_v10");
        EditorPrefs.DeleteKey("AutoFixWindowAndDialogue_v11");
        EditorPrefs.DeleteKey("AutoFixWindowAndDialogue_v12");
        EditorPrefs.DeleteKey("AutoFixWindowAndDialogue_v13");
        EditorPrefs.DeleteKey("AutoFixWindowAndDialogue_v14");
        EditorPrefs.DeleteKey("AutoFixWindowAndDialogue_v15");

        EditorUtility.DisplayDialog(
            "Запуск восстановления",
            "Запущено окно командной строки для распаковки физического стола.\n\nПожалуйста, дождитесь завершения распаковки в появившемся черном окне консоли, нажмите любую клавишу, а затем вернитесь в Unity.\n\nВам останется лишь применить меню Parallax -> RELOAD SCENE FROM DISK для обновления сцены и автоматической генерации Допроса и Окна!",
            "Понял!"
        );
    }
}
