using UnityEngine;

[ExecuteAlways]
public class MonitorForcePosition : MonoBehaviour
{
    void LateUpdate()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            // ЖЕСТКО блокируем позицию каждый кадр, перебивая ЛЮБЫЕ другие скрипты!
            rt.anchoredPosition = new Vector2(650f, -90f);
        }
    }
}
