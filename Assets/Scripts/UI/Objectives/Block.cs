using System.Collections.Generic;
using Objectives;
using Objectives.Data;
using SaveSystem;
using Scriptables.Objectives;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Objectives {
  public class Block : MonoBehaviour {
    [SerializeField] private ObjectivesConfig config;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private Image icon;
    [SerializeField] private List<Task> tasks;

    private Dictionary<string, Task> taskMap = new();
    private ObjectivesManager manager;

    private bool isInitialized;
    private GameManager gameManager;

    private void Awake() {
      gameManager = GameManager.Instance;
    }

    public string GetConfigId() {
      return config.id;
    }

    public ObjectivesManager Init(ObjectivesData data = null) {
      manager = new ObjectivesManager(config);

      if (data != null) {
        manager.LoadData(data);
      }

      manager.OnTaskCompleted += OnTaskCompletedHandler;
      manager.OnGroupCompleted += OnGroupCompletedHandler;
      manager.OnTaskProgress += OnTaskProgressHandler;
      manager.OnTaskRewarded += OnTaskRewardedHandler;
      manager.OnGroupRewarded += OnGroupRewardedHandler;

      ShowGroup();
      isInitialized = true;
      return manager;
    }

    private void OnTaskRewardedHandler(ObjectiveData data) {
      gameManager.PlayerInventory.AddItemToInventory(data.reward.item, data.reward.amount, transform.position);
      gameManager.MessagesManager.ShowSimpleMessage("You got " + data.reward.amount + " " + data.reward.item.name +
                                                    " for completing task");
    }

    private void OnGroupRewardedHandler(ObjectiveGroup data) {
      gameManager.PlayerInventory.AddItemToInventory(data.reward.item, data.reward.amount, transform.position);
      gameManager.MessagesManager.ShowSimpleMessage("You got " + data.reward.amount + " " + data.reward.item.name +
                                                    " for completing group");
    }

    private void OnTaskProgressHandler(ObjectiveData data, int current) {
      if (taskMap.TryGetValue(data.id, out var taskUI)) {
        taskUI.UpdateProgress(current);
      }
    }

    private void OnTaskCompletedHandler(ObjectiveData task) {
      if (taskMap.TryGetValue(task.id, out var taskUI)) {
        taskUI.ShowCompleted();
      }
    }

    private void OnGroupCompletedHandler(ObjectiveGroup group) {
      HideCurrentGroup();
      ShowGroup();
    }

    private void ShowGroup() {
      var currentGroup = manager.GetCurrentGroup();
      if (currentGroup == null) {
        Hide();
        return;
      }

      title.text = currentGroup.groupTitle;
      title.color = config.titleColor;

      if (config.titleIcon) {
        icon.gameObject.SetActive(true);
        icon.sprite = config.titleIcon;
        icon.color = config.titleColor;
      }
      else {
        icon.gameObject.SetActive(false);
      }

      for (var i = 0; i < currentGroup.objectives.Count; i++) {
        var objective = currentGroup.objectives[i];
        var task = tasks[i];
        task.Init(objective, config);
        taskMap[objective.id] = task;

        if (manager.IsTaskCompleted(objective)) {
          task.ShowCompleted();
        }
        else {
          task.ShowInProgress();
          var current = manager.GetAccumulatedAmount(objective.id);
          var target = manager.GetTargetAmount(objective.taskData);
          task.SetProgress(current, target);
        }
      }

      Show();
    }

    private void HideCurrentGroup() {
      title.text = "";

      foreach (var task in taskMap.Values) {
        task.Hide();
      }

      taskMap.Clear();
      Hide();
    }

    private void Show() {
      gameObject.SetActive(true);
    }

    private void Hide() {
      gameObject.SetActive(false);
    }
  }
}