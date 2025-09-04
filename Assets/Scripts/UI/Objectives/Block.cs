using System.Collections;
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
    [SerializeField] private float completedGroupShowDelay = 7f;

    private readonly Dictionary<string, Task> taskMap = new();
    private ObjectivesManager manager;
    private GameManager gameManager;
    private Coroutine showCompletedGroupCoroutine;
    private bool isInitialized;

    private void Awake() {
      gameManager = GameManager.Instance;
    }

    public string GetConfigId() => config.id;

    public ObjectivesManager Init(ObjectivesData data = null) {
      manager = new ObjectivesManager(config);

      if (data != null) {
        manager.LoadData(data);
      }

      manager.OnTaskProgress += OnTaskProgressHandler;
      manager.OnTaskRewarded += OnTaskRewardedHandler;
      manager.OnGroupRewarded += OnGroupRewardedHandler;

      ShowGroup();
      isInitialized = true;
      return manager;
    }

    private void OnTaskRewardedHandler(ObjectiveData data) {
      if (taskMap.TryGetValue(data.id, out var taskUI)) {
        taskUI.ShowCompleted();
        gameManager.MessagesManager.ShowSimpleMessage("Task completed!");
      }

      GrantReward(data.reward, "for completing task");
    }

    private void OnGroupRewardedHandler(ObjectiveGroup data) {
      GrantReward(data.reward, "for completing group");
      gameManager.MessagesManager.ShowSimpleMessage("Quest completed!");
      DelayShowNextGroup();
    }

    private void OnTaskProgressHandler(ObjectiveData data, int current) {
      if (taskMap.TryGetValue(data.id, out var taskUI)) {
        taskUI.UpdateProgress(current);
      }
    }

    private void ShowGroup() {
      var currentGroup = manager.GetCurrentGroup();
      if (currentGroup == null) {
        Hide();
        return;
      }

      manager.SetCurrentGroupDisplay(currentGroup);
      SetupGroupHeader(currentGroup);
      SetupTasks(currentGroup);

      Show();

      manager.CheckRewards();
    }

    private void SetupGroupHeader(ObjectiveGroup currentGroup) {
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
    }

    private void SetupTasks(ObjectiveGroup currentGroup) {
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
    }

    private void GrantReward(ObjectiveRewardData reward, string messageSuffix) {
      if (reward?.item == null) {
        return;
      }

      var pos = gameManager.PlayerController.transform.position + new Vector3(5, 3, 0);
      gameManager.PlayerInventory.AddItemToInventory(reward.item, reward.amount, pos);
      gameManager.MessagesManager.ShowSimpleMessage(
        $"You got {reward.amount} {reward.item.name} {messageSuffix}"
      );
    }

    private void DelayShowNextGroup() {
      if (showCompletedGroupCoroutine != null) {
        StopCoroutine(showCompletedGroupCoroutine);
      }

      showCompletedGroupCoroutine = StartCoroutine(ShowNextGroupAfterSeconds(completedGroupShowDelay));
    }

    private IEnumerator ShowNextGroupAfterSeconds(float seconds) {
      yield return new WaitForSeconds(seconds);
      SwitchToNextGroup();
    }

    private void SwitchToNextGroup() {
      HideCurrentGroup();
      ShowGroup();
    }

    private void HideCurrentGroup() {
      manager.ClearCurrentGroupDisplay();
      title.text = "";
      foreach (var task in taskMap.Values) {
        task.Hide();
      }

      taskMap.Clear();
      Hide();
    }

    private void Show() => gameObject.SetActive(true);
    private void Hide() => gameObject.SetActive(false);
  }
}