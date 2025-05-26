using Inventory;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/BuildingBlock", fileName = "BuildingBlock")]
  public class BuildingBlock : ItemObject, IConsumableItem {
    public AudioData consumeSound;
    public ResourceData ResourceData;
    public Building BuildingData;
    public AudioData ConsumeSound => consumeSound;
  }
}