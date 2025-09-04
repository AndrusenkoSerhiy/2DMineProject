using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Objectives {
  public class ObjectiveProgressState {
    private readonly Dictionary<string, int> taskProgress = new();
    private readonly HashSet<string> completedTasks = new();
    private readonly HashSet<string> rewardedTasks = new();
    private readonly HashSet<string> completedGroups = new();
    private readonly HashSet<string> rewardedGroups = new();

    public bool IsTaskCompleted(string id) => completedTasks.Contains(id);
    public bool IsTaskRewarded(string id) => rewardedTasks.Contains(id);
    public void MarkTaskCompleted(string id) => completedTasks.Add(id);
    public void MarkTaskRewarded(string id) => rewardedTasks.Add(id);

    public bool IsGroupRewarded(string id) => rewardedGroups.Contains(id);
    public bool IsGroupCompleted(string id) => completedGroups.Contains(id);
    public void MarkGroupRewarded(string id) => rewardedGroups.Add(id);
    public void MarkGroupCompleted(string id) => completedGroups.Add(id);

    public int GetAccumulated(string id) => taskProgress.TryGetValue(id, out var value) ? value : 0;
    public void AddAmount(string id, int value) => taskProgress[id] = GetAccumulated(id) + value;

    public SerializedDictionary<string, int> GetAllProgress() {
      var dict = new SerializedDictionary<string, int>();
      foreach (var pair in taskProgress) {
        dict[pair.Key] = pair.Value;
      }

      return dict;
    }

    public HashSet<string> GetCompletedTasks() => completedTasks;
    public HashSet<string> GetRewardedTasks() => rewardedTasks;
    public HashSet<string> GetCompletedGroups() => completedGroups;
    public HashSet<string> GetRewardedGroups() => rewardedGroups;

    public void SetProgress(Dictionary<string, int> newProgress) {
      taskProgress.Clear();
      foreach (var pair in newProgress) {
        taskProgress[pair.Key] = pair.Value;
      }
    }

    public void SetCompletedTasks(List<string> completedIds) {
      completedTasks.Clear();
      foreach (var id in completedIds) {
        completedTasks.Add(id);
      }
    }

    public void SetRewardedTasks(List<string> rewardedIds) {
      rewardedTasks.Clear();
      foreach (var id in rewardedIds) {
        rewardedTasks.Add(id);
      }
    }

    public void SetCompletedGroups(List<string> completedIds) {
      completedGroups.Clear();
      foreach (var id in completedIds) {
        completedGroups.Add(id);
      }
    }

    public void SetRewardedGroups(List<string> rewardedIds) {
      rewardedGroups.Clear();
      foreach (var id in rewardedIds) {
        rewardedGroups.Add(id);
      }
    }
  }
}