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
    private readonly HashSet<string> completedGroups = new();
    private GameManager gameManager;

    public event Action<ObjectiveData> OnTaskCompleted;
    public event Action<ObjectiveData> OnTaskRewarded;
    public event Action<ObjectiveGroup> OnGroupRewarded;
    public event Action<ObjectiveData, int> OnTaskProgress;
    public event Action<ObjectiveGroup> OnGroupCompleted;

    public ObjectivesManager(ObjectivesConfig config) {
      this.config = config;
      gameManager = GameManager.Instance;
      RegisterDefaultHandlers();
    }

    public string GetConfigId() {
      return config.id;
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
        if (!IsGroupCompleted(group)) {
          return group;
        }
      }

      return null;
    }

    private bool IsGroupCompleted(ObjectiveGroup group) {
      return completedGroups.Contains(group.id);
    }

    public bool IsTaskCompleted(ObjectiveData data) {
      return state.IsCompleted(data.id);
    }

    public void Report(ObjectiveTaskType type, object context) {
      if (!handlers.TryGetValue(type, out var handler)) {
        Debug.LogWarning($"No handler registered for type {type}");
        return;
      }

      var group = GetCurrentGroup();
      if (group == null) {
        return;
      }

      var groupNowCompleted = true;

      foreach (var obj in group.objectives) {
        if (state.IsCompleted(obj.id)) {
          continue;
        }

        if (obj.taskData.TaskType != type) {
          groupNowCompleted = false;
          continue;
        }

        var current = state.GetAccumulated(obj.id);

        if (handler.IsTaskSatisfied(obj.taskData, context, current, out int toAdd)) {
          state.MarkCompleted(obj.id);
          OnTaskCompleted?.Invoke(obj);

          gameManager.MessagesManager.ShowSimpleMessage("You completed a task");

          if (obj.reward?.item != null) {
            OnTaskRewarded?.Invoke(obj);
          }

          // Debug.Log($"✅ Objective completed: {obj.title}");
        }
        else {
          if (toAdd > 0) {
            var newCurrent = current + toAdd;
            state.AddAmount(obj.id, toAdd);
            OnTaskProgress?.Invoke(obj, newCurrent);
            // Debug.Log($"Progress: {newCurrent}/{GetTargetAmount(obj.taskData)} for {obj.title}");
          }

          groupNowCompleted = false;
        }
      }

      if (!groupNowCompleted || completedGroups.Contains(group.id)) {
        return;
      }

      completedGroups.Add(group.id);
      OnGroupCompleted?.Invoke(group);

      gameManager.MessagesManager.ShowSimpleMessage("You completed a group");

      if (group.reward?.item != null) {
        OnGroupRewarded?.Invoke(group);
      }

      // Debug.Log($"🏁 Objective Group completed: {group.groupTitle}");
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
        CompletedTasks = state.GetCompletedTasks(),
        CompletedGroups = new List<string>(completedGroups)
      };
    }

    public void LoadData(ObjectivesData data) {
      state.SetProgress(data.Progress);
      state.SetCompletedTasks(data.CompletedTasks);

      completedGroups.Clear();
      foreach (var groupId in data.CompletedGroups) {
        completedGroups.Add(groupId);
      }
    }
  }
}