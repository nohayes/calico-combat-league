using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Lightweight full-body fighter presentation. Idle and action motion use separate
// transforms, with at most one coroutine of each type active per combatant.
public class BattleFighterVisual : MonoBehaviour
{
    RectTransform rootRect;
    RectTransform spriteRect;
    Image spriteImage;
    string fighterId;
    ArchetypeType archetype;
    Color themeColor;
    bool facesRight;

    Vector2 baseRootPosition;
    Vector3 baseRootScale;
    Quaternion baseRootRotation;
    Vector2 baseSpritePosition;
    Vector3 baseSpriteScale;
    Quaternion baseSpriteRotation;
    Color baseSpriteColor;
    float idlePhase;

    Coroutine idleRoutine;
    Coroutine actionRoutine;

    public void Initialize(Image image, string id, ArchetypeType fighterArchetype, Color color, bool faceRight)
    {
        rootRect = (RectTransform)transform;
        spriteImage = image;
        spriteRect = image.rectTransform;
        fighterId = id;
        archetype = fighterArchetype;
        themeColor = color;
        facesRight = faceRight;

        baseRootPosition = rootRect.anchoredPosition;
        baseRootScale = rootRect.localScale;
        baseRootRotation = rootRect.localRotation;
        baseSpritePosition = GetStanceOffset(archetype);
        baseSpriteScale = new Vector3(facesRight ? 1f : -1f, 1f, 1f);
        baseSpriteRotation = Quaternion.identity;
        idlePhase = Random.Range(0f, Mathf.PI * 2f);

        ResetForBattle();
    }

    public void ResetForBattle()
    {
        StopAllCoroutines();
        idleRoutine = null;
        actionRoutine = null;
        ApplyPoseSprite(FighterSpritePose.Idle);
        ResetTransforms();
        if (gameObject.activeInHierarchy) idleRoutine = StartCoroutine(IdleRoutine());
    }

    public void PlayAttack(MoveType moveType)
    {
        float distance;
        float lift;
        float scale;
        float duration;

        switch (moveType)
        {
            case MoveType.Wrestling:
            case MoveType.Judo:
            case MoveType.GroundAndPound:
                distance = 54f;
                lift = -5f;
                scale = 1.1f;
                duration = 0.27f;
                break;
            case MoveType.Kickboxing:
            case MoveType.MuayThai:
            case MoveType.Karate:
            case MoveType.Taekwondo:
                distance = 43f;
                lift = 10f;
                scale = 1.07f;
                duration = 0.23f;
                break;
            case MoveType.BrazilianJiuJitsu:
                distance = 38f;
                lift = -7f;
                scale = 1.08f;
                duration = 0.25f;
                break;
            default:
                distance = 30f;
                lift = 2f;
                scale = 1.045f;
                duration = 0.16f;
                break;
        }

        StartAction(MotionRoutine(new Vector2(facesRight ? distance : -distance, lift), scale,
            facesRight ? -2f : 2f, duration, 0f));
    }

    public void Recoil(float weight, bool critical)
    {
        float distance = Mathf.Lerp(18f, 46f, Mathf.InverseLerp(0.75f, 3f, weight));
        float scale = critical ? 0.9f : 0.96f;
        float rotation = (facesRight ? -1f : 1f) * (critical ? 7f : 3f);
        StartAction(MotionRoutine(new Vector2(facesRight ? -distance : distance, critical ? 7f : 3f), scale,
            rotation, critical ? 0.42f : 0.27f, critical ? 10f : 4f));
    }

    public void Dodge()
    {
        StartAction(MotionRoutine(new Vector2(facesRight ? -20f : 20f, 18f), 0.96f,
            facesRight ? 4f : -4f, 0.3f, 0f));
    }

    public void PlayStatus(StatusEffectType type)
    {
        Color flashColor;
        float rotation;
        float shake;
        switch (type)
        {
            case StatusEffectType.Bleed:
                flashColor = new Color(0.9f, 0.18f, 0.16f, 1f);
                rotation = 0f;
                shake = 3f;
                break;
            case StatusEffectType.Stun:
                flashColor = UIFactory.GoldColor;
                rotation = 6f;
                shake = 7f;
                break;
            case StatusEffectType.DefenseDown:
                flashColor = new Color(0.75f, 0.45f, 0.25f, 1f);
                rotation = -3f;
                shake = 4f;
                break;
            default:
                flashColor = new Color(0.42f, 0.62f, 0.9f, 1f);
                rotation = 2f;
                shake = 1f;
                break;
        }

        StartAction(StatusRoutine(flashColor, rotation, shake));
    }

    public void PlayVictoryPose(bool champion, bool leader)
    {
        StopIdle();
        ApplyPoseSprite(FighterSpritePose.Victory);
        float lift = champion ? 26f : 16f;
        float scale = champion ? 1.18f : (leader ? 1.12f : 1.09f);
        float rotation = archetype == ArchetypeType.Boxer ? -3f :
            archetype == ArchetypeType.MuayThaiFighter ? 3f : 0f;
        StartAction(HoldPoseRoutine(new Vector2(0f, lift), scale, rotation, champion ? UIFactory.GoldColor : baseSpriteColor));
    }

    public void PlayDefeatPose()
    {
        StopIdle();
        ApplyPoseSprite(FighterSpritePose.Defeat);
        StartAction(HoldPoseRoutine(new Vector2(facesRight ? -12f : 12f, -20f), 0.92f,
            facesRight ? -8f : 8f, new Color(baseSpriteColor.r * 0.72f, baseSpriteColor.g * 0.72f,
                baseSpriteColor.b * 0.72f, baseSpriteColor.a)));
    }

    IEnumerator IdleRoutine()
    {
        GetIdleProfile(archetype, out float speed, out float bounce, out float shift, out float breath, out float sway);
        float t = idlePhase;
        while (true)
        {
            t += Time.unscaledDeltaTime * speed;
            float wave = Mathf.Sin(t);
            float halfWave = Mathf.Sin(t * 0.5f + idlePhase);
            spriteRect.anchoredPosition = baseSpritePosition + new Vector2(halfWave * shift, wave * bounce);
            spriteRect.localScale = new Vector3(baseSpriteScale.x * (1f + breath * halfWave),
                1f + breath * wave, 1f);
            spriteRect.localRotation = Quaternion.Euler(0f, 0f, halfWave * sway);
            yield return null;
        }
    }

    IEnumerator MotionRoutine(Vector2 peakOffset, float peakScale, float peakRotation, float duration, float shake)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            float wave = Mathf.Sin(p * Mathf.PI);
            float damper = 1f - p;
            Vector2 jitter = shake > 0f
                ? new Vector2(Random.Range(-1f, 1f), Random.Range(-0.4f, 0.4f)) * shake * damper
                : Vector2.zero;
            rootRect.anchoredPosition = baseRootPosition + peakOffset * wave + jitter;
            rootRect.localScale = Vector3.Lerp(baseRootScale, baseRootScale * peakScale, wave);
            rootRect.localRotation = Quaternion.Euler(0f, 0f, peakRotation * wave);
            yield return null;
        }
        ResetRootTransform();
        actionRoutine = null;
    }

    IEnumerator StatusRoutine(Color flashColor, float rotation, float shake)
    {
        Color startColor = baseSpriteColor;
        const float duration = 0.34f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsed / duration);
            float wave = Mathf.Sin(p * Mathf.PI);
            spriteImage.color = Color.Lerp(startColor, flashColor, wave);
            rootRect.localRotation = Quaternion.Euler(0f, 0f, rotation * wave);
            rootRect.anchoredPosition = baseRootPosition + new Vector2(Random.Range(-1f, 1f) * shake * wave, 0f);
            yield return null;
        }
        spriteImage.color = startColor;
        ResetRootTransform();
        actionRoutine = null;
    }

    IEnumerator HoldPoseRoutine(Vector2 offset, float scale, float rotation, Color targetColor)
    {
        Color startColor = baseSpriteColor;
        float elapsed = 0f;
        const float duration = 0.4f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float p = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 3f);
            rootRect.anchoredPosition = Vector2.Lerp(baseRootPosition, baseRootPosition + offset, p);
            rootRect.localScale = Vector3.Lerp(baseRootScale, baseRootScale * scale, p);
            rootRect.localRotation = Quaternion.Euler(0f, 0f, rotation * p);
            spriteImage.color = Color.Lerp(startColor, targetColor, p * 0.35f);
            yield return null;
        }
        actionRoutine = null;
    }

    void ApplyPoseSprite(FighterSpritePose pose)
    {
        UIFactory.SetFighterBattleSprite(spriteImage, fighterId, archetype, pose, themeColor);
        baseSpriteColor = spriteImage.color;
    }

    void StartAction(IEnumerator routine)
    {
        if (actionRoutine != null) StopCoroutine(actionRoutine);
        ResetRootTransform();
        spriteImage.color = baseSpriteColor;
        actionRoutine = StartCoroutine(routine);
    }

    void StopIdle()
    {
        if (idleRoutine != null) StopCoroutine(idleRoutine);
        idleRoutine = null;
        spriteRect.anchoredPosition = baseSpritePosition;
        spriteRect.localScale = baseSpriteScale;
        spriteRect.localRotation = baseSpriteRotation;
    }

    void ResetTransforms()
    {
        ResetRootTransform();
        spriteRect.anchoredPosition = baseSpritePosition;
        spriteRect.localScale = baseSpriteScale;
        spriteRect.localRotation = baseSpriteRotation;
        spriteImage.color = baseSpriteColor;
    }

    void ResetRootTransform()
    {
        rootRect.anchoredPosition = baseRootPosition;
        rootRect.localScale = baseRootScale;
        rootRect.localRotation = baseRootRotation;
    }

    static Vector2 GetStanceOffset(ArchetypeType type)
    {
        switch (type)
        {
            case ArchetypeType.Wrestler: return new Vector2(0f, -6f);
            case ArchetypeType.BjjSpecialist: return new Vector2(0f, -3f);
            case ArchetypeType.MuayThaiFighter: return new Vector2(0f, 3f);
            default: return Vector2.zero;
        }
    }

    static void GetIdleProfile(ArchetypeType type, out float speed, out float bounce, out float shift,
        out float breath, out float sway)
    {
        switch (type)
        {
            case ArchetypeType.Boxer:
                speed = 3.4f; bounce = 4.5f; shift = 3f; breath = 0.014f; sway = 0.7f;
                break;
            case ArchetypeType.Wrestler:
                speed = 1.45f; bounce = 1.2f; shift = 1.2f; breath = 0.009f; sway = 0.3f;
                break;
            case ArchetypeType.BjjSpecialist:
                speed = 1.15f; bounce = 1.8f; shift = 3.5f; breath = 0.012f; sway = 1.1f;
                break;
            case ArchetypeType.MuayThaiFighter:
                speed = 2.45f; bounce = 3.2f; shift = 2.2f; breath = 0.013f; sway = 0.8f;
                break;
            default:
                speed = 1.8f; bounce = 2f; shift = 2f; breath = 0.01f; sway = 0.5f;
                break;
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        idleRoutine = null;
        actionRoutine = null;
        if (rootRect != null && spriteRect != null) ResetTransforms();
    }
}
