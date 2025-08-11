using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Objectives {
  public class ObjectiveProgressState {
    private readonly Dictionary<string, int> progress = new();
    private readonly HashSet<string> completed = new();

    public bool IsCompleted(string id) => completed.Contains(id);
    public void MarkCompleted(string id) => completed.Add(id);
    public int GetAccumulated(string id) => progress.TryGetValue(id, out var value) ? value : 0;
    public void AddAmount(string id, int value) => progress[id] = GetAccumulated(id) + value;

    public SerializedDictionary<string, int> GetAllProgress() {
      var dict = new SerializedDictionary<string, int>();
      foreach (var pair in progress) {
        dict[pair.Key] = pair.Value;
      }

      return dict;
    }

    public List<string> GetCompletedTasks() => new(completed);

    public void SetProgress(Dictionary<string, int> newProgress) {
      progress.Clear();
      foreach (var pair in newProgress) {
        progress[pair.Key] = pair.Value;
      }
    }

    public void SetCompletedTasks(List<string> completedIds) {
      completed.Clear();
      foreach (var id in completedIds) {
        completed.Add(id);
      }
    }
  }
}