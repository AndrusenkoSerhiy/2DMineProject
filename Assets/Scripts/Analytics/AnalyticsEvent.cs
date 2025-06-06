using UnityEngine;

namespace Analytics {
  [CreateAssetMenu(menuName = "Analytics/Events")]
  public class AnalyticsEvent : ScriptableObject {
    public string PlayerDied = "Player Died";
    public string RobotRepaired = "Robot Repaired";
    public string StationPlaced = "Station Placed";
    public string StationRemoved = "Station Removed";
    public string UserInfo = "User Info";
    public string ContinueGame = "Continue Game";
    public string NewGame = "New Game";
    public string LogError = "log_error";
    public string UnobservedTaskException = "unobserved_task_exception";
    public string UnhandledException = "unhandled_exception";
  }
}