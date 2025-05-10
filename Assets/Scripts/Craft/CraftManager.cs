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

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
    }

    #region Save/Load

    public int Priority => LoadPriority.CRAFT;

    public void Load() {
      if (SaveLoadSystem.Instance.IsNewGame()) {
        return;
      }

      SetStationsFromLoadData();
      LoadInputs();
    }

    public void Clear() {
      allStationsMap.Clear();
      stations.Clear();
      windowOpenStates.Clear();
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

        SaveWorkstationInputs(station);
      }
    }

    private void SaveWorkstationInputs(Workstation station) {
      var inputs = new List<CraftInputData>();

      var stationId = station.Id;

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

    #endregion

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

    public Workstation GetWorkstation(string fullId, string stationObjectId) {
      if (allStationsMap.ContainsKey(fullId)) {
        return allStationsMap[fullId];
      }

      var stationObject = workstationsDatabase.ItemsMap[stationObjectId];
      var station = new Workstation(stationObject, fullId);

      return station;
    }

    private void SetStationsFromLoadData() {
      var stationsData = SaveLoadSystem.Instance.gameData.Workstations;
      foreach (var (id, stationData) in stationsData) {
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
  }
}