using Scriptables;
using UnityEngine;
using Utils;

namespace World {
  public class BuildingsDataController : MonoBehaviour {
    private Building[,] _buildDatas;

    //Cells fill data array
    private int[,] _buildFillDatas;

    public int[,] BuildFillDatas => _buildFillDatas;

    public void Initialize() {
      _buildDatas = new Building[GameManager.Instance.GameConfig.BuildingAreaSizeX,
        GameManager.Instance.GameConfig.BuildingAreaSizeY];
      _buildFillDatas = new int[GameManager.Instance.GameConfig.BuildingAreaSizeX,
        GameManager.Instance.GameConfig.BuildingAreaSizeY];
    }

    public Building GetBuildDataConverted(int xCoord, int yCoord) {
      var convertedCoords = CoordsTransformer.GridToBuildingsGrid(xCoord, yCoord);
      return _buildDatas[convertedCoords.X, convertedCoords.Y];
    }

    public Building GetBuildData(int xCoord, int yCoord) {
      var buildCoords = CoordsTransformer.GridToBuildingsGrid(new Coords(xCoord, yCoord));
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
      //Debug.DrawRay(pos, Vector3.up * 10f, Color.green, 100f);
    }

    public void RemoveBuildData(Building data, Vector3 pos) {
      var worldCoords = CoordsTransformer.WorldToGrid(pos);
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