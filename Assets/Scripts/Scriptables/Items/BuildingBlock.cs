using Inventory;
using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/BuildingBlock", fileName = "BuildingBlock")]
  public class BuildingBlock : ItemObject {
    public ResourceData ResourceData;
    public override void Use(InventorySlot slot) {
      //Debug.LogError($"use {ResourceData.ItemData.name}");
      GameManager.Instance.PlaceCell.ActivateBuildMode(slot, ResourceData);
      //GameManager.instance.PlayerEquipment.OnEquipItem();
    }
  }
}