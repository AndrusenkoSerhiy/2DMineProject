using System;
using System.Collections.Generic;
using System.Linq;
using Scriptables.Items;
using Scriptables.Siege;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scriptables.DropZombieData {
  [CreateAssetMenu(fileName = "DropZombieData", menuName = "DropZombieData")]

  public class DropZombieData : ScriptableObject {
    public List<DropData> drops = new();

    public void DropItems(int stage) {
      var data = GetDropData(stage);
      var items = data.possibleItems;
      var maxItems = data.maxItems;
      var currItemsCount = 0;
      
      foreach (var item in items) {
        var rand = Random.value;
        if (currItemsCount >= maxItems)
          break;
        
        if (rand > item.chance) {
          continue;
        }

        var minCount = (int)item.rndCount.x;
        var maxCount = (int)item.rndCount.y + 1;
        var count = Random.Range(minCount, maxCount);
        count = Mathf.Min(count, maxItems - currItemsCount);
        
        currItemsCount += count;
        GameManager.Instance.PlayerInventory.SpawnItem(new Item(item.item), count);
      }
    }

    private DropData GetDropData(int stage) {
      return drops.FirstOrDefault(e => e.stageRange.x <= stage && e.stageRange.y >= stage);
    }
  }
  
  [Serializable]
  public class DropData {
    //TODO: use difficulty profile instead of stageRange SiegeManager.GetDifficultyProfilesByWeight()
    public ZombieDifficultyProfile difficultyProfile;
    public Vector2 stageRange;
    public int maxItems;
    public List<DropResource> possibleItems;
  }
  
  [Serializable]
  public struct DropResource {
    public ItemObject item;
    [Range(0, 1)] public float chance;
    public Vector2 rndCount;
  }
}