using System.Collections.Generic;
using Interaction;
using Scriptables;
using Scriptables.Items;
using UnityEngine;

namespace Inventory {
  public class RandomStorage : Storage {
    [SerializeField] private List<ResourceData.BonusResource> bonusResources;

    private Inventory storageInventory;
    private bool lootGenerated;

    public override bool Interact(PlayerInteractor playerInteractor) {
      TryGenerateRandomLoot();
      return base.Interact(playerInteractor);
    }

    private void TryGenerateRandomLoot() {
      if (lootGenerated || bonusResources == null || bonusResources.Count == 0) {
        return;
      }

      storageInventory ??= gameManager.PlayerInventory.GetInventoryByTypeAndId(inventoryType, GetId());

      foreach (var resource in bonusResources) {
        TryAddRandomResource(resource);
      }

      lootGenerated = true;
    }

    private void TryAddRandomResource(ResourceData.BonusResource resource) {
      if (!(Random.value <= resource.chance)) {
        return;
      }

      var count = Random.Range((int)resource.rndCount.x, (int)resource.rndCount.y);
      storageInventory.AddItem(new Item(resource.item), count);
    }

    protected override void OnAllBaseCellsDestroyed() {
      TryGenerateRandomLoot();
      base.OnAllBaseCellsDestroyed();
    }
  }
}