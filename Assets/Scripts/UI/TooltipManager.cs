using System.Collections;
using UnityEngine;

namespace UI {
  public class TooltipManager : MonoBehaviour {
    public Tooltip tooltip;
    public float delay = 0.3f;

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
      tooltip.SetText(content, header);
      tooltip.gameObject.SetActive(true);
    }
  }
}