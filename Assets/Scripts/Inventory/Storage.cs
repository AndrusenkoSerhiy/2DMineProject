using Windows;
using Interaction;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Inventory {
  public class Storage : MonoBehaviour, IInteractable {
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] private string interactText;
    [SerializeField] private string holdInteractText;
    [SerializeField] private InventoryType inventoryType;
    [SerializeField] private BuildingDataObject buildObject;
    [SerializeField] private ItemObject storageItemObject;
    [SerializeField] private bool hasHoldInteraction = true;
    public string InteractionText => interactText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;

    private StorageWindow storageWindow;
    private GameManager gameManager;
    private string id;
    private string entityId;

    private void Awake() {
      gameManager = GameManager.Instance;
    }

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

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      if (!HasHoldInteraction) {
        return false;
      }

      var storageInventory = gameManager.PlayerInventory.GetInventoryByTypeAndId(inventoryType, GetId());

      if (!storageInventory.IsEmpty()) {
        gameManager.MessagesManager.ShowSimpleMessage("Storage is not empty.");
        return false;
      }

      gameManager.PlayerInventory.TakeBuildingToInventory(buildObject, storageItemObject);
      return true;
    }

    private void Init() {
      if (storageWindow != null) {
        return;
      }

      var storageWindowObj = Instantiate(interfacePrefab, GameManager.Instance.Canvas.transform);

      storageWindowObj.transform.SetSiblingIndex(0);
      storageWindow = storageWindowObj.GetComponent<StorageWindow>();
      GameManager.Instance.WindowsController.AddWindow(storageWindow);
      storageWindow.StorageUI.Setup(inventoryType, GetId());
      storageWindow.InventoryUI.SetupFastDrop(inventoryType, GetId());
    }

    private string GetId() {
      if (string.IsNullOrEmpty(id)) {
        id = InventoryObject.GenerateId(inventoryType, GetEntityId());
      }

      return id;
    }

    private string GetEntityId() {
      if (string.IsNullOrEmpty(entityId)) {
        entityId = InventoryObject.GenerateEntityIdByCell(buildObject);
      }

      return entityId;
    }
  }
}