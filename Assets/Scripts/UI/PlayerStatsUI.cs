using Tools;
using UnityEngine;

namespace UI {
  public class PlayerStatsUI : MonoBehaviour {
    [SerializeField] private FoodModifiersUI foodModifiersUI;
    [SerializeField] private StatModifiersUI statModifiersUI;

    private void Awake() {
      MiningRobotTool.OnPlayerSitOnRobot += HidePlayerStats;
      MiningRobotTool.OnPlayerExitFromRobot += ShowPlayerStats;
    }

    private void ShowPlayerStats() {
      foodModifiersUI.gameObject.SetActive(true);
      statModifiersUI.UpdateController();
    }

    private void HidePlayerStats() {
      foodModifiersUI.gameObject.SetActive(false);
      statModifiersUI.UpdateController();
    }
  }
}