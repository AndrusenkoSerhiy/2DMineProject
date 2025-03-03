using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows;
using SaveSystem;
using Scriptables.Craft;

namespace Craft {
  public class Crafter : MonoBehaviour, ISaveLoad {
    [SerializeField] private GameObject overlayPrefab;
    [SerializeField] private GameObject interfacePrefab;
    [SerializeField] protected Workstation station;

    protected CraftWindow craftWindow;
    private CraftManager craftManager;

    private bool saving;
    private Coroutine checkCoroutine;

    public void Awake() {
      station.Clear();

      Load();
      StartCraftingCheck();
    }

    private void StartCraftingCheck() {
      if (checkCoroutine == null && station.CraftingTasks.Count > 0) {
        checkCoroutine = StartCoroutine(CheckCraftingLoop());
      }
    }

    private void UpdateCraftingAndCheck() {
      station.UpdateCraftingTasks();
      StartCraftingCheck();
    }

    private void StopCraftingCheck() {
      if (checkCoroutine == null) {
        return;
      }

      StopCoroutine(checkCoroutine);
      checkCoroutine = null;
    }

    private IEnumerator CheckCraftingLoop() {
      while (true) {
        yield return new WaitForSeconds(1);

        CheckForCompletedTask();

        // Stop if no more tasks
        if (station.CraftingTasks.Count > 0) {
          continue;
        }

        StopCraftingCheck();
        break;
      }
    }

    private void CheckForCompletedTask() {
      Debug.Log($"CheckForCompletedTask {gameObject.name}");
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

    protected void Init() {
      if (craftWindow != null) {
        return;
      }

      var craftWindowObj = Instantiate(interfacePrefab, overlayPrefab.transform);
      craftManager = craftWindowObj.GetComponent<CraftManager>();
      craftManager.Setup(station);

      craftWindowObj.transform.SetSiblingIndex(0);

      craftWindow = craftWindowObj.GetComponent<CraftWindow>();
      GameManager.Instance.WindowsController.AddWindow(craftWindow);

      craftWindow.OnHide += HideWindowHandler;
    }

    protected void CheckInteract() {
      Init();

      if (craftWindow.IsShow) {
        craftWindow.Hide();
      }
      else {
        craftWindow.Show();
      }
    }

    private void HideWindowHandler(WindowBase window) {
      UpdateCraftingAndCheck();
      Save();
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