using Interaction;
using UnityEngine;
using Windows;

namespace Craft {
  public class Workbench : MonoBehaviour, IInteractable {
    [SerializeField] private string interactText;
    private WindowsController windowsController;
    private CraftWindow craftWindow;

    public string InteractionPrompt => interactText;

    private void Start() {
      windowsController = GameManager.instance.WindowsController;
      craftWindow = windowsController.GetWindow<CraftWindow>();
    }

    public bool Interact(Interactor interactor) {
      if (craftWindow.IsShow) {
        craftWindow.Hide();
      }
      else {
        craftWindow.Show();
      }

      return true;
    }
  }
}