using UnityEngine;

namespace Analytics {
  [CreateAssetMenu(menuName = "Analytics/Events")]
  public class AnalyticsEvent : ScriptableObject {
    public string PlayerDied = "Player Died";
    public string RobotRepaired = "Robot Repaired";
    public string ItemCrafted = "Item Crafted";
    public string StationPlaced = "Station Placed";
  }
}