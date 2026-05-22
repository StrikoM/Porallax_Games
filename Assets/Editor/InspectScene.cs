using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class InspectScene : EditorWindow
{
    [MenuItem("Parallax/Inspect Scene Layout")]
    public static void Inspect()
    {
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            File.WriteAllText("inspect_output.txt", "Canvas not found.");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Canvas: {canvas.name}, active: {canvas.gameObject.activeSelf}");
        PrintChildren(canvas.transform, "", sb);
        File.WriteAllText("inspect_output.txt", sb.ToString());
        Debug.Log("[Inspect] Scene layout written to inspect_output.txt");
    }

    private static void PrintChildren(Transform t, string indent, StringBuilder sb)
    {
        RectTransform rt = t.GetComponent<RectTransform>();
        string posInfo = rt != null ? $", Pos: {rt.anchoredPosition3D}, Size: {rt.sizeDelta}, AnchorMin: {rt.anchorMin}, AnchorMax: {rt.anchorMax}, Pivot: {rt.pivot}" : "";
        string compInfo = "";
        UnityEngine.UI.Image img = t.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            string spriteName = img.sprite != null ? img.sprite.name : "null";
            compInfo += $", ImageSprite: {spriteName}, Color: {img.color}, Type: {img.type}";
        }
        UnityEngine.UI.Mask m = t.GetComponent<UnityEngine.UI.Mask>();
        if (m != null)
        {
            compInfo += $", Mask(ShowGraphic: {m.showMaskGraphic})";
        }
        sb.AppendLine($"{indent}- {t.name} (Active: {t.gameObject.activeSelf}){posInfo}{compInfo}");
        for (int i = 0; i < t.childCount; i++)
        {
            PrintChildren(t.GetChild(i), indent + "  ", sb);
        }
    }
}
