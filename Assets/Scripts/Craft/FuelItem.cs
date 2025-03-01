using Scriptables.Craft;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class FuelItem : MonoBehaviour {
    [SerializeField] private Image blockFade;
    [SerializeField] private Image progressFade;

    private Workstation station;
    private GameObject blockFadeGameObject;
    private GameObject progressFadeGameObject;
    private EventTrigger trigger;
    private bool blocked;
    private bool current;

    public void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
    }

    public void Update() {
      if (!current) {
        return;
      }

      Progress();
    }

    public void Block() {
      GetTrigger().enabled = false;
      GetFadeGameObject().SetActive(true);
      blocked = true;
    }

    public void UnBlock() {
      GetTrigger().enabled = true;
      GetFadeGameObject().SetActive(false);
      GetProgressGameObject().SetActive(false);
      ResetFill();
      blocked = false;
      current = false;
    }

    public void SetCurrent() {
      current = true;
      ResetFill();
      GetProgressGameObject().SetActive(true);
    }

    private void Progress() {
      var totalTime = station.GetProgressCraftTime();
      var currentTime = station.GetProgressTime();
      if (totalTime <= 0) {
        return;
      }

      UpdateFill(currentTime / totalTime);
    }

    private void UpdateFill(float fill) {
      progressFade.fillAmount = fill;
    }

    private void ResetFill() {
      progressFade.fillAmount = 1;
    }

    private GameObject GetFadeGameObject() => blockFadeGameObject ?? blockFade.gameObject;

    private GameObject GetProgressGameObject() => progressFadeGameObject ?? progressFade.gameObject;

    private EventTrigger GetTrigger() => trigger ?? gameObject.GetComponent<EventTrigger>();
  }
}