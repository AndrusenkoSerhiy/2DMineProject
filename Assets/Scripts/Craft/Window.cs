using Inventory;
using UnityEngine;
using UnityEngine.UI;
using UnityServiceLocator;

namespace Craft {
  public class Window : MonoBehaviour, IInventoryDropZoneUI {
    [SerializeField] private Button takeAllButton;
    [SerializeField] private bool preventItemDrop;
    [SerializeField] private UserInterface outputInterface;

    private Workstation station;

    public bool PreventItemDropIn => preventItemDrop;
    public Workstation Station => station;

    public void Setup(Workstation station) {
      this.station = station;
      outputInterface.Setup(station.OutputInventoryType, station.Id);
    }

    public void Awake() {
      ServiceLocator.For(this).Register(station);
    }

    public void OnEnable() {
      AddEvents();
    }

    private void OnDisable() {
      RemoveEvents();
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
      station.MoveAllFromOutput();
      GameManager.Instance.AudioController.PlayUIClick();
    }
  }
}