using Inventory;
using Scriptables.Repair;
using World;

namespace Repair {
  public class RobotRepair {
    private string id;
    private RobotRepairObject robotRepairObject;
    private InventoryObject resourcesInventory;
    private GameManager gameManager;

    public string Id => id;
    public RobotRepairObject RobotRepairObject => robotRepairObject;
    public InventoryObject ResourcesInventory => resourcesInventory;

    public static string GenerateId(CellObject cellObject, RobotRepairObject repairSettings) {
      return $"{repairSettings.Id}_{cellObject.CellData.x}_{cellObject.CellData.y}".ToLower();
    }

    public RobotRepair(CellObject cellObject, RobotRepairObject repairSettings) {
      gameManager = GameManager.Instance;
      id = GenerateId(cellObject, repairSettings);
      robotRepairObject = repairSettings;
      resourcesInventory = gameManager.PlayerInventory.GetInventoryByTypeAndId(robotRepairObject.InventoryType, id);
    }
  }
}