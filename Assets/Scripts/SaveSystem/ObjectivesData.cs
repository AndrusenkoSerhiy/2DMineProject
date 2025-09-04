using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace SaveSystem {
  [Serializable]
  public class ObjectivesData {
    public SerializedDictionary<string, int> Progress = new();
    public List<string> CompletedTasks = new();
    public List<string> CompletedGroups = new();
    public List<string> RewardedTasks = new();
    public List<string> RewardedGroups = new();
  }
}