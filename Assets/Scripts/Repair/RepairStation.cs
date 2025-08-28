using Windows;
using Interaction;
using Scriptables.Repair;
using UnityEngine;
using UnityEngine.Serialization;
using World;

namespace Repair {
  public class RepairStation : MonoBehaviour, IInteractable {
    [SerializeField] private RobotRepairObject robotObject;
    [SerializeField] private CellObject cellObject;
    [SerializeField] private string interactText;
    [SerializeField] private string holdInteractText;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private WindowBase window;
    private RepairWindow repairWindow;

    public string InteractionText => interactText;
    public bool HasHoldInteraction { get; }
    public string HoldInteractionText => holdInteractText;

    public Bounds GetBounds() {
      return spriteRenderer ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.zero);
    }

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

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      return false;
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