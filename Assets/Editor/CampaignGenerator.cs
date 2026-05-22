using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class CampaignGenerator : EditorWindow
{
    [MenuItem("Parallax/Сгенерировать 7 Дней Игры (Кампанию)")]
    public static void GenerateCampaign()
    {
        string folderPath = "Assets/Campaign";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "Campaign");
        }

        string[] visitorGuids = AssetDatabase.FindAssets("t:VisitorData", new[] { "Assets" });
        List<Sprite> normalSprites = new List<Sprite>();
        List<Sprite> monsterSprites = new List<Sprite>();

        foreach (string guid in visitorGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            // Игнорируем то, что мы уже сгенерировали, чтобы не было рекурсии
            if (path.Contains("Campaign")) continue;

            VisitorData vd = AssetDatabase.LoadAssetAtPath<VisitorData>(path);
            if (vd != null && vd.visitorSprite != null)
            {
                if (vd.isMonster)
                    monsterSprites.Add(vd.visitorSprite);
                else
                    normalSprites.Add(vd.visitorSprite);
            }
        }

        if (normalSprites.Count == 0 && monsterSprites.Count == 0)
        {
            EditorUtility.DisplayDialog("Ошибка", "В проекте не найдено ни одного файла VisitorData с картинками!", "OK");
            return;
        }

        if (normalSprites.Count == 0) normalSprites.AddRange(monsterSprites);
        if (monsterSprites.Count == 0) monsterSprites.AddRange(normalSprites);

        string[] names = { "Иван", "Алексей", "Петр", "Сергей", "Михаил", "Дмитрий", "Олег", "Анна", "Елена", "Ольга", "Мария", "Наталья" };
        string[] surnames = { "Иванов", "Смирнов", "Попов", "Соколов", "Лебедев", "Козлов", "Новиков", "Морозов", "Волков", "Алексеев" };
        string[] eyes = { "Карие", "Голубые", "Серые", "Зеленые" };

        int globalVisitorCount = 0;

        VisitorData CreateVisitor(bool makeMonster, bool useVisualAnomaly, string folder)
        {
            VisitorData vd = ScriptableObject.CreateInstance<VisitorData>();
            string firstName = names[Random.Range(0, names.Length)];
            string lastName = surnames[Random.Range(0, surnames.Length)];
            
            vd.dossierName = firstName + " " + lastName;
            vd.dossierId = Random.Range(10, 99) + "-" + (char)Random.Range('A', 'Z') + "-" + Random.Range(10, 99);
            vd.dossierEyes = eyes[Random.Range(0, eyes.Length)];

            vd.passportName = vd.dossierName;
            vd.passportId = vd.dossierId;
            vd.passportEyes = vd.dossierEyes;
            vd.isMonster = makeMonster;

            if (makeMonster)
            {
                if (useVisualAnomaly && monsterSprites.Count > 0)
                {
                    vd.visitorSprite = monsterSprites[Random.Range(0, monsterSprites.Count)];
                }
                else
                {
                    vd.visitorSprite = normalSprites[Random.Range(0, normalSprites.Count)];
                    
                    int errorType = Random.Range(0, 3);
                    if (errorType == 0) vd.passportName = firstName + " " + surnames[Random.Range(0, surnames.Length)];
                    else if (errorType == 1) vd.passportId = Random.Range(10, 99) + "-" + (char)Random.Range('A', 'Z') + "-" + Random.Range(10, 99);
                    else if (errorType == 2) vd.passportEyes = eyes[Random.Range(0, eyes.Length)];
                    
                    if (vd.passportEyes == vd.dossierEyes) vd.passportEyes = "Черные";
                }
            }
            else
            {
                vd.visitorSprite = normalSprites[Random.Range(0, normalSprites.Count)];
            }

            globalVisitorCount++;
            AssetDatabase.CreateAsset(vd, $"{folder}/GeneratedVisitor_{globalVisitorCount}.asset");
            return vd;
        }

        ShiftData[] allShifts = new ShiftData[7];

        int[] dayCounts = { 3, 4, 4, 5, 5, 6, 7 };
        int[] dayMonsters = { 1, 1, 2, 2, 3, 3, 4 };
        string[] directives = {
            "Вводный день. Сверяйте ID паспорта.",
            "Внимание на лица! Появились визуальные двойники.",
            "Проверяйте цвет глаз. Шпионы часто ошибаются.",
            "Будьте бдительны. Угроза средняя.",
            "Угроза высокая. Внимательно читайте имена.",
            "Монстры научились подделывать ID. Ищите другие ошибки.",
            "ФИНАЛЬНАЯ СМЕНА. Город рассчитывает на вас."
        };

        for (int i = 0; i < 7; i++)
        {
            ShiftData sd = ScriptableObject.CreateInstance<ShiftData>();
            sd.shiftName = "Смена " + (i + 1);
            sd.directiveText = directives[i];
            
            int total = dayCounts[i];
            int monsters = dayMonsters[i];
            sd.shiftVisitors = new VisitorData[total];

            List<bool> isMonsterList = new List<bool>();
            for (int m = 0; m < monsters; m++) isMonsterList.Add(true);
            for (int n = 0; n < (total - monsters); n++) isMonsterList.Add(false);
            
            for (int k = 0; k < isMonsterList.Count; k++) {
                bool temp = isMonsterList[k];
                int randomIndex = Random.Range(k, isMonsterList.Count);
                isMonsterList[k] = isMonsterList[randomIndex];
                isMonsterList[randomIndex] = temp;
            }

            for (int v = 0; v < total; v++)
            {
                bool isM = isMonsterList[v];
                bool isVisual = (i >= 1) && (Random.value > 0.5f);
                sd.shiftVisitors[v] = CreateVisitor(isM, isVisual, folderPath);
            }

            AssetDatabase.CreateAsset(sd, $"{folderPath}/Shift_{i+1}.asset");
            allShifts[i] = sd;
        }

        GameManager gm = Object.FindAnyObjectByType<GameManager>();
        if (gm != null)
        {
            gm.shiftsDatabase = allShifts;
            EditorUtility.SetDirty(gm);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        EditorUtility.DisplayDialog("Успех!", "Сгенерировано 7 дней и " + globalVisitorCount + " уникальных посетителей!\nОни автоматически загружены в GameManager.", "ОК");
    }
}
