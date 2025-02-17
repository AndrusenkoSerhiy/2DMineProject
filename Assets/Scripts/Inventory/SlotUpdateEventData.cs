namespace Inventory {
  public class SlotUpdateEventData {
    public InventorySlot before { get; private set; }
    public InventorySlot after { get; private set; }
    public InventorySlot from { get; private set; }

    public SlotUpdateEventData(InventorySlot before, InventorySlot after, InventorySlot from) {
      this.before = before;
      this.after = after;
      this.from = from;
    }
  }
}