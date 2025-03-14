using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class FuelItem : MonoBehaviour {
    [SerializeField] private Image blockFade;
    [SerializeField] private Image progressFade;
    [SerializeField] private Image background;

    private Workstation station;
    private GameObject blockFadeGameObject;
    private GameObject progressFadeGameObject;

    private EventTrigger trigger;

    private bool current;
    private Color defaultBgColor;
    private Coroutine blinkCoroutine;
    // private bool started;

    public void Init() {
      station = ServiceLocator.For(this).Get<Workstation>();
      defaultBgColor = background.color;
    }

    private void Update() {
      if (!current) {
        return;
      }

      Progress();
    }

    private void OnDisable() {
      ClearBlinkEffect();
    }

    public void Block() {
      GetTrigger().enabled = false;
      GetFadeGameObject().SetActive(true);
    }

    public void UnBlock() {
      GetTrigger().enabled = true;
      GetFadeGameObject().SetActive(false);
      GetProgressGameObject().SetActive(false);
      ResetFill();
      current = false;
    }

    public void SetCurrent() {
      current = true;
      ResetFill();
      GetProgressGameObject().SetActive(true);
    }

    public void StartBlink(Color color, float time) {
      blinkCoroutine = StartCoroutine(BlinkBackgroundColor(color, time));
    }

    public void ClearBlinkEffect() {
      if (blinkCoroutine != null) {
        StopCoroutine(blinkCoroutine);
        blinkCoroutine = null;
      }

      ResetBackgroundColor();
    }

    private IEnumerator BlinkBackgroundColor(Color color, float time) {
      while (true) {
        /*if (!started) {
          yield return null;
        }*/

        background.color = color;
        yield return new WaitForSeconds(time);
        background.color = defaultBgColor;
        yield return new WaitForSeconds(time);
      }
    }

    private void ResetBackgroundColor() {
      background.color = defaultBgColor;
    }

    private void Progress() {
      var totalTimeInMilliseconds = station.CurrentProgress.CraftTimeForOneInMilliseconds;
      var currentTimeInMillisecond = station.CurrentProgress.CurrentTimeInMilliseconds;
      if (totalTimeInMilliseconds <= 0) {
        return;
      }

      UpdateFill(currentTimeInMillisecond / totalTimeInMilliseconds);
    }

    private void UpdateFill(float fill) {
      progressFade.fillAmount = fill;
    }

    private void ResetFill() {
      progressFade.fillAmount = 1;
    }

    private GameObject GetFadeGameObject() {
      if (blockFadeGameObject == null) {
        blockFadeGameObject = blockFade.gameObject;
      }

      return blockFadeGameObject;
    }

    private GameObject GetProgressGameObject() {
      if (progressFadeGameObject == null) {
        progressFadeGameObject = progressFade.gameObject;
      }

      return progressFadeGameObject;
    }

    private EventTrigger GetTrigger() {
      if (trigger == null) {
        trigger = gameObject.GetComponent<EventTrigger>();
      }

      return trigger;
    }
  }
}