using System.Collections.Generic;
using Inventory;
using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Repair {
  [CreateAssetMenu(menuName = "Repair System/Robot", fileName = "New robot repair station")]
  public class RobotRepairObject : BaseScriptableObject {
    public GameObject InterfacePrefab;
    public GameObject RobotPrefab;
    public InventoryType InventoryType;
    public List<ItemObject> RepairResources;
    public List<int> RepairResourcesAmount;
  }
}