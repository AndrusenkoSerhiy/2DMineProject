using Windows;
using Interaction;
using Scriptables.Repair;
using UnityEngine;
using UnityEngine.Serialization;
using World;

namespace Repair {
  public class RepairStation : MonoBehaviour, IInteractable {
    [FormerlySerializedAs("robotRepairObject")] [SerializeField] private RobotObject robotObject;
    [SerializeField] private CellObject cellObject;
    [SerializeField] private string interactText;
    [SerializeField] private string interactHeader;

    private WindowBase window;
    private RepairWindow repairWindow;

    public string InteractionText => interactText;
    public string InteractionHeader => interactHeader;

    public bool Interact(PlayerInteractor playerInteractor) {
      Init();

      /*if (repairWindow.Repaired) {
        GameManager.Instance.MessagesManager.ShowSimpleMessage("This robot is already repaired");
        return false;
      }*/

      if (window.IsShow) {
        window.Hide();
      }
      else {
        window.Show();
      }

      return true;
    }

    private void Init() {
      if (window != null) {
        return;
      }

      var windowObj = Instantiate(robotObject.InterfacePrefab, GameManager.Instance.Canvas.transform);
      windowObj.transform.SetSiblingIndex(0);

      repairWindow = windowObj.GetComponent<RepairWindow>();
      // repairWindow.Setup(cellObject, robotRepairObject);

      window = windowObj.GetComponent<WindowBase>();
      GameManager.Instance.WindowsController.AddWindow(window);
    }
  }
}