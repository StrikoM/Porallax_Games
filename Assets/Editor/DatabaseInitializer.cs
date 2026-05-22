using UnityEngine;
using UnityEditor;
using System.IO;

public class DatabaseInitializer : EditorWindow
{
    [MenuItem("Parallax/Инициализировать базу жителей")]
    public static void InitDatabase()
    {
        // Путь к папке с данными
        string folderPath = "Assets/Data/Shift1";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        // 1. Создаем Виктора (Честный гражданин)
        VisitorData viktor = CreateInstance<VisitorData>();
        viktor.dossierName = "ВИКТОР КОЗЛОВ";
        viktor.dossierId = "44-12-09";
        viktor.dossierEyes = "КАРИЕ";
        viktor.passportName = "ВИКТОР КОЗЛОВ";
        viktor.passportId = "44-12-09";
        viktor.passportEyes = "КАРИЕ";
        viktor.isMonster = false;
        viktor.welcomeSpeech = "Добрый вечер... Опять дожди. Впустите меня скорее, я ужасно устал.";
        viktor.visitorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Visitors/Viktor.png");
        AssetDatabase.CreateAsset(viktor, folderPath + "/ViktorKozlov.asset");

        // 2. Создаем Елену (Ошибка в ID)
        VisitorData elena = CreateInstance<VisitorData>();
        elena.dossierName = "ЕЛЕНА МАРКОВА";
        elena.dossierId = "88-11-22";
        elena.dossierEyes = "ГОЛУБЫЕ";
        elena.passportName = "ЕЛЕНА МАРКОВА";
        elena.passportId = "88-11-99"; // ОШИБКА ТУТ!
        elena.passportEyes = "ГОЛУБЫЕ";
        elena.isMonster = false;
        elena.welcomeSpeech = "Привет! Я сегодня задержалась на работе... Надеюсь, я не последняя в очереди?";
        elena.visitorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Visitors/Elena.png");
        AssetDatabase.CreateAsset(elena, folderPath + "/ElenaMarkova.asset");

        // 3. Создаем "Скрытую угрозу" (Монстр - внешность не та)
        VisitorData monster = CreateInstance<VisitorData>();
        monster.dossierName = "НЕИЗВЕСТНЫЙ";
        monster.dossierId = "00-00-00";
        monster.dossierEyes = "ЗЕЛЕНЫЕ";
        monster.passportName = "ИВАН ИВАНОВ"; 
        monster.passportId = "11-11-11";
        monster.passportEyes = "ЗЕЛЕНЫЕ";
        monster.isMonster = true; 
        monster.welcomeSpeech = "Впусти... меня... ВНУТРЬ... МНЕ... НУЖНО... ДОМОЙ...";
        monster.visitorSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Visitors/Monster.png");
        AssetDatabase.CreateAsset(monster, folderPath + "/UnknownSubject.asset");

        // Создаем саму Смену
        ShiftData shift1 = CreateInstance<ShiftData>();
        shift1.shiftName = "ПЕРВАЯ СМЕНА";
        shift1.directiveText = "ВНИМАНИЕ: Участились случаи подделки ID-карт. Сверяйте каждую цифру!";
        shift1.shiftVisitors = new VisitorData[] { viktor, elena, monster };
        AssetDatabase.CreateAsset(shift1, folderPath + "/Shift1.asset");

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("База данных", "Первая смена и жители успешно созданы в Assets/Data/Shift1!", "Отлично");
    }
}
