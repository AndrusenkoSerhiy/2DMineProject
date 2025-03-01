using System.Collections.Generic;
using Interaction;
using UnityEngine;
using Windows;
using SaveSystem;
using Scriptables.Craft;

namespace Craft {
  public class Workbench : MonoBehaviour, IInteractable, ISaveLoad {
    [SerializeField] private string interactText;
    [SerializeField] private GameObject overlayPrefab;
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] protected Workstation station;

    private CraftWindow craftWindow;
    private CraftManager craftManager;
    public string InteractionPrompt => interactText;

    private bool saving;

    public void Awake() {
      station.Clear();

      Load();
    }

    public void Update() {
      if (station == null || saving || IsWindowOpen) {
        return;
      }

      var task = station.RemoveFirstTaskIfEnded();
      if (task != null) {
        RunEffect(task.Value.ItemId);
      }
    }

    private bool IsWindowOpen => craftWindow && craftWindow.IsShow;

    private void RunEffect(string itemId) {
      var windowOpen = craftWindow && craftWindow.IsShow;

      if (!windowOpen && !station.PlayEffectsWhenClosed) {
        return;
      }

      Debug.Log($"{station.name} RunEffect itemId {itemId}");
    }

    private void Init() {
      if (craftWindow != null) {
        return;
      }

      var craftWindowObj = Instantiate(interfacePrefab, overlayPrefab.transform);
      craftManager = craftWindowObj.GetComponent<CraftManager>();
      craftManager.Setup(station);

      craftWindowObj.transform.SetSiblingIndex(0);

      craftWindow = craftWindowObj.GetComponent<CraftWindow>();
      GameManager.Instance.WindowsController.AddWindow(craftWindow);
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      Init();

      if (craftWindow.IsShow) {
        craftWindow.Hide();
        Save();
      }
      else {
        craftWindow.Show();
      }

      return true;
    }

    #region Save/Load

    public string Id => station.Id;

    public void Load() {
      LoadFuelInventory();
      LoadInputs();
      LoadOutputInventory();
    }

    public void Save() {
      saving = true;

      station.ProcessCraftedInputs();
      SaveOutputInventory();
      SaveFuelInventory();
      SaveWorkstationInputs();
      saving = false;
    }

    private void LoadFuelInventory() {
      if (station.FuelInventory == null) {
        return;
      }

      station.FuelInventory.LoadFromGameData();
    }

    private void LoadOutputInventory() {
      station.OutputInventory.LoadFromGameData();
    }

    private void LoadInputs() {
      if (!SaveLoadSystem.Instance.gameData.Workstations.TryGetValue(Id, out var data)) {
        return;
      }

      var isNew = data.Inputs == null || data.Inputs.Count == 0;
      if (isNew) {
        return;
      }

      station.Load(data);
    }

    private void SaveOutputInventory() {
      station.OutputInventory.SaveToGameData();
    }

    private void SaveFuelInventory() {
      if (station.FuelInventory == null) {
        return;
      }

      station.FuelInventory.SaveToGameData();
    }

    private void SaveWorkstationInputs() {
      var inputs = new List<CraftInputData>();

      if (station.Inputs.Count == 0) {
        SaveLoadSystem.Instance.gameData.Workstations[Id] = new WorkstationsData { Id = Id, Inputs = inputs };
        return;
      }

      var timeLeft = station.CalculateTimeLeft(station.Inputs[0]);

      foreach (var input in station.Inputs) {
        inputs.Add(new CraftInputData {
          ItemId = input.Recipe.Result.Id,
          Count = input.Count,
        });
      }

      SaveLoadSystem.Instance.gameData.Workstations[Id] = new WorkstationsData
        { Id = Id, Inputs = inputs, SecondsLeft = timeLeft };
    }

    #endregion
  }
}