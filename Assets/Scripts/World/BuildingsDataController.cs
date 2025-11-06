using System.Collections.Generic;
using System.Linq;
using Craft;
using SaveSystem;
using Scriptables;
using UnityEngine;
using Utils;

namespace World {
  public class BuildingsDataController : MonoBehaviour, ISaveLoad {
    [SerializeField] private BuildingDataLibrary buildingDataLibrary;
    private Building[,] _buildDatas;

    //Cells fill data array
    private int[,] _buildFillDatas;
    public int[,] BuildFillDatas => _buildFillDatas;

    public void Awake() {
      SaveLoadSystem.Instance.Register(this);
    }

    public void Initialize() {
      /*_buildDatas = new Building[GameManager.Instance.GameConfig.BuildingAreaSizeX,
        GameManager.Instance.GameConfig.BuildingAreaSizeY];
      _buildFillDatas = new int[GameManager.Instance.GameConfig.BuildingAreaSizeX,
        GameManager.Instance.GameConfig.BuildingAreaSizeY];*/

      if (!GameManager.Instance.InitScriptsOnStart()) {
        return;
      }

      Load();
    }

    public void AddPlantBox(PlantBox plantBox, Coords coords) {
      //UpdateFromLoad();
      GameManager.Instance.FarmManager.InitPlantBox(plantBox, coords);
      //Debug.LogError("Added plantbox");
    }
    
    public void RemovePlantBox(PlantBox plantBox) {
      //GameManager.Instance.FarmManager.RemovePlantBox(plantBox, coords);
      Debug.LogError("Removed plantbox");
    }

    #region Save/Load

    public int Priority => LoadPriority.BUILDINGS;

    public void Clear() {
    }

    public void Load() {
      var sizeX = GameManager.Instance.GameConfig.BuildingAreaSizeX;
      var sizeY = GameManager.Instance.GameConfig.BuildingAreaSizeY;

      _buildDatas = new Building[sizeX, sizeY];
      _buildFillDatas = new int[sizeX, sizeY];

      if (SaveLoadSystem.Instance.IsNewGame()) {
        return;
      }

      var data = SaveLoadSystem.Instance.gameData.WorldData;

      // Load buildings
      if (data.BuildDatas is { Count: > 0 }) {
        foreach (var bd in data.BuildDatas) {
          _buildDatas[bd.X, bd.Y] = buildingDataLibrary.ItemsMap[bd.BuildId];
        }
      }

      // Load fills
      if (data.BuildFillDatas is { Count: > 0 }) {
        foreach (var fill in data.BuildFillDatas) {
          _buildFillDatas[fill.X, fill.Y] = fill.Value;
        }
      }
    }
    
    public void Save() {
      if (_buildDatas == null || _buildFillDatas == null) {
        return;
      }

      var data = SaveLoadSystem.Instance.gameData.WorldData;
      data.BuildDatas = new List<BuildingData>();
      data.BuildFillDatas = new List<CellFill>();

      // Save _buildDatas without nulls
      for (var x = 0; x < _buildDatas.GetLength(0); x++) {
        for (var y = 0; y < _buildDatas.GetLength(1); y++) {
          var building = _buildDatas[x, y];
          if (building != null) {
            data.BuildDatas.Add(new BuildingData {
              X = x,
              Y = y,
              BuildId = building.Id
            });
          }
        }
      }

      // Save only non-zero fill cells
      for (var x = 0; x < _buildFillDatas.GetLength(0); x++) {
        for (var y = 0; y < _buildFillDatas.GetLength(1); y++) {
          var val = _buildFillDatas[x, y];
          if (val != 0) {
            data.BuildFillDatas.Add(new CellFill {
              X = x,
              Y = y,
              Value = val
            });
          }
        }
      }
    }

    #endregion
    public Building GetBuildDataConverted(int xCoord, int yCoord) {
      var convertedCoords = CoordsTransformer.GridToBuildingsGrid(xCoord, yCoord);
      return _buildDatas[convertedCoords.X, convertedCoords.Y];
    }

    public Building GetBuildData(int xCoord, int yCoord) {
      var buildCoords = new Coords(xCoord, yCoord);
      //Debug.LogError($"build data {xCoord} | {yCoord} | {_buildDatas[buildCoords.X, buildCoords.Y]}");
      return _buildDatas[buildCoords.X, buildCoords.Y];
    }

    public void SetBuildFill(Building data, int xCoord, int yCoord, int value = 1) {
      var convertedCoords = CoordsTransformer.GridToBuildingsGrid(xCoord, yCoord);
      for (var x = 0; x < data.SizeX; x++) {
        for (var y = 0; y < data.SizeY; y++) {
          var coordX = convertedCoords.X + x;
          var coordY = convertedCoords.Y - y;
          _buildFillDatas[coordX, coordY] = value;
          //Debug.DrawRay(CoordsTransformer.GridToWorldBuildings(coordX,coordY), Vector3.up, Color.green, 100f);
        }
      }
    }

    public void SetBuildData(Building data, Vector3 pos) {
      var worldCoords = CoordsTransformer.MouseToGridPosition(pos);
      var buildCoords = CoordsTransformer.WorldToGridBuildings(pos);
      _buildDatas[buildCoords.X, buildCoords.Y] = data;
      SetBuildFill(data, worldCoords.X, worldCoords.Y);
      //Debug.DrawRay(pos, Vector3.up * 10f, Color.red, 100f);
      //Debug.LogError($"spawn {buildCoords.X} | {buildCoords.Y} | data {data}");
    }

    public void RemoveBuildData(Building data, Vector3 pos) {
      var worldCoords = CoordsTransformer.MouseToGridPosition(pos);
      var buildCoords = CoordsTransformer.WorldToGridBuildings(pos);
      _buildDatas[buildCoords.X, buildCoords.Y] = null;
      SetBuildFill(data, worldCoords.X, worldCoords.Y, 0);
    }

    public int GetCellFill(int x, int y) {
      var convertedCoords = CoordsTransformer.GridToBuildingsGrid(x, y);

      if (convertedCoords.X < 0 ||
          convertedCoords.X > GameManager.Instance.GameConfig.BuildingAreaSizeX ||
          convertedCoords.Y < 0 ||
          convertedCoords.Y > GameManager.Instance.GameConfig.BuildingAreaSizeY)
        return 0;

      return _buildFillDatas[convertedCoords.X, convertedCoords.Y];
    }
  }
}