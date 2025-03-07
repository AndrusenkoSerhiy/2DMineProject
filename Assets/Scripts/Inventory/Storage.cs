using Windows;
using Interaction;
using UnityEngine;

namespace Inventory {
  public class Storage : MonoBehaviour, IInteractable {
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] private string interactText;
    [SerializeField] private string interactHeader;
    [SerializeField] private StorageType storageType;
    public string InteractionText => interactText;
    public string InteractionHeader => interactHeader;

    private StorageWindow storageWindow;

    //TODO, we can generate id from cell position
    private string id = "case_0";

    public bool Interact(PlayerInteractor playerInteractor) {
      Init();

      if (storageWindow.IsShow) {
        storageWindow.Hide();
      }
      else {
        storageWindow.Show();
      }

      return true;
    }

    private void Init() {
      if (storageWindow != null) {
        return;
      }

      var caseInventory = new InventoryObject(InventoryType.Storage, id, storageType);
      GameManager.Instance.PlayerInventory.SetStorage(caseInventory);
      var storageWindowObj = Instantiate(interfacePrefab, GameManager.Instance.Canvas.transform);

      storageWindowObj.transform.SetSiblingIndex(0);

      storageWindow = storageWindowObj.GetComponent<StorageWindow>();
      GameManager.Instance.WindowsController.AddWindow(storageWindow);
      storageWindow.StorageUI.Setup(id);
      storageWindow.InventoryUI.SetupFastDrop(id);
    }
  }
}