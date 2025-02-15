using Scriptables.Items;

namespace Inventory {
  public interface IPlayerInventory {
    void AddItemToInventory(ItemObject item, int count);
  }
}