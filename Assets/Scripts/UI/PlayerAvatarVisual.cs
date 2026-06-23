using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Lightweight presentation-only avatar used outside of battle (Hub, Gym Map, Gym
// Entry). Mirrors the spirit of BattleFighterVisual's idle/motion math but stays
// independent of battle-specific poses, moves, or status effects - this is the
// "same fighter" the player sees walking the world, not fighting in it.
public class PlayerAvatarVisual : MonoBehaviour
{
    RectTransform rootRect;
    RectTransform spriteRect;
    Image spriteImage;

    Vector3 baseSpriteScale;
    Vector2 baseSpritePosition;
    float idlePhase;
    float weightShiftPhase;
    bool facesRight;
    ArchetypeType currentArchetype;
    Color currentTheme;

    Coroutine idleRoutine;
    Coroutine moveRoutine;

    public void Initialize(Image image, ArchetypeType archetype, Color themeColor, bool faceRight)
    {
        rootRect = (RectTransform)transform;
        spriteImage = image;
        spriteRect = image.rectTransform;
        facesRight = faceRight;
        currentArchetype = archetype;
        currentTheme = themeColor;

        UIFactory.SetPlayerAvatar(image, archetype, themeColor);

        baseSpritePosition = spriteRect.anchoredPosition;
        baseSpriteScale = new Vector3(faceRight ? 1f : -1f, 1f, 1f);
        spriteRect.localScale = baseSpriteScale;
        idlePhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        weightShiftPhase = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        PlayIdle();
    }

    public void PlayIdle()
    {
        StopIdle();
        if (gameObject.activeInHierarchy) idleRoutine = StartCoroutine(IdleRoutine());
    }

    // Animates the marker's own anchors toward a new normalized position (the same
    // anchorMin/anchorMax convention every screen in this project already uses), with
    // a walk-cycle-style bounce/squash, a directional flip, and a snappy arrival pop -
    // no sprite sheet needed unless a dedicated walk sprite is supplied.
    public void MoveToAnchor(Vector2 targetAnchorMin, Vector2 targetAnchorMax, float duration, Action onComplete = null)
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        StopIdle();
        moveRoutine = StartCoroutine(MoveRoutine(targetAnchorMin, targetAnchorMax, duration, onComplete));
    }

    IEnumerator IdleRoutine()
    {
        GetIdleProfile(currentArchetype, out float speed, out float bounce, out float shift, out float breath, out float sway);
        float t = idlePhase;
        float shiftT = weightShiftPhase;
        while (true)
        {
            t += Time.unscaledDeltaTime * speed;
            shiftT += Time.unscaledDeltaTime * 0.55f;
            float wave = Mathf.Sin(t);
            float halfWave = Mathf.Sin(t * 0.5f + idlePhase);
            float weightShift = Mathf.Sin(shiftT) * shift * 0.7f;
            spriteRect.anchoredPosition = baseSpritePosition + new Vector2(halfWave * shift * 0.6f + weightShift, wave * bounce);
            spriteRect.localScale = new Vector3(baseSpriteScale.x * (1f + breath * halfWave), 1f + breath * wave, 1f);
            spriteRect.localRotation = Quaternion.Euler(0f, 0f, halfWave * sway);
            yield return null;
        }
    }

    IEnumerator MoveRoutine(Vector2 targetMin, Vector2 targetMax, float duration, Action onComplete)
    {
        Vector2 startMin = rootRect.anchorMin;
        Vector2 startMax = rootRect.anchorMax;

        float startCenterX = (startMin.x + startMax.x) * 0.5f;
        float targetCenterX = (targetMin.x + targetMax.x) * 0.5f;
        float startCenterY = (startMin.y + startMax.y) * 0.5f;
        float targetCenterY = (targetMin.y + targetMax.y) * 0.5f;
        bool horizontalTravel = Mathf.Abs(targetCenterX - startCenterX) > Mathf.Abs(targetCenterY - startCenterY) * 0.5f;
        if (horizontalTravel && Mathf.Abs(targetCenterX - startCenterX) > 0.0001f)
            SetFacing(targetCenterX > startCenterX);

        UIFactory.SetPlayerAvatarWalk(spriteImage, currentArchetype, currentTheme);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            float eased = EaseOutBack(p);
            rootRect.anchorMin = Vector2.LerpUnclamped(startMin, targetMin, eased);
            rootRect.anchorMax = Vector2.LerpUnclamped(startMax, targetMax, eased);

            float stepWave = Mathf.Abs(Mathf.Sin(p * Mathf.PI * 5f));
            spriteRect.localScale = new Vector3(baseSpriteScale.x * (1f - stepWave * 0.08f), 1f + stepWave * 0.14f, 1f);
            spriteRect.anchoredPosition = baseSpritePosition + new Vector2(0f, stepWave * 8f);
            yield return null;
        }

        rootRect.anchorMin = targetMin;
        rootRect.anchorMax = targetMax;
        spriteRect.anchoredPosition = baseSpritePosition;

        // A brief arrival pop - simple, cheap "you've arrived" feedback with no new assets.
        float popElapsed = 0f;
        const float popDuration = 0.15f;
        while (popElapsed < popDuration)
        {
            popElapsed += Time.unscaledDeltaTime;
            float pp = Mathf.Clamp01(popElapsed / popDuration);
            float wave = Mathf.Sin(pp * Mathf.PI);
            spriteRect.localScale = new Vector3(baseSpriteScale.x * (1f - wave * 0.08f), 1f + wave * 0.16f, 1f);
            yield return null;
        }
        spriteRect.localScale = baseSpriteScale;

        UIFactory.SetPlayerAvatar(spriteImage, currentArchetype, currentTheme);

        moveRoutine = null;
        PlayIdle();
        onComplete?.Invoke();
    }

    static float EaseOutBack(float p)
    {
        const float c1 = 0.9f;
        const float c3 = c1 + 1f;
        float x = p - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }

    static void GetIdleProfile(ArchetypeType type, out float speed, out float bounce, out float shift, out float breath, out float sway)
    {
        switch (type)
        {
            case ArchetypeType.Boxer:
                speed = 2.6f; bounce = 2.6f; shift = 2.2f; breath = 0.012f; sway = 0.55f;
                break;
            case ArchetypeType.Wrestler:
                speed = 1.3f; bounce = 1.1f; shift = 1.1f; breath = 0.008f; sway = 0.28f;
                break;
            case ArchetypeType.BjjSpecialist:
                speed = 1.1f; bounce = 1.6f; shift = 2.6f; breath = 0.011f; sway = 0.8f;
                break;
            case ArchetypeType.MuayThaiFighter:
                speed = 2.0f; bounce = 2.1f; shift = 1.8f; breath = 0.012f; sway = 0.6f;
                break;
            default:
                speed = 1.7f; bounce = 1.9f; shift = 1.9f; breath = 0.01f; sway = 0.45f;
                break;
        }
    }

    void SetFacing(bool faceRight)
    {
        if (facesRight == faceRight) return;
        facesRight = faceRight;
        baseSpriteScale = new Vector3(faceRight ? 1f : -1f, 1f, 1f);
    }

    void StopIdle()
    {
        if (idleRoutine != null) StopCoroutine(idleRoutine);
        idleRoutine = null;
        if (spriteRect != null)
        {
            spriteRect.anchoredPosition = baseSpritePosition;
            spriteRect.localScale = baseSpriteScale;
            spriteRect.localRotation = Quaternion.identity;
        }
    }

    void OnDisable()
    {
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = null;
        StopIdle();
    }
}
