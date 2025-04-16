using Scriptables.Items;
using UnityEngine;

namespace Scriptables.Repair {
  [CreateAssetMenu(menuName = "Robots", fileName = "New robot")]
  public class RobotObject : BaseScriptableObject, IRepairable {
    [Tooltip("Count of repair kits")]
    [SerializeField] private int repairCost;
    public int RepairCost => repairCost;
  }
}