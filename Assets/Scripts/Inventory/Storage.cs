using Windows;
using Interaction;
using UnityEngine;
using World;

namespace Inventory {
  public class Storage : MonoBehaviour, IInteractable {
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] private string interactText;
    [SerializeField] private string interactHeader;
    [SerializeField] private StorageType storageType;
    [SerializeField] private CellObject cellObject;
    public string InteractionText => interactText;
    public string InteractionHeader => interactHeader;

    private StorageWindow storageWindow;

    private string id = "storage_";
    public string Id => id;

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

      GenerateId();
      var storageWindowObj = Instantiate(interfacePrefab, GameManager.Instance.Canvas.transform);

      storageWindowObj.transform.SetSiblingIndex(0);
      storageWindow = storageWindowObj.GetComponent<StorageWindow>();
      GameManager.Instance.WindowsController.AddWindow(storageWindow);
      storageWindow.StorageUI.Setup(id, storageType);
      storageWindow.InventoryUI.SetupFastDrop(id, storageType);
    }

    private void GenerateId() {
      if (cellObject == null) {
        return;
      }

      id += cellObject.CellData.x + "_" + cellObject.CellData.y;
    }
  }
}