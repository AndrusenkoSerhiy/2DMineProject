using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class InputItem : MonoBehaviour {
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timeIcon;
    [SerializeField] private Image fade;
    [SerializeField] private Button cancel;

    private Workstation station;
    private Input input;
    private int position;

    private void InitWorkstation() {
      if (station != null) {
        return;
      }

      station = ServiceLocator.For(this).Get<Workstation>();
    }

    public void Init(Input inputData, int inputPosition) {
      InitWorkstation();

      input = inputData;
      position = inputPosition;

      SetupUI();
      PrintCount();
      PrintTime();

      cancel.onClick.AddListener(CancelHandler);
    }

    private void PrintTime() {
      var time = position == 0 && !station.CurrentProgress.Finished
        ? (station.CurrentProgress.MillisecondsLeft / 1000)
        : (input.Count * input.Recipe.CraftingTime);
      timerText.text = Helper.SecondsToTimeString(time);
    }

    private void CancelHandler() {
      GameManager.Instance.AudioController.PlayUIClick();
      station.CancelInput(input, position);
    }

    private void SetupUI() {
      icon.sprite = input.Recipe.Result.UiDisplay;
      icon.color = new Color(1, 1, 1, 255);
      timeIcon.gameObject.SetActive(true);
      fade.gameObject.SetActive(true);
      cancel.gameObject.SetActive(true);
    }

    private void PrintCount() {
      countText.text = input.Count.ToString();
    }

    public void ResetInput() {
      cancel.onClick.RemoveAllListeners();

      input = null;

      icon.sprite = null;
      icon.color = new Color(1, 1, 1, 0);
      timeIcon.gameObject.SetActive(false);
      fade.gameObject.SetActive(false);
      cancel.gameObject.SetActive(false);
      countText.text = string.Empty;
    }
  }
}