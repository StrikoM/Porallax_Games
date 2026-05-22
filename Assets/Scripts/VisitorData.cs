using UnityEngine;

[CreateAssetMenu(fileName = "New Visitor", menuName = "Parallax/Visitor Data")]
public class VisitorData : ScriptableObject
{
    public Sprite visitorSprite;     // Картинка (внешность)
    public Sprite dossierSprite;     // Нормальное фото в базе данных (монитор)
    
    [Header("Данные из Досье (База данных)")]
    public string dossierName;       // Настоящее имя
    public string dossierId;         // Настоящий ID
    public string dossierEyes;       // Настоящие глаза
    public string dossierExpDate;    // Срок действия (база)

    [Header("Данные из Паспорта (Документ)")]
    public string passportName;      // Имя в бумажке
    public string passportId;        // ID в бумажке
    public string passportEyes;      // Глаза в бумажке
    public string passportExpDate;   // Срок действия (бумажка)
    
    [Header("Секретные данные (только для системы)")]
    public bool isMonster;           // Является ли он подделкой?
    public bool isImpatient;         // Монстр, который стучит по стеклу
    public bool isMimic;             // Монстр, который странно дышит и повторяет фразы
    
    [TextArea(2, 5)]
    public string welcomeSpeech;     // Что он говорит при входе?
    
    [Header("Ответы на допрос (Вопросы)")]
    [TextArea(2, 3)]
    public string responseName; // Ответ на "Почему не совпадает имя/ID?"
    
    [TextArea(2, 3)]
    public string responseEyes; // Ответ на "Что с вашими глазами?"
    
    [TextArea(2, 3)]
    public string responseDate; // Ответ на "Ваш паспорт просрочен?"
}
