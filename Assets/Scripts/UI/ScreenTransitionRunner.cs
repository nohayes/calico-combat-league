using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// One of these is attached to every screen's Root by UIScreen. It hosts the
// coroutines a screen needs (enter transition, occasional celebration effect)
// so plain C# UIScreen subclasses don't need to be MonoBehaviours themselves.
public class ScreenTransitionRunner : MonoBehaviour
{
    Coroutine enterRoutine;

    public void PlayEnter(CanvasGroup group, RectTransform rect)
    {
        if (enterRoutine != null) StopCoroutine(enterRoutine);
        enterRoutine = StartCoroutine(EnterRoutine(group, rect));
    }

    IEnumerator EnterRoutine(CanvasGroup group, RectTransform rect)
    {
        const float duration = 0.18f;
        Vector2 startOffset = new Vector2(0f, -36f);

        group.alpha = 0f;
        rect.anchoredPosition = startOffset;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            float eased = 1f - (1f - p) * (1f - p);
            group.alpha = eased;
            rect.anchoredPosition = Vector2.Lerp(startOffset, Vector2.zero, eased);
            yield return null;
        }

        group.alpha = 1f;
        rect.anchoredPosition = Vector2.zero;
        enterRoutine = null;
    }

    // A small, fixed-count celebration burst - not a particle system, just a
    // handful of coroutine-driven Images that drift down and fade, then self-destroy.
    public void SpawnConfetti(Transform container, int count = 16)
    {
        StartCoroutine(ConfettiRoutine(container, count));
    }

    IEnumerator ConfettiRoutine(Transform container, int count)
    {
        var pieces = new List<RectTransform>(count);
        var images = new List<Image>(count);
        var speeds = new List<float>(count);
        var startXs = new List<float>(count);

        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Confetti", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(container, false);
            var rt = go.GetComponent<RectTransform>();
            float startX = Random.Range(0.05f, 0.95f);
            rt.anchorMin = new Vector2(startX, 1.05f);
            rt.anchorMax = new Vector2(startX, 1.05f);
            rt.sizeDelta = new Vector2(14f, 14f);

            var img = go.GetComponent<Image>();
            img.sprite = UIFactory.CircleSprite;
            img.color = ConfettiColors[Random.Range(0, ConfettiColors.Length)];

            pieces.Add(rt);
            images.Add(img);
            speeds.Add(Random.Range(0.6f, 1.15f));
            startXs.Add(startX);
        }

        const float duration = 1.6f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float progress = t / duration;
            for (int i = 0; i < pieces.Count; i++)
            {
                if (pieces[i] == null) continue;
                float fallY = Mathf.Lerp(1.05f, -0.1f, Mathf.Clamp01(progress * speeds[i]));
                pieces[i].anchorMin = new Vector2(startXs[i], fallY);
                pieces[i].anchorMax = new Vector2(startXs[i], fallY);

                var c = images[i].color;
                images[i].color = new Color(c.r, c.g, c.b, Mathf.Clamp01(1.3f - progress));
            }
            yield return null;
        }

        for (int i = 0; i < pieces.Count; i++)
            if (pieces[i] != null) Destroy(pieces[i].gameObject);
    }

    static readonly Color[] ConfettiColors =
    {
        UIFactory.GoldColor, UIFactory.AccentOrange, UIFactory.CreamColor
    };
}
