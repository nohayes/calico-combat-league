using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Attached to every button by UIFactory.CreateButton. Gives a small, responsive
// scale change on press/hover - no layout changes, just a transform tween.
public class ButtonPunch : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    const float PressedScale = 0.94f;
    const float HoverScale = 1.03f;
    const float ReturnDuration = 0.12f;

    Vector3 originalScale;
    Coroutine active;
    bool pointerOver;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StopActive();
        transform.localScale = originalScale * PressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateTo(pointerOver ? HoverScale : 1f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pointerOver = true;
        AnimateTo(HoverScale);
        // Milestone 35, Part 2: every button, selectable fighter portrait, and
        // gym entry already has ButtonPunch attached (UIFactory.CreateButton /
        // GymScreen's fighter slots) - hooking the hover sound here covers all
        // three with one change instead of touching every call site.
        AudioManager.Instance?.PlayHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerOver = false;
        AnimateTo(1f);
    }

    void AnimateTo(float scaleMultiplier)
    {
        StopActive();
        active = StartCoroutine(ReturnRoutine(originalScale * scaleMultiplier));
    }

    void StopActive()
    {
        if (active != null) StopCoroutine(active);
        active = null;
    }

    IEnumerator ReturnRoutine(Vector3 target)
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < ReturnDuration)
        {
            t += Time.unscaledDeltaTime;
            transform.localScale = Vector3.Lerp(start, target, t / ReturnDuration);
            yield return null;
        }
        transform.localScale = target;
        active = null;
    }

    void OnDisable()
    {
        StopActive();
        transform.localScale = originalScale;
        pointerOver = false;
    }
}
