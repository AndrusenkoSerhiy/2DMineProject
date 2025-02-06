using Interaction;
using UnityEngine;
using Windows;

namespace Craft {
  public class Workbench : MonoBehaviour, IInteractable {
    [SerializeField] private string interactText;
    [SerializeField] private GameObject overlayPrefab;
    [SerializeField] private GameObject interfacePrefab;

    private CraftWindow craftWindow;
    public string InteractionPrompt => interactText;

    private void Init() {
      if (craftWindow != null) {
        return;
      }

      craftWindow = Instantiate(interfacePrefab, overlayPrefab.transform).GetComponent<CraftWindow>();
      GameManager.instance.WindowsController.AddWindow(craftWindow);
    }

    public bool Interact(Interactor interactor) {
      Init();

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