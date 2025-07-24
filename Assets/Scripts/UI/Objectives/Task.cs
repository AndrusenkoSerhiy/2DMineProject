using Objectives.Data;
using Scriptables.Objectives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Objectives {
  public class Task : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Image progressIcon;

    private ObjectivesConfig config;
    private ObjectiveData objective;
    private int current;
    private int target;

    public void Init(ObjectiveData obj, ObjectivesConfig settings) {
      config = settings;
      objective = obj;
      current = 0;
      target = 1;
      UpdateText();
    }

    public void SetProgress(int currentAmount, int targetAmount) {
      current = Mathf.Clamp(currentAmount, 0, targetAmount);
      target = targetAmount;
      UpdateText();
    }

    public void UpdateProgress(int newAmount) {
      current = newAmount;
      UpdateText();
    }

    private void UpdateText() {
      var progressText = target > 1 ? $" ({current}/{target})" : "";
      text.text = objective.title + progressText;
    }

    public void ShowCompleted() {
      UpdateProgress(target);

      gameObject.SetActive(true);
      text.color = config.taskColorCompleted;

      if (progressIcon && config.taskIconCompleted) {
        progressIcon.gameObject.SetActive(true);
        progressIcon.sprite = config.taskIconCompleted;
        progressIcon.color = config.taskColorCompleted;
      }
      else {
        progressIcon.gameObject.SetActive(false);
      }
    }

    public void ShowInProgress() {
      gameObject.SetActive(true);
      text.color = config.taskColorIncomplete;

      if (progressIcon && config.taskIconIncomplete) {
        progressIcon.gameObject.SetActive(true);
        progressIcon.sprite = config.taskIconIncomplete;
        progressIcon.color = config.taskColorIncomplete;
      }
      else {
        progressIcon.gameObject.SetActive(false);
      }
    }

    public void Hide() {
      gameObject.SetActive(false);
    }
  }
}