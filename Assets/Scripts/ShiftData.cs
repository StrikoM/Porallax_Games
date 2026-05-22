using UnityEngine;

[CreateAssetMenu(fileName = "New Shift", menuName = "Parallax/Shift Data")]
public class ShiftData : ScriptableObject
{
    [Header("Информация о смене")]
    public string shiftName = "Смена 1";
    
    [TextArea(2, 4)]
    public string directiveText = "Особых указаний нет. Проверяйте документы.";
    
    [Header("Очередь посетителей")]
    public VisitorData[] shiftVisitors; 

    [Header("Газета смены")]
    public bool hasNewspaper = false;
    public string newspaperHeadline = "";
    [TextArea(3, 6)]
    public string newspaperBody = ""; 
}
