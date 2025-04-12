using Windows;
using Interaction;
using UnityEngine;
using World;

namespace Inventory {
  public class Storage : MonoBehaviour, IInteractable {
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] private string interactText;
    [SerializeField] private string interactHeader;
    [SerializeField] private InventoryType inventoryType;
    [SerializeField] private BuildingDataObject buildObject;
    public string InteractionText => interactText;
    public string InteractionHeader => interactHeader;

    private StorageWindow storageWindow;

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

      var entityId = InventoryObject.GenerateEntityIdByCell(buildObject);
      var id = InventoryObject.GenerateId(inventoryType, entityId);
      var storageWindowObj = Instantiate(interfacePrefab, GameManager.Instance.Canvas.transform);

      storageWindowObj.transform.SetSiblingIndex(0);
      storageWindow = storageWindowObj.GetComponent<StorageWindow>();
      GameManager.Instance.WindowsController.AddWindow(storageWindow);
      storageWindow.StorageUI.Setup(inventoryType, id);
      storageWindow.InventoryUI.SetupFastDrop(inventoryType, id);
    }
  }
}