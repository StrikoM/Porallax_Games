using UnityEngine;
using UnityEditor;
using System.IO;

public class PixelatorTool : EditorWindow
{
    [MenuItem("Parallax/Утилиты/Сделать ВЫБРАННЫЕ картинки пиксельными (64x64)")]
    public static void PixelateSelectedImages()
    {
        Object[] selectedObjects = Selection.objects;
        int count = 0;

        foreach (Object obj in selectedObjects)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(path)) continue;

            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue; // Не картинка

            // Разрешаем чтение
            ti.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            Texture2D original = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (original == null) continue;

            int newSize = 64; // Около 50х50, но 64 - оптимальный размер для Unity

            // Создаем новую пиксельную текстуру
            Texture2D pixelated = new Texture2D(newSize, newSize, TextureFormat.RGBA32, false);
            pixelated.filterMode = FilterMode.Point;

            // Переносим цвета (усредняем блок пикселей для хорошего ретро-вида)
            for (int y = 0; y < newSize; y++)
            {
                for (int x = 0; x < newSize; x++)
                {
                    float u = (float)x / newSize;
                    float v = (float)y / newSize;
                    
                    // Билинейная фильтрация сгладит шум, а потом мы сохраним это как 1 толстый пиксель
                    Color color = original.GetPixelBilinear(u, v); 
                    pixelated.SetPixel(x, y, color);
                }
            }
            pixelated.Apply();

            // Сохраняем поверх оригинала
            byte[] bytes = pixelated.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            // Меняем настройки импорта на ретро-стиль (без размытия)
            ti.isReadable = false;
            ti.filterMode = FilterMode.Point; // Убираем размытие!
            ti.textureCompression = TextureImporterCompression.Uncompressed; // Убираем артефакты сжатия
            ti.maxTextureSize = 256;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            count++;
        }

        AssetDatabase.Refresh();
        
        if (count > 0)
        {
            EditorUtility.DisplayDialog("Готово!", $"Обработано картинок: {count}.\nТеперь они выглядят как трушный ретро-пиксель-арт без размытия!", "Супер");
        }
        else
        {
            EditorUtility.DisplayDialog("Внимание", "Вы не выбрали ни одной картинки!\nСначала выделите картинки персонажей в нижнем окне Project, а потом нажмите эту кнопку.", "Понял");
        }
    }
}
