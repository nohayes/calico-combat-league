using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Attached to each fighter card in BattleScreen. All card motion is routed through
// one cancellable coroutine so attacks, dodges and reactions cannot accumulate.
public class FighterCardFX : MonoBehaviour
{
    Image cardImage;
    Color baseColor;
    RectTransform rect;
    Vector2 basePosition;
    Vector3 baseScale;
    Quaternion baseRotation;
    Transform popupAnchor;

    Coroutine flashRoutine;
    Coroutine motionRoutine;

    public void Initialize(Image image, Transform popupAnchorTransform)
    {
        cardImage = image;
        baseColor = image.color;
        rect = (RectTransform)transform;
        basePosition = rect.anchoredPosition;
        baseScale = rect.localScale;
        baseRotation = rect.localRotation;
        popupAnchor = popupAnchorTransform;
    }

    public void ClearPopups()
    {
        if (popupAnchor == null) return;
        for (int i = popupAnchor.childCount - 1; i >= 0; i--)
            Destroy(popupAnchor.GetChild(i).gameObject);
    }

    public void SpawnPopup(string text, Color color, bool big = false)
    {
        if (popupAnchor == null) return;
        DamagePopup.Spawn(popupAnchor, text, color, big);
    }

    public void Flash(Color flashColor)
    {
        if (cardImage == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(flashColor));
    }

    public void Shake(float intensity)
    {
        PlayMotion(Vector2.zero, 0.04f, 0.18f, intensity, 1f, 0f, useScaledTime: false);
    }

    public void PlayAttack(MoveType moveType, bool upward)
    {
        float distance;
        float scale;
        float duration;

        switch (moveType)
        {
            case MoveType.Wrestling:
            case MoveType.Judo:
            case MoveType.GroundAndPound:
                distance = 34f;
                scale = 1.045f;
                duration = 0.24f;
                break;
            case MoveType.Kickboxing:
            case MoveType.MuayThai:
            case MoveType.Karate:
            case MoveType.Taekwondo:
                distance = 26f;
                scale = 1.035f;
                duration = 0.2f;
                break;
            case MoveType.BrazilianJiuJitsu:
                distance = 22f;
                scale = 1.04f;
                duration = 0.22f;
                break;
            default:
                distance = 16f;
                scale = 1.025f;
                duration = 0.14f;
                break;
        }

        PlayMotion(new Vector2(0f, upward ? distance : -distance), duration * 0.38f, duration * 0.62f,
            0f, scale, 0f, useScaledTime: true);
    }

    public void Recoil(float weight, bool upward, bool critical)
    {
        float distance = Mathf.Lerp(9f, 24f, Mathf.InverseLerp(0.75f, 3f, weight));
        float shake = critical ? 13f : Mathf.Lerp(4f, 8f, weight / 3f);
        float rotation = (upward ? 1f : -1f) * (critical ? 3.5f : 1.5f);
        PlayMotion(new Vector2(0f, upward ? distance : -distance), 0.06f, critical ? 0.34f : 0.2f,
            shake, critical ? 0.965f : 0.985f, rotation, useScaledTime: true);
    }

    public void Dodge(bool right)
    {
        PlayMotion(new Vector2(right ? 24f : -24f, 4f), 0.08f, 0.18f, 0f, 0.98f,
            right ? -2f : 2f, useScaledTime: true);
    }

    public void PlayStatus(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Bleed:
                Flash(new Color(0.72f, 0.08f, 0.08f, 1f));
                PlayMotion(Vector2.zero, 0.07f, 0.2f, 2f, 1.04f, 0f, useScaledTime: false);
                break;
            case StatusEffectType.Stun:
                Flash(new Color(0.9f, 0.72f, 0.18f, 1f));
                PlayMotion(Vector2.zero, 0.1f, 0.3f, 7f, 1f, 4f, useScaledTime: false);
                break;
            case StatusEffectType.DefenseDown:
                Flash(new Color(0.55f, 0.36f, 0.2f, 1f));
                PlayMotion(new Vector2(0f, -5f), 0.06f, 0.22f, 5f, 0.97f, -2f, useScaledTime: false);
                break;
            case StatusEffectType.SpeedDown:
                Flash(new Color(0.28f, 0.42f, 0.65f, 1f));
                PlayMotion(new Vector2(14f, 0f), 0.14f, 0.38f, 1f, 0.98f, 0f, useScaledTime: false);
                break;
        }
    }

    IEnumerator FlashRoutine(Color flashColor)
    {
        const float duration = 0.18f;
        cardImage.color = flashColor;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            cardImage.color = Color.Lerp(flashColor, baseColor, t / duration);
            yield return null;
        }
        cardImage.color = baseColor;
        flashRoutine = null;
    }

    void PlayMotion(Vector2 peakOffset, float outboundDuration, float returnDuration, float shakeIntensity,
        float peakScale, float peakRotation, bool useScaledTime)
    {
        if (motionRoutine != null) StopCoroutine(motionRoutine);
        motionRoutine = StartCoroutine(MotionRoutine(peakOffset, outboundDuration, returnDuration, shakeIntensity,
            peakScale, peakRotation, useScaledTime));
    }

    IEnumerator MotionRoutine(Vector2 peakOffset, float outboundDuration, float returnDuration, float shakeIntensity,
        float peakScale, float peakRotation, bool useScaledTime)
    {
        float t = 0f;
        while (t < outboundDuration)
        {
            t += useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime;
            float p = outboundDuration > 0f ? Mathf.Clamp01(t / outboundDuration) : 1f;
            rect.anchoredPosition = Vector2.Lerp(basePosition, basePosition + peakOffset, p);
            rect.localScale = Vector3.Lerp(baseScale, baseScale * peakScale, p);
            rect.localRotation = Quaternion.Lerp(baseRotation, Quaternion.Euler(0f, 0f, peakRotation), p);
            yield return null;
        }

        t = 0f;
        while (t < returnDuration)
        {
            t += useScaledTime ? Time.deltaTime : Time.unscaledDeltaTime;
            float p = returnDuration > 0f ? Mathf.Clamp01(t / returnDuration) : 1f;
            float damper = 1f - p;
            Vector2 jitter = shakeIntensity > 0f
                ? new Vector2(Random.Range(-1f, 1f), Random.Range(-0.35f, 0.35f)) * shakeIntensity * damper
                : Vector2.zero;
            rect.anchoredPosition = Vector2.Lerp(basePosition + peakOffset, basePosition, p) + jitter;
            rect.localScale = Vector3.Lerp(baseScale * peakScale, baseScale, p);
            rect.localRotation = Quaternion.Lerp(Quaternion.Euler(0f, 0f, peakRotation), baseRotation, p);
            yield return null;
        }

        rect.anchoredPosition = basePosition;
        rect.localScale = baseScale;
        rect.localRotation = baseRotation;
        motionRoutine = null;
    }

    void OnDisable()
    {
        if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }
        if (motionRoutine != null) { StopCoroutine(motionRoutine); motionRoutine = null; }
        if (cardImage != null) cardImage.color = baseColor;
        if (rect != null)
        {
            rect.anchoredPosition = basePosition;
            rect.localScale = baseScale;
            rect.localRotation = baseRotation;
        }
    }
}
