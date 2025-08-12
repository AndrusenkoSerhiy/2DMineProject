using System.Collections.Generic;
using Interaction;
using Scriptables;
using Scriptables.Items;
using UnityEngine;

namespace Inventory {
  public class RandomStorage : Storage {
    private Inventory storageInventory;
    [SerializeField] private List<ResourceData.BonusResource> bonusResources;
    public override bool Interact(PlayerInteractor playerInteractor) {
      storageInventory = gameManager.PlayerInventory.GetInventoryByTypeAndId(inventoryType, GetId());
      if (storageInventory.IsEmpty()) {
        GenerateRandomLoot();
      }
      return base.Interact(playerInteractor);
    }

    private void GenerateRandomLoot() {
      for (var i = 0; i < bonusResources.Count; i++) {
        var currentResource = bonusResources[i];
        var rand = Random.value;
        if (rand > currentResource.chance) {
          continue;
        }

        var count = Random.Range((int)currentResource.rndCount.x, (int)currentResource.rndCount.y);
        storageInventory.AddItem(new Item(currentResource.item), count);
      }
    }
  }
}