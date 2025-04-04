using UnityEngine;

namespace Scriptables.Items {
  [CreateAssetMenu(menuName = "Inventory System/Items/BuildingBlock", fileName = "BuildingBlock")]
  public class BuildingBlock : ItemObject, IConsumableItem {
    public ResourceData ResourceData;

    /*public override void Use() {
      //Debug.LogError($"use {ResourceData.ItemData.name}");
      var prefab = CharacterDisplay ? CharacterDisplay : spawnPrefab;
      GameManager.Instance.PlaceCell.ActivateBuildMode(ResourceData, prefab);
    }*/
  }
}