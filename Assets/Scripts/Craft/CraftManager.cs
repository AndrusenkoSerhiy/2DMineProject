using System.Collections.Generic;
using SaveSystem;
using Scriptables.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace Craft {
  public class CraftManager : MonoBehaviour, ISaveLoad {
    [SerializeField] private WorkstationsDatabaseObject workstationsDatabase;
    [SerializeField] private SerializedDictionary<string, Workstation> allStationsMap;

    [SerializeField] private List<string> stations;

    [SerializeField] private SerializedDictionary<string, bool> windowOpenStates;
    // [SerializeField] private SerializedDictionary<string, bool> saving;

    private InventoriesPool inventoriesPool;

    public InventoriesPool InventoriesPool => inventoriesPool;

    private void Awake() {
      inventoriesPool = new InventoriesPool();
      inventoriesPool.Init();
      Load();
    }

    public void SetStation(Workstation station) {
      if (stations.Contains(station.Id)) {
        return;
      }

      var id = station.Id;
      allStationsMap.Add(id, station);
      stations.Add(id);
      windowOpenStates.Add(id, false);
      // saving.Add(id, false);
    }

    public void UpdateWindowState(string stationId, bool isOpen) {
      windowOpenStates[stationId] = isOpen;
    }

    public bool IsWindowOpen(string stationId) {
      return windowOpenStates != null && windowOpenStates[stationId];
    }

    /*private bool IsSaving(string stationId) {
      return saving != null && saving[stationId];
    }*/

    public Workstation GetWorkstation(string fullId, string stationObjectId) {
      if (allStationsMap.ContainsKey(fullId)) {
        return allStationsMap[fullId];
      }

      var stationObject = workstationsDatabase.ItemsMap[stationObjectId];
      var station = new Workstation(stationObject, fullId);

      return station;
    }

    public void Load() {
      SetStationsFromLoadData();
      LoadInputs();
    }

    private void SetStationsFromLoadData() {
      var stationsData = SaveLoadSystem.Instance.gameData.Workstations;
      foreach (var (id, stationData) in stationsData) {
        /*if (string.IsNullOrEmpty(stationData.ResourcePath)) {
          return;
        }

        var handle = Addressables.LoadAssetAsync<Workstation>(stationData.ResourcePath);
        var station = handle.WaitForCompletion();*/
        var station = GetWorkstation(id, stationData.WorkStationObjectId);
        if (station == null) {
          return;
        }

        SetStation(station);
      }
    }

    private void OnDisable() {
      if (allStationsMap.Count <= 0) {
        return;
      }

      foreach (var (id, station) in allStationsMap) {
        station.CancelCraft(true);
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
        station.CancelCraft(true);

        // saving[id] = true;
        SaveWorkstationInputs(station);
        // saving[id] = false;
      }
    }

    private void SaveWorkstationInputs(Workstation station) {
      var inputs = new List<CraftInputData>();

      var stationId = station.Id;
      var resourcePath = station.WorkstationObject.ResourcePath;

      if (station.Inputs.Count == 0) {
        SaveLoadSystem.Instance.gameData.Workstations[stationId] =
          new WorkstationsData {
            Id = stationId,
            Inputs = inputs,
            WorkStationObjectId = station.WorkstationObject.Id,
            CurrentProgress = new CurrentProgress()
          };
        return;
      }

      foreach (var input in station.Inputs) {
        inputs.Add(new CraftInputData {
          RecipeId = input.Recipe.Id,
          Count = input.Count,
        });
      }

      SaveLoadSystem.Instance.gameData.Workstations[stationId] = new WorkstationsData {
        Id = stationId,
        Inputs = inputs,
        WorkStationObjectId = station.WorkstationObject.Id,
        CurrentProgress = station.CurrentProgress
      };
    }
  }
}