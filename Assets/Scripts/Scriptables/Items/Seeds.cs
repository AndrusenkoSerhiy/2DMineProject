using System.Collections.Generic;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/Seeds", fileName = "Seed")]
  public class Seeds : ItemObject {
    public int TimeToGrowth;
    public ItemObject Harvest;
    public ItemObject ItemToHarvest;
    public List<Sprite> GrownSprites;
  }
}