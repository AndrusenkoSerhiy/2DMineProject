using System.Collections;
using System.Collections.Generic;
using SaveSystem;
using Scriptables.Craft;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Craft {
  public class CraftTasks : MonoBehaviour, ISaveLoad {
    [SerializeField] private SerializedDictionary<string, Workstation> allStationsMap;
    [SerializeField] private List<string> stations;
    [SerializeField] private SerializedDictionary<string, bool> windowOpenStates;
    [SerializeField] private SerializedDictionary<string, bool> saving;
    [SerializeField] private SerializedDictionary<string, Coroutine> checkCoroutines;

    private void Awake() {
      Load();
      StartCraftingCheck();
    }

    public void SetStation(Workstation station) {
      if (stations.Contains(station.Id)) {
        return;
      }

      var id = station.Id;
      allStationsMap.Add(id, station);
      stations.Add(id);
      windowOpenStates.Add(id, false);
      saving.Add(id, false);
      checkCoroutines.Add(id, null);
    }

    public void UpdateWindowState(string stationId, bool isOpen) {
      windowOpenStates[stationId] = isOpen;

      if (!isOpen) {
        UpdateCraftingAndCheck(stationId);
      }
    }

    private void StartCraftingCheck() {
      foreach (var id in stations) {
        StationCraftingCheck(id);
      }
    }

    private void StationCraftingCheck(string id) {
      var station = allStationsMap[id];
      if (checkCoroutines[id] == null && station.CraftingTasks.Count > 0) {
        checkCoroutines[id] = StartCoroutine(CheckCraftingLoop(station));
      }
    }

    private void StopCraftingCheck(string stationId) {
      if (checkCoroutines[stationId] == null) {
        return;
      }

      StopCoroutine(checkCoroutines[stationId]);
      checkCoroutines[stationId] = null;
    }

    private IEnumerator CheckCraftingLoop(Workstation station) {
      while (true) {
        yield return new WaitForSeconds(1);

        CheckForCompletedTask(station);

        // Stop if no more tasks
        if (station.CraftingTasks.Count > 0) {
          continue;
        }

        StopCraftingCheck(station.Id);
        break;
      }
    }

    private void CheckForCompletedTask(Workstation station) {
      if (station == null || IsWindowOpen(station.Id)) {
        return;
      }

      var task = station.RemoveFirstTaskIfEnded();
      if (task != null) {
        RunEffect(station, task.Value.ItemId);
      }
    }

    private void UpdateCraftingAndCheck(string id) {
      var station = allStationsMap[id];
      station.UpdateCraftingTasks();
      StationCraftingCheck(id);
    }


    private void RunEffect(Workstation station, string itemId) {
      if (!station.PlayEffectsWhenClosed) {
        return;
      }
      
      var item = GameManager.Instance.ItemDatabaseObject.ItemsMap[itemId];
      GameManager.Instance.MessagesManager.ShowCraftMessage(item, 1);
    }

    private bool IsWindowOpen(string stationId) {
      return windowOpenStates != null && windowOpenStates[stationId];
    }

    private bool IsSaving(string stationId) {
      return saving != null && saving[stationId];
    }

    public void Load() {
      SetStationsFromLoadData();
      LoadInputs();
    }

    private void SetStationsFromLoadData() {
      var stationsData = SaveLoadSystem.Instance.gameData.Workstations;
      foreach (var (id, stationData) in stationsData) {
        if (string.IsNullOrEmpty(stationData.ResourcePath)) {
          return;
        }
        
        var station = AssetDatabase.LoadAssetAtPath<Workstation>(stationData.ResourcePath);
        if (station == null) {
          return;
        }

        SetStation(station);
      }
    }

    private void LoadInputs() {
      foreach (var id in stations) {
        if (!allStationsMap.ContainsKey(id)) {
          continue;
        }

        var station = allStationsMap[id];
        LoadStationInputs(station);
      }
    }

    private void LoadStationInputs(Workstation station) {
      if (!SaveLoadSystem.Instance.gameData.Workstations.TryGetValue(station.Id, out var data)) {
        return;
      }

      station.Load(data);
    }

    public void Save() {
      foreach (var id in stations) {
        if (!allStationsMap.ContainsKey(id)) {
          continue;
        }

        var station = allStationsMap[id];

        saving[id] = true;
        station.ProcessCraftedInputs();
        SaveWorkstationInputs(station);
        saving[id] = false;
      }
    }

    private void SaveWorkstationInputs(Workstation station) {
      var inputs = new List<CraftInputData>();

      var stationId = station.Id;
      var resourcePath = AssetDatabase.GetAssetPath(station);

      if (station.Inputs.Count == 0) {
        SaveLoadSystem.Instance.gameData.Workstations[stationId] =
          new WorkstationsData { Id = stationId, Inputs = inputs, ResourcePath = resourcePath };
        return;
      }

      var timeLeftInMilliseconds = station.CalculateTimeLeftInMilliseconds(station.Inputs[0]);

      foreach (var input in station.Inputs) {
        inputs.Add(new CraftInputData {
          RecipeId = input.Recipe.Id,
          Count = input.Count,
        });
      }

      SaveLoadSystem.Instance.gameData.Workstations[stationId] = new WorkstationsData
        { Id = stationId, Inputs = inputs, MillisecondsLeft = timeLeftInMilliseconds, ResourcePath = resourcePath };
    }
  }
}