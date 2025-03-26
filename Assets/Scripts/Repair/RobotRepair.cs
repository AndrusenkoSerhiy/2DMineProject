using Scriptables.Repair;

namespace Repair {
  public class RobotRepair {
    private string id;
    private RobotObject robotObject;
    private Inventory.Inventory resourcesInventory;
    private GameManager gameManager;

    public string Id => id;
    public RobotObject RobotObject => robotObject;
    public Inventory.Inventory ResourcesInventory => resourcesInventory;

    public RobotRepair(RobotObject settings) {
      gameManager = GameManager.Instance;
      id = settings.Id;
      robotObject = settings;
      resourcesInventory = gameManager.PlayerInventory.GetInventoryByTypeAndId(robotObject.InventoryType, id);
    }
  }
}