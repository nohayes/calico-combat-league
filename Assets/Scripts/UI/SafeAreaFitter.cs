using UnityEngine;

// Keeps a full-stretch RectTransform inside Screen.safeArea so UI avoids
// notches, status bars, and the home indicator on phones that have them.
[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rectTransform;
    Rect lastSafeArea;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Apply();
    }

    void Update()
    {
        if (Screen.safeArea != lastSafeArea) Apply();
    }

    void Apply()
    {
        var safeArea = Screen.safeArea;
        lastSafeArea = safeArea;

        if (Screen.width <= 0 || Screen.height <= 0) return;

        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
