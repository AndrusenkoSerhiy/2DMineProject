using Scriptables.Items;

namespace Inventory {
  public interface IPlayerInventory {
    int GetResourceTotalAmount(int resourceId);
    void AddItemToInventory(ItemObject item, int count);
    void SpawnItem(InventorySlot slot);
  }
}