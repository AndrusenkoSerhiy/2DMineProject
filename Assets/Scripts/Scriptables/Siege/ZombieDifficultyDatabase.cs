using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Siege {
  [CreateAssetMenu(fileName = "ZombieDifficultyDatabase", menuName = "Siege/Zombie Difficulty Database", order = 1)]
  public class ZombieDifficultyDatabase : ScriptableObject {
    [Serializable]
    public class DifficultyProfile {
      public ZombieDifficultyProfile profile;
      public float percentage;
    }

    [Serializable]
    public class DifficultyEntry {
      public float minWeight;
      public float maxWeight;
      public List<DifficultyProfile> profiles;
    }

    public List<DifficultyEntry> difficultyEntries = new();

    public DifficultyEntry GetProfileByWeight(float totalWeight) {
      foreach (var entry in difficultyEntries) {
        if (totalWeight >= entry.minWeight && totalWeight < entry.maxWeight) {
          return entry;
        }
      }

      return difficultyEntries.Count > 0 ? difficultyEntries[^1] : null;
    }
  }
}