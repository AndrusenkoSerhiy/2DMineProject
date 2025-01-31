using Inventory;

namespace QuickSlots {
  public interface IUsableItem {
    void Use(InventorySlot slot);
  }
}