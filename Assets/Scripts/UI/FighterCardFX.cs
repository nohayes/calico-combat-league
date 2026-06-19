using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Attached to each fighter card in BattleScreen. Provides the three lightweight
// hit-feedback primitives the battle screen drives from parsed log lines: a brief
// color flash, a small positional shake, and floating combat-text popups.
public class FighterCardFX : MonoBehaviour
{
    Image cardImage;
    Color baseColor;
    RectTransform rect;
    Vector2 basePosition;
    Transform popupAnchor;

    Coroutine flashRoutine;
    Coroutine shakeRoutine;

    public void Initialize(Image image, Transform popupAnchorTransform)
    {
        cardImage = image;
        baseColor = image.color;
        rect = (RectTransform)transform;
        basePosition = rect.anchoredPosition;
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
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeRoutine(intensity));
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

    IEnumerator ShakeRoutine(float intensity)
    {
        const float duration = 0.22f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float damper = 1f - (t / duration);
            float offsetX = Random.Range(-1f, 1f) * intensity * damper;
            rect.anchoredPosition = basePosition + new Vector2(offsetX, 0f);
            yield return null;
        }
        rect.anchoredPosition = basePosition;
        shakeRoutine = null;
    }

    void OnDisable()
    {
        if (flashRoutine != null) { StopCoroutine(flashRoutine); flashRoutine = null; }
        if (shakeRoutine != null) { StopCoroutine(shakeRoutine); shakeRoutine = null; }
        if (cardImage != null) cardImage.color = baseColor;
        if (rect != null) rect.anchoredPosition = basePosition;
    }
}
