using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFader : MonoBehaviour
{
    [SerializeField]
    CanvasGroup uiElement;
    
    public void FadeIn(float lerpTime)
    {
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 1, lerpTime));
    }
    public void FadeOut(float lerpTime)
    {
        StartCoroutine(FadeCanvasGroup(uiElement, uiElement.alpha, 0, lerpTime));
    }

    public IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end, float lerpTime = 1f)
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
    }
}
