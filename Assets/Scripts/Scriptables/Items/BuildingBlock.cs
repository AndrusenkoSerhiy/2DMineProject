using Inventory;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/BuildingBlock", fileName = "BuildingBlock")]
  public class BuildingBlock : ItemObject {
    public override void Use(InventorySlot slot) {
      //Debug.LogError("Build");
      GameManager.instance.PlaceCell.ActivateBuildMode(slot);
      //GameManager.instance.PlayerEquipment.OnEquipItem();
    }
  }
}