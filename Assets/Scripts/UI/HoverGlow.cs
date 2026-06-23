using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Milestone 28: lightweight hover feedback that brightens an Image's color.
// Deliberately avoids touching localScale so it can coexist on the same
// GameObject as ButtonPunch (which already owns scale-based press/hover feedback).
public class HoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image target;
    const float BrightenFactor = 1.2f;

    Color baseColor;

    void Awake()
    {
        if (target == null) target = GetComponent<Image>();
        baseColor = target.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        target.color = new Color(
            Mathf.Min(baseColor.r * BrightenFactor, 1f),
            Mathf.Min(baseColor.g * BrightenFactor, 1f),
            Mathf.Min(baseColor.b * BrightenFactor, 1f),
            baseColor.a);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        target.color = baseColor;
    }
}
