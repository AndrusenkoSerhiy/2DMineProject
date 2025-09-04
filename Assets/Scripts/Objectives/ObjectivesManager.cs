using System;
using System.Collections.Generic;
using Objectives.Data;
using Objectives.Handlers;
using SaveSystem;
using Scriptables.Objectives;
using UnityEngine;

namespace Objectives {
  public class ObjectivesManager {
    private readonly ObjectivesConfig config;
    private readonly ObjectiveProgressState state = new();
    private readonly Dictionary<ObjectiveTaskType, IObjectiveTaskHandler> handlers = new();

    private GameManager gameManager;
    private ObjectiveGroup currentGroupDisplay;

    // public event Action<ObjectiveData> OnTaskCompleted;
    // public event Action<ObjectiveGroup> OnGroupCompleted;
    public event Action<ObjectiveData> OnTaskRewarded;
    public event Action<ObjectiveGroup> OnGroupRewarded;
    public event Action<ObjectiveData, int> OnTaskProgress;

    public ObjectivesManager(ObjectivesConfig config) {
      this.config = config;
      gameManager = GameManager.Instance;
      RegisterDefaultHandlers();
    }

    public string GetConfigId() {
      return config.id;
    }

    public void SetCurrentGroupDisplay(ObjectiveGroup group) {
      currentGroupDisplay = group;
    }

    public void ClearCurrentGroupDisplay() {
      currentGroupDisplay = null;
    }

    private void RegisterDefaultHandlers() {
      RegisterHandler(new PickupItemTaskHandler());
      RegisterHandler(new CraftItemTaskHandler());
      RegisterHandler(new BuildTaskHandler());
      RegisterHandler(new RobotRepairTaskHandler());
      RegisterHandler(new ItemUseTaskHandler());
      RegisterHandler(new ItemRepairTaskHandler());
      RegisterHandler(new ItemEquipTaskHandler());
      RegisterHandler(new SurviveSiegeTaskHandler());
    }

    private void RegisterHandler(IObjectiveTaskHandler handler) {
      handlers[handler.Type] = handler;
    }

    public ObjectiveGroup GetCurrentGroup() {
      foreach (var group in config.groups) {
        if (!state.IsGroupRewarded(group.id)) {
          return group;
        }
      }

      return null;
    }

    public bool IsTaskCompleted(ObjectiveData data) {
      return state.IsTaskCompleted(data.id);
    }

    public bool IsTaskRewarded(ObjectiveData data) {
      return state.IsTaskRewarded(data.id);
    }

    public bool IsGroupCompleted(ObjectiveGroup group) {
      return state.IsGroupCompleted(group.id);
    }

    public bool IsGroupRewarded(ObjectiveGroup group) {
      return state.IsGroupRewarded(group.id);
    }

    public void Report(ObjectiveTaskType type, object context) {
      if (state.GetRewardedGroups().Count == config.groups.Count) {
        return;
      }

      if (!handlers.TryGetValue(type, out var handler)) {
        Debug.LogWarning($"No handler registered for type {type}");
        return;
      }

      //update progress without rewards
      foreach (var group in config.groups) {
        if (state.IsGroupCompleted(group.id)) {
          continue;
        }

        foreach (var obj in group.objectives) {
          if (state.IsTaskCompleted(obj.id)) {
            continue;
          }

          if (obj.taskData.TaskType != type) {
            continue;
          }

          var current = state.GetAccumulated(obj.id);
          if (handler.IsTaskSatisfied(obj.taskData, context, current, out var toAdd)) {
            state.MarkTaskCompleted(obj.id);
            // OnTaskCompleted?.Invoke(obj);
          }
          else if (toAdd > 0) {
            var newCurrent = current + toAdd;
            state.AddAmount(obj.id, toAdd);
            OnTaskProgress?.Invoke(obj, newCurrent);
          }
        }

        // check if group is completed but not marked as completed
        if (state.IsGroupCompleted(group.id) ||
            !group.objectives.TrueForAll(o => state.IsTaskCompleted(o.id))) {
          continue;
        }

        state.MarkGroupCompleted(group.id);
        // OnGroupCompleted?.Invoke(group);
      }

      CheckRewards();
    }

    public void CheckRewards() {
      // reward tasks and groups for current group
      var activeGroup = GetCurrentGroup();

      if (activeGroup == null || activeGroup != currentGroupDisplay || state.IsGroupRewarded(activeGroup.id)) {
        return;
      }

      // completed tasks that are not rewarded
      foreach (var obj in activeGroup.objectives) {
        if (!state.IsTaskCompleted(obj.id) || state.IsTaskRewarded(obj.id)) {
          continue;
        }

        state.MarkTaskRewarded(obj.id);
        OnTaskRewarded?.Invoke(obj);
      }

      // group rewards
      if (!state.IsGroupCompleted(activeGroup.id) || state.IsGroupRewarded(activeGroup.id)) {
        return;
      }

      state.MarkGroupRewarded(activeGroup.id);
      OnGroupRewarded?.Invoke(activeGroup);
    }

    public void MarkTaskRewarded(ObjectiveData task) {
      state.MarkTaskRewarded(task.id);
    }

    public void MarkGroupRewarded(ObjectiveGroup group) {
      state.MarkGroupRewarded(group.id);
    }

    public int GetTargetAmount(ObjectiveTaskData data) {
      return data switch {
        BuildTaskData b => b.amount,
        CraftItemTaskData c => c.amount,
        ItemEquipTaskData e => e.amount,
        ItemRepairTaskData r => r.amount,
        ItemUseTaskData u => u.amount,
        PickupItemTaskData p => p.amount,
        _ => 1
      };
    }

    public int GetAccumulatedAmount(string objectiveId) {
      return state.GetAccumulated(objectiveId);
    }

    public ObjectivesData GetSaveData() {
      return new ObjectivesData {
        Progress = state.GetAllProgress(),
        CompletedTasks = new List<string>(state.GetCompletedTasks()),
        CompletedGroups = new List<string>(state.GetCompletedGroups()),
        RewardedTasks = new List<string>(state.GetRewardedTasks()),
        RewardedGroups = new List<string>(state.GetRewardedGroups())
      };
    }

    public void LoadData(ObjectivesData data) {
      state.SetProgress(data.Progress);
      state.SetCompletedTasks(data.CompletedTasks);
      state.SetRewardedTasks(data.RewardedTasks);
      state.SetCompletedGroups(data.CompletedGroups);
      state.SetRewardedGroups(data.RewardedGroups);
    }
  }
}