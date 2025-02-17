namespace Inventory {
  public class SlotSwappedEventData {
    public InventorySlot slot { get; private set; }
    public InventorySlot target { get; private set; }

    public SlotSwappedEventData(InventorySlot slot, InventorySlot target) {
      this.slot = slot;
      this.target = target;
    }
  }
}