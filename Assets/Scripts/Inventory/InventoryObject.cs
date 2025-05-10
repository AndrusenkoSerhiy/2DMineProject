using SaveSystem;
using Scriptables.Items;
using World;

namespace Inventory {
  public class InventoryObject {
    private bool loaded;
    private string id;
    private ItemDatabaseObject database;
    private InventoryType type;
    private InventoryContainer container;

    public ItemDatabaseObject Database => database;
    public InventoryType Type => type;
    public string Id => id;
    public InventorySlot[] Slots => container.Slots;

    public static string GenerateId(InventoryType type, string entityId) {
      if (type is InventoryType.Inventory
          or InventoryType.QuickSlots
          or InventoryType.Equipment) {
        entityId = "";
      }

      return $"{type.ToString()}_{entityId}".ToLower();
    }

    public static string GenerateEntityIdByCell(BuildingDataObject buildObject) {
      return $"{buildObject.transform.position.x}_{buildObject.transform.position.y}";
    }

    public InventoryObject(InventoryType inventoryType, string inventoryId) {
      id = inventoryId;
      type = inventoryType;
      var db = GameManager.Instance.ItemDatabaseObject;
      database = db;
      var size = GameManager.Instance.PlayerInventory.GetInventorySizeByType(type);
      container = new InventoryContainer(size, inventoryType, inventoryId);
    }

    public void SaveToGameData() {
      SaveLoadSystem.Instance.gameData.Inventories[Id] = new InventoryData {
        Id = Id,
        Slots = Slots
      };
    }

    public void LoadFromGameData() {
      if (SaveLoadSystem.Instance.IsNewGame()) {
        return;
      }
      
      if (!SaveLoadSystem.Instance.gameData.Inventories.TryGetValue(Id, out var data)) {
        return;
      }

      var isNew = data.Slots == null || data.Slots.Length == 0;
      if (isNew) {
        return;
      }

      Load(data.Slots);
    }

    private void Load(InventorySlot[] slots) {
      if (loaded) {
        return;
      }

      for (var i = 0; i < slots.Length; i++) {
        var slotData = slots[i];
        Slots[i].isSelected = slotData.isSelected;
        if (slotData.Item.id == string.Empty) {
          continue;
        }

        slotData.Item.RestoreItemObject(database.ItemObjects);

        Slots[i].UpdateSlotBySlot(slotData);
      }

      loaded = true;
    }
  }
}