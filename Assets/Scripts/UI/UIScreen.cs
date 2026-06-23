using UnityEngine;
using System.Collections;

public abstract class UIScreen
{
    protected readonly GameObject Root;
    protected readonly GameManager GM;

    readonly CanvasGroup canvasGroup;
    readonly RectTransform rootRect;
    readonly ScreenTransitionRunner transitionRunner;

    protected UIScreen(Transform parent, GameManager gm, string name, string backgroundKey = null)
    {
        GM = gm;
        Root = UIFactory.CreatePanel(parent, name).gameObject;
        if (!string.IsNullOrEmpty(backgroundKey)) UIFactory.ApplyScreenBackground(Root, backgroundKey);
        rootRect = Root.GetComponent<RectTransform>();
        canvasGroup = Root.AddComponent<CanvasGroup>();
        transitionRunner = Root.AddComponent<ScreenTransitionRunner>();
        Root.SetActive(false);
    }

    public void SetVisible(bool visible)
    {
        if (visible)
        {
            Root.SetActive(true);
            transitionRunner.PlayEnter(canvasGroup, rootRect);
        }
        else
        {
            Root.SetActive(false);
        }
    }

    // Lets subclasses (e.g. the Championship screen) trigger a one-off celebration burst.
    protected void PlayCelebration(int pieceCount = 16) => transitionRunner.SpawnConfetti(Root.transform, pieceCount);
    protected void RunAnimation(IEnumerator routine) => transitionRunner.Run(routine);
    protected void PlayTimeImpact(float scale = 0.3f, float duration = 0.12f) => transitionRunner.PlayTimeImpact(scale, duration);
    protected void PlayPulse(RectTransform target, float peakScale = 1.08f, float duration = 0.45f) =>
        transitionRunner.PlayPulse(target, peakScale, duration);
    protected void PlayReveal(CanvasGroup group, RectTransform target, float delay = 0.2f, float duration = 0.35f) =>
        transitionRunner.PlayReveal(group, target, delay, duration);
    protected void PlayExhausted(RectTransform target) => transitionRunner.PlayExhausted(target);
}
