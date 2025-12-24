using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Items {
  [System.Serializable]
  public class SeedHarvest {
    public ItemObject Harvest;
    public int MinCount;
    public int MaxCount;
  }

  [CreateAssetMenu(menuName = "Inventory System/Items/Seeds", fileName = "Seed")]
  public class Seeds : ItemObject {
    public int TimeToGrowthMin;
    public int TimeToGrowthMax;
    public List<SeedHarvest> HarvestList;
    public ItemObject ItemToHarvest;
    public List<Sprite> GrownSprites;
  }
}