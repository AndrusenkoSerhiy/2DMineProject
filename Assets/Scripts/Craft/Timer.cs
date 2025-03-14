using TMPro;
using UnityEngine;
using UnityServiceLocator;

namespace Craft {
  public class Timer : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI timerText;

    private Workstation station;

    private void Awake() {
      station = ServiceLocator.For(this).Get<Workstation>();
    }

    private void Update() {
      if (station.CurrentProgress.IsCrafting) {
        PrintTime();
      }
    }

    private void OnEnable() {
      station.OnInputAllCrafted += ClearTime;
    }

    private void OnDisable() {
      station.OnInputAllCrafted -= ClearTime;
    }

    private void ClearTime() {
      timerText.text = string.Empty;
    }

    private void PrintTime() {
      var roundedTimeLeft = Mathf.Round(station.CurrentProgress.MillisecondsLeft / 1000);
      timerText.text = Helper.SecondsToTimeString(roundedTimeLeft);
    }
  }
}