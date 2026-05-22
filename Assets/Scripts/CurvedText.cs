using UnityEngine;
using TMPro;

[ExecuteInEditMode]
[RequireComponent(typeof(TMP_Text))]
public class CurvedText : MonoBehaviour
{
    [Header("Настройки изгиба")]
    public float curveMultiplier = 1.0f; // Сила изгиба (положительная - выгибает вниз по краям)

    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    void Update()
    {
        if (textComponent == null) return;

        textComponent.ForceMeshUpdate();
        TMP_TextInfo textInfo = textComponent.textInfo;

        if (textInfo == null || textInfo.characterCount == 0) return;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int vertexIndex = charInfo.vertexIndex;
            int materialIndex = charInfo.materialReferenceIndex;
            Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

            for (int j = 0; j < 4; j++)
            {
                // Делаем эффект "выпуклой линзы" (парабола)
                float x = vertices[vertexIndex + j].x;
                // Искривляем по оси Y в зависимости от отдаления от центра (X)
                vertices[vertexIndex + j].y -= Mathf.Pow(x * 0.002f, 2) * curveMultiplier;
            }
        }

        textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
    }
}
