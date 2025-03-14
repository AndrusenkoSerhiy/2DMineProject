using System.Collections.Generic;
using Inventory;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class Window : MonoBehaviour, IInventoryDropZoneUI {
    [SerializeField] private Button takeAllButton;
    [SerializeField] private bool preventItemDrop;
    [SerializeField] private UserInterface outputInterface;

    private GameManager gameManager;
    private Workstation station;
    private PlayerInventory playerInventory;
    private List<InventoryObject> outputInventories;
    private bool started;

    public bool PreventItemDropIn => preventItemDrop;
    public Workstation Station => station;

    public void Setup(Workstation station) {
      this.station = station;
      outputInterface.Setup(station.OutputInventoryType, station.Id);
    }

    public void Awake() {
      ServiceLocator.For(this).Register(station);
      gameManager = GameManager.Instance;
    }

    public void Start() {
      Init();
      started = true;
    }

    public void OnEnable() {
      if (!started) {
        return;
      }

      Init();
    }

    private void OnDisable() {
      RemoveEvents();
    }

    private void Init() {
      InitReferences();
      AddEvents();
    }

    private void InitReferences() {
      if (playerInventory != null) {
        return;
      }

      playerInventory = gameManager.PlayerInventory;
      outputInventories = station.GetOutputInventories();
    }

    private void AddEvents() {
      //craft output slots
      takeAllButton?.onClick.AddListener(OnTakeAllButtonClickHandler);
    }

    private void RemoveEvents() {
      //craft output slots
      takeAllButton?.onClick.RemoveAllListeners();
    }

    private void OnTakeAllButtonClickHandler() {
      foreach (var inventory in station.InventoriesPool.Inventories) {
        outputInventories[0].MoveAllItemsTo(inventory);
      }
    }
  }
}