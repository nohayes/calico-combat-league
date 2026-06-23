using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Milestone 29: a small, reusable "someone is talking to you" popup for rival
// appearances outside the battle intro (which already owns its own Fight Night
// dialogue flow). Lines only ever advance on an explicit tap - the same rule
// BattleScreen's bio/quote beats already follow - so a one-line quip can never
// be missed to an accidental click-through.
public class RivalDialogueBox : MonoBehaviour
{
    Text nameText;
    Text lineText;
    Text tapPrompt;
    CanvasGroup group;
    RectTransform card;

    List<string> pendingLines;
    int lineIndex;
    bool waitingForTap;
    bool tapRequested;
    Action onComplete;

    public void Initialize(Text nameText, Text lineText, Text tapPrompt, CanvasGroup group, RectTransform card)
    {
        this.nameText = nameText;
        this.lineText = lineText;
        this.tapPrompt = tapPrompt;
        this.group = group;
        this.card = card;
    }

    public void Show(string speakerName, IList<string> lines, Action onComplete = null)
    {
        if (lines == null || lines.Count == 0) return;
        StopAllCoroutines();

        // Milestone 33, Part 9: "rival intro sound if available" - reuses the
        // existing crit stinger rather than adding a new audio asset.
        AudioManager.Instance?.PlayCriticalHit();

        this.onComplete = onComplete;
        pendingLines = new List<string>(lines);
        lineIndex = 0;
        nameText.text = speakerName;
        gameObject.SetActive(true);
        group.alpha = 0f;
        card.localScale = Vector3.one * 0.92f;
        StartCoroutine(RunRoutine());
    }

    public void OnTapped()
    {
        if (!waitingForTap) return;
        tapRequested = true;
    }

    IEnumerator RunRoutine()
    {
        yield return Fade(0f, 1f, 0.18f);
        while (lineIndex < pendingLines.Count)
        {
            lineText.text = pendingLines[lineIndex];
            tapPrompt.gameObject.SetActive(true);
            tapRequested = false;
            waitingForTap = true;
            while (!tapRequested) yield return null;
            waitingForTap = false;
            lineIndex++;
        }
        tapPrompt.gameObject.SetActive(false);
        yield return Fade(1f, 0f, 0.15f);
        gameObject.SetActive(false);

        var callback = onComplete;
        onComplete = null;
        callback?.Invoke();
    }

    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duration);
            group.alpha = Mathf.Lerp(from, to, p);
            card.localScale = Vector3.one * Mathf.Lerp(0.92f, 1f, p);
            yield return null;
        }
        group.alpha = to;
        card.localScale = Vector3.one;
    }
}
