using UnityEngine;

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
}
