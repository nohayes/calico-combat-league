using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Smooth value tween with one delayed trailing fill. All routines are replaced,
// never queued, so rapid battle updates remain bounded on mobile.
[RequireComponent(typeof(Slider))]
public class SmoothSlider : MonoBehaviour
{
    const float DrainDuration = 0.22f;
    const float RecoveryDuration = 0.34f;
    const float DamageTrailDelay = 0.14f;
    const float DamageTrailDuration = 0.38f;

    Slider slider;
    Image fillImage;
    Image delayedImage;
    Color baseColor = Color.white;
    bool emphasizeLow;

    Coroutine valueRoutine;
    Coroutine flashRoutine;
    Coroutine lowRoutine;

    void Awake()
    {
        slider = GetComponent<Slider>();
    }

    public void Configure(Image mainFill, Image delayedFill, Color fillColor)
    {
        fillImage = mainFill;
        delayedImage = delayedFill;
        baseColor = fillColor;
    }

    public void SetLowEmphasis(bool enabled)
    {
        emphasizeLow = enabled;
        RefreshLowState();
    }

    public void SetValue(float target, bool instant = false)
    {
        if (slider == null) slider = GetComponent<Slider>();
        target = Mathf.Clamp01(target);

        if (instant || !gameObject.activeInHierarchy)
        {
            StopValueRoutine();
            slider.value = target;
            if (delayedImage != null) delayedImage.fillAmount = target;
            RefreshLowState();
            return;
        }

        if (Mathf.Approximately(slider.value, target) &&
            (delayedImage == null || Mathf.Approximately(delayedImage.fillAmount, target)))
            return;

        bool decreasing = target < slider.value;
        StopValueRoutine();
        valueRoutine = StartCoroutine(Animate(target, decreasing));
        Flash(decreasing ? new Color(1f, 0.82f, 0.72f, 1f) : new Color(0.45f, 0.9f, 0.5f, 1f));
    }

    IEnumerator Animate(float target, bool decreasing)
    {
        float mainStart = slider.value;
        float delayedStart = delayedImage != null ? delayedImage.fillAmount : mainStart;
        float mainDuration = decreasing ? DrainDuration : RecoveryDuration;
        float totalDuration = decreasing ? DamageTrailDelay + DamageTrailDuration : RecoveryDuration;
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            slider.value = Mathf.Lerp(mainStart, target, Mathf.Clamp01(elapsed / mainDuration));

            if (delayedImage != null)
            {
                float delayedProgress = decreasing
                    ? Mathf.Clamp01((elapsed - DamageTrailDelay) / DamageTrailDuration)
                    : Mathf.Clamp01(elapsed / RecoveryDuration);
                delayedImage.fillAmount = Mathf.Lerp(delayedStart, target, delayedProgress);
            }
            yield return null;
        }

        slider.value = target;
        if (delayedImage != null) delayedImage.fillAmount = target;
        valueRoutine = null;
        RefreshLowState();
    }

    void Flash(Color flashColor)
    {
        if (fillImage == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(flashColor));
    }

    IEnumerator FlashRoutine(Color flashColor)
    {
        const float duration = 0.2f;
        fillImage.color = flashColor;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            fillImage.color = Color.Lerp(flashColor, baseColor, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        fillImage.color = baseColor;
        flashRoutine = null;
    }

    void RefreshLowState()
    {
        bool shouldPulse = emphasizeLow && slider != null && slider.value <= 0.25f && gameObject.activeInHierarchy;
        if (shouldPulse && lowRoutine == null) lowRoutine = StartCoroutine(LowPulseRoutine());
        else if (!shouldPulse && lowRoutine != null)
        {
            StopCoroutine(lowRoutine);
            lowRoutine = null;
            if (flashRoutine == null && fillImage != null) fillImage.color = baseColor;
        }
    }

    IEnumerator LowPulseRoutine()
    {
        Color lowColor = Color.Lerp(baseColor, UIFactory.DangerColor, 0.7f);
        float t = 0f;
        while (emphasizeLow && slider != null && slider.value <= 0.25f)
        {
            t += Time.unscaledDeltaTime * 4f;
            if (flashRoutine == null && fillImage != null)
                fillImage.color = Color.Lerp(baseColor, lowColor, (Mathf.Sin(t) + 1f) * 0.5f);
            yield return null;
        }
        if (flashRoutine == null && fillImage != null) fillImage.color = baseColor;
        lowRoutine = null;
    }

    void StopValueRoutine()
    {
        if (valueRoutine == null) return;
        StopCoroutine(valueRoutine);
        valueRoutine = null;
    }

    void OnDisable()
    {
        StopAllCoroutines();
        valueRoutine = null;
        flashRoutine = null;
        lowRoutine = null;
        if (fillImage != null) fillImage.color = baseColor;
    }
}
