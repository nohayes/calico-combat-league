using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Attached to every Slider by UIFactory.CreateSlider. Animates toward a target
// value instead of snapping, so health/stamina changes read as smooth motion.
// Does not change what the bar represents - purely a presentation tween.
[RequireComponent(typeof(Slider))]
public class SmoothSlider : MonoBehaviour
{
    const float Duration = 0.35f;

    Slider slider;
    Coroutine active;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void SetValue(float target, bool instant = false)
    {
        if (slider == null) slider = GetComponent<Slider>();

        if (instant || !gameObject.activeInHierarchy)
        {
            if (active != null) StopCoroutine(active);
            active = null;
            slider.value = target;
            return;
        }

        if (Mathf.Approximately(slider.value, target)) return;

        if (active != null) StopCoroutine(active);
        active = StartCoroutine(Animate(target));
    }

    IEnumerator Animate(float target)
    {
        float start = slider.value;
        float t = 0f;
        while (t < Duration)
        {
            t += Time.unscaledDeltaTime;
            slider.value = Mathf.Lerp(start, target, t / Duration);
            yield return null;
        }
        slider.value = target;
        active = null;
    }
}
