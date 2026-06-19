using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Floating combat text - damage, crit, heal, miss, status. Each call spawns one
// short-lived GameObject that rises, fades, and destroys itself. No pooling:
// battles are short and popups are rare enough (a few per turn) that this is
// negligible allocation, not a hot path (see Milestone 10 performance notes).
public class DamagePopup : MonoBehaviour
{
    public static void Spawn(Transform parent, string text, Color color, bool big)
    {
        var go = new GameObject("Popup", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(180f, 60f);
        rt.anchoredPosition = new Vector2(Random.Range(-14f, 14f), 0f);

        var label = UIFactory.CreateText(go.transform, text, big ? 36 : 26, color, TextAnchor.MiddleCenter,
            Vector2.zero, Vector2.one, FontStyle.Bold);
        label.raycastTarget = false;

        var popup = go.AddComponent<DamagePopup>();
        popup.StartCoroutine(popup.Run(rt, label));
    }

    IEnumerator Run(RectTransform rt, Text label)
    {
        const float duration = 0.75f;
        Vector2 start = rt.anchoredPosition;
        Vector2 end = start + new Vector2(0f, 70f);
        Color baseColor = label.color;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            rt.anchoredPosition = Vector2.Lerp(start, end, p);
            label.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - p);
            float scale = p < 0.15f ? Mathf.Lerp(0.6f, 1.1f, p / 0.15f) : 1f;
            rt.localScale = Vector3.one * scale;
            yield return null;
        }

        Destroy(gameObject);
    }
}
