using UnityEngine;

// Landscape Conversion (Milestone 26): every screen in this game is authored
// with normalized 0-1 anchors, so the actual fill logic here barely matters to
// them - they just adapt to whatever shape this frame ends up being. Landscape
// (aspect >= 1) is now the primary, native target (1920x1080 / 16:9) and fills
// the window completely, no cropping or capping. A portrait window (an actual
// phone, held upright) is the secondary/future case: rather than squishing the
// landscape-authored layouts into a tall narrow shape, the frame is held at a
// widescreen aspect and letterboxed (bars above/below) so the design still
// reads correctly - mobile support is preserved, just not the primary target.
[RequireComponent(typeof(RectTransform))]
public class DesktopFrameFitter : MonoBehaviour
{
    const float TargetLandscapeAspect = 1920f / 1080f;
    const float LandscapeThreshold = 1f;

    RectTransform rectTransform;
    int lastWidth = -1;
    int lastHeight = -1;

    void Awake()
    {
        rectTransform = (RectTransform)transform;
        Apply();
    }

    void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight) Apply();
    }

    void Apply()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        if (Screen.height <= 0) return;

        float aspect = (float)Screen.width / Screen.height;
        if (aspect >= LandscapeThreshold)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
        }
        else
        {
            float heightFraction = Mathf.Clamp01(aspect / TargetLandscapeAspect);
            float yMin = (1f - heightFraction) * 0.5f;
            rectTransform.anchorMin = new Vector2(0f, yMin);
            rectTransform.anchorMax = new Vector2(1f, 1f - yMin);
        }
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}
