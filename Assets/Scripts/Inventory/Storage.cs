using System;
using System.Collections.Generic;
using Windows;
using Interaction;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Inventory {
  public class Storage : MonoBehaviour, IInteractable, IBaseCellHolder {
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

    private List<CellObject> baseCells = new();
    private readonly Dictionary<CellObject, Action> cellDestroyedHandlers = new();

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

    public void ClearBaseCells() {
      foreach (var kvp in cellDestroyedHandlers) {
        kvp.Key.OnDestroyed -= kvp.Value;
      }

      cellDestroyedHandlers.Clear();
      baseCells.Clear();
    }
    
    public void SetBaseCells(List<CellObject> cells) {
      ClearBaseCells();

      baseCells = cells;

      foreach (var cell in cells) {
        Action handler = () => OnBaseCellDestroyedHandler(cell);
        cellDestroyedHandlers[cell] = handler;
        cell.OnDestroyed += handler;
      }
    }

    private void OnBaseCellDestroyedHandler(CellObject cell) {
      if (baseCells == null || baseCells.Count == 0) {
        return;
      }

      if (cellDestroyedHandlers.TryGetValue(cell, out var handler)) {
        cell.OnDestroyed -= handler;
        cellDestroyedHandlers.Remove(cell);
      }

      baseCells.Remove(cell);

      if (baseCells.Count > 0) {
        return;
      }

      var storageInventory = gameManager.PlayerInventory.GetInventoryByTypeAndId(inventoryType, GetId());
      if (storageInventory != null) {
        foreach (var slot in storageInventory.Slots) {
          if (slot.isEmpty) {
            continue;
          }

          gameManager.PlayerInventory.SpawnItem(slot.Item, slot.amount, transform.position);
        }

        storageInventory.Clear();
      }

      gameManager.PlaceCell.RemoveBuilding(buildObject, storageItemObject);

      gameManager.MessagesManager.ShowSimpleMessage("Storage destroyed");

      gameManager.PoolEffects.SpawnFromPool("PlaceCellEffect", transform.position, Quaternion.identity);
      gameManager.AudioController.PlayStorageDestroyed();
    }
  }
}