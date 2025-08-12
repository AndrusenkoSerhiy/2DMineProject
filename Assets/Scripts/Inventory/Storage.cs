using System.Collections.Generic;
using Windows;
using Interaction;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Inventory {
  public class Storage : MonoBehaviour, IInteractable, IBaseCellHolder {
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] private string interactText;
    [SerializeField] private string holdInteractText;
    [SerializeField] protected InventoryType inventoryType;
    [SerializeField] private BuildingDataObject buildObject;
    [SerializeField] private ItemObject storageItemObject;
    [SerializeField] private bool hasHoldInteraction = true;
    [SerializeField] private Recipe storageRecipe;
    [SerializeField] private Color destroyEffectColor = new(148, 198, 255, 255);

    public string InteractionText => interactText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;

    private StorageWindow storageWindow;
    protected GameManager gameManager;
    private string id;
    private string entityId;

    private CellHolderHandler cellHandler;

    private void Awake() {
      gameManager = GameManager.Instance;
      cellHandler = new CellHolderHandler(OnAllBaseCellsDestroyed, storageRecipe, transform.position);
    }

    public virtual bool Interact(PlayerInteractor playerInteractor) {
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

    protected string GetId() {
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

    public void SetBaseCells(List<CellObject> cells) {
      cellHandler.SetBaseCells(cells, transform.position);
    }

    public void ClearBaseCells() {
      cellHandler.ClearBaseCells();
    }

    private void OnAllBaseCellsDestroyed() {
      var storageInventory = gameManager.PlayerInventory.GetInventoryByTypeAndId(inventoryType, GetId());
      if (storageInventory != null) {
        foreach (var slot in storageInventory.Slots) {
          if (!slot.isEmpty) {
            gameManager.PlayerInventory.SpawnItem(slot.Item, slot.amount, transform.position);
          }
        }

        storageInventory.Clear();
      }

      var psGo = GameManager.Instance.PoolEffects
        .SpawnFromPool("CellDestroyEffect", transform.position, Quaternion.identity)
        .gameObject;
      var ps = psGo.GetComponent<ParticleSystem>();

      if (ps != null) {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(destroyEffectColor);
      }

      gameManager.PlaceCell.RemoveBuilding(buildObject, storageItemObject);
      gameManager.MessagesManager.ShowSimpleMessage("Storage destroyed");
      gameManager.AudioController.PlayStorageDestroyed();
    }
  }
}