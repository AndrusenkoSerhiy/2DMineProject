using System.Collections;
using UnityEngine;

namespace UI {
  public class TooltipManager : MonoBehaviour {
    public Tooltip tooltip;
    public CanvasGroup canvasGroup;
    public float delay = 0.3f;
    public float fadeDelay = 0.3f;

    private Coroutine showCoroutine;

    public void Show(string content, string header = "") {
      if (showCoroutine != null) {
        StopCoroutine(showCoroutine);
      }

      showCoroutine = StartCoroutine(ShowWithDelay(content, header, delay));
    }

    public void Hide() {
      if (showCoroutine != null) {
        StopCoroutine(showCoroutine);
        showCoroutine = null;
      }

      tooltip.gameObject.SetActive(false);
    }

    private IEnumerator ShowWithDelay(string content, string header, float delay) {
      yield return new WaitForSeconds(delay);
      canvasGroup.alpha = 0f;
      tooltip.SetText(content, header);
      tooltip.gameObject.SetActive(true);
      yield return FadeTo(1f, fadeDelay);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration) {
      var startAlpha = canvasGroup.alpha;
      var time = 0f;

      while (time < duration) {
        canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
        time += Time.deltaTime;
        yield return null;
      }

      canvasGroup.alpha = targetAlpha;
    }
  }
}