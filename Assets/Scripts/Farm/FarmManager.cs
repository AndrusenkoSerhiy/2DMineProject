using System;
using Craft;
using SaveSystem;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;
using World;

namespace Farm {
  public class FarmManager : MonoBehaviour, ISaveLoad {
    [SerializeField] private SerializedDictionary<string, ProcessingPlantBox> allPlantBoxes = new();

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
    }
    
    public void InitPlantBox(PlantBox box, Coords coords) {
      var buildCoord = CoordsTransformer.GridToBuildingsGrid(coords);
      var key = $"{buildCoord.X}|{buildCoord.Y}";
      if (allPlantBoxes.ContainsKey(key))
        return;
      
      //Debug.LogError($"init {key}");
      allPlantBoxes.Add($"{key}", new ProcessingPlantBox() {
        Coord = $"{key}",
        HasGround = false,
        HasSeeds = false,
        StartGrowing = false,
        HasRipened = false,
        CurrSeed = null,
        CurrHarvest = null,
        TimeToGrowth = 0,
        CurrTime = 0
      });
      box.SetParamFromManager(allPlantBoxes[$"{key}"]);
    }

    public void RemovePlantBox(Coords coords) {
      var gridCoords = CoordsTransformer.GridToBuildingsGrid(coords);
      //Debug.LogError($"RemovePlantBox{gridCoords.X}|{gridCoords.Y}");
      if (allPlantBoxes.ContainsKey($"{gridCoords.X}|{gridCoords.Y}")) {
        allPlantBoxes.Remove($"{gridCoords.X}|{gridCoords.Y}");
      }
    }
    //when obj disable because we are to far
    public void SetParamFromBuild(string key, BuildingDataObject build) {
      //Debug.LogError($"SetParamFromBuild: {key}");
      if (build.TryGetComponent<PlantBox>(out var box)) {
        allPlantBoxes[key].Coord = key;//string coord
        allPlantBoxes[key].HasGround = box.HasGround;
        allPlantBoxes[key].HasSeeds = box.HasSeeds;
        allPlantBoxes[key].StartGrowing = box.StartGrowing;
        allPlantBoxes[key].HasRipened = box.HasRipened;
        allPlantBoxes[key].CurrSeed = box.CurrSeed;
        allPlantBoxes[key].CurrHarvest = box.CurrHarvest;
        allPlantBoxes[key].TimeToGrowth = box.TimeToGrowth;
        allPlantBoxes[key].CurrTime = box.CurrTime;
        allPlantBoxes[key].LastUpdateTime = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
      }
    }

    //when we close enough to the obj
    public void UpdateParamAfterEnable(string key, BuildingDataObject build, bool load = false) {
      //Debug.LogError($"UpdateParamAfterEnable: {key}");
      if (build.TryGetComponent<PlantBox>(out var box)) {
        //Debug.LogError($"SetParamFromManager!!!!!!!!!!: {key}");
        box.SetParamFromManager(allPlantBoxes[key], load);
      }
    }
    
    public void UpdateParamAfterLoad(string key) {
      var parts = key.Split('|');
      int.TryParse(parts[0], out int first);
      int.TryParse(parts[1], out int second);
      var buildData = GameManager.Instance.ChunkController.GetBuildingData(first, second);
      if (buildData) {
        //Debug.LogError($"UpdateParamAfterLoad!!!!!!!!!!: {key}");
        GameManager.Instance.FarmManager.UpdateParamAfterEnable($"{first}|{second}", buildData, true);
      }
    }
    
    #region Save/Load

    public int Priority => LoadPriority.FARM;
    public void Save() {
      //save plant boxes
      if (allPlantBoxes.Count == 0) {
        return;
      }
      
      SaveLoadSystem.Instance.gameData.AllPlantBoxes = new SerializedDictionary<string, ProcessingPlantBox>();
      foreach (var boxData in allPlantBoxes) {
        var parts = boxData.Key.Split('|');
        int.TryParse(parts[0], out int first);
        int.TryParse(parts[1], out int second);
        var buildData = GameManager.Instance.ChunkController.GetBuildingData(first, second);
        if (buildData) {
          SetParamFromBuild(boxData.Key, buildData);
          boxData.Value.LastUpdateTime = 0;
        }

        SaveLoadSystem.Instance.gameData.AllPlantBoxes.Add(boxData.Key, boxData.Value);
      }
    }

    public void Load() {
      if (SaveLoadSystem.Instance.IsNewGame()) {
        return;
      }
      //Debug.LogError("load boxes");
      allPlantBoxes.Clear();
      foreach (var boxData in SaveLoadSystem.Instance.gameData.AllPlantBoxes) {
        allPlantBoxes.Add(boxData.Key, boxData.Value);
      }
      foreach (var boxData in allPlantBoxes) {
        UpdateParamAfterLoad(boxData.Key);
      }
    }

    public void Clear() {
      allPlantBoxes.Clear();
      SaveLoadSystem.Instance.gameData.AllPlantBoxes = new ();
    }
    #endregion
  }
}