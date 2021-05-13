using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFader : MonoBehaviour
{
    [SerializeField]
    CanvasGroup uiElement;
    
    public void FadeIn(float lerpTime, GameObject gameObject)
    {
        // Make sure canvas is active before fading in, instead of enabling object then calling this function
        gameObject.SetActive(true);
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 1, lerpTime, gameObject, true));
    }

    // Extra parameter "disable" to clarify whether object should be disabled after fading out
    public void FadeOut(float lerpTime, GameObject gameObject, bool disable)
    {
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 0, lerpTime, gameObject, !disable));
    }

    // Adapted from a tutorial found at https://www.youtube.com/watch?v=92Fz3BjjPL8
    public IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float lerpTime, GameObject gameObject, bool enabled)
    {
        float timeStartedLerping = Time.time;
        float timeSinceStarted = Time.time - timeStartedLerping;
        float percentageComplete = timeSinceStarted / lerpTime;

        while (true)
        {
            timeSinceStarted = Time.time - timeStartedLerping;
            percentageComplete = timeSinceStarted / lerpTime;

            float currentValue = Mathf.Lerp(start, end, percentageComplete);

            cg.alpha = currentValue;

            if (percentageComplete >= 1) break;

            yield return new WaitForEndOfFrame();
        }

        gameObject.SetActive(enabled);

        yield break;
    }
}
