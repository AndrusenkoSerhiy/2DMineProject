
using UnityEngine;
using Utils;

namespace World {
  public static class CoordsTransformer {
    public static Vector3 GridToWorld(int col, int row) {
      // Shift column and row by their respective origins
      var x = (col - GameManager.Instance.GameConfig.OriginCol) * GameManager.Instance.GameConfig.CellSizeX;
      var y = (row - GameManager.Instance.GameConfig.OriginRow) * (-GameManager.Instance.GameConfig.CellSizeY);
      return new Vector3(x, y, 0);
    }

    // Convert from world position (Vector3) to grid coordinates (col, row)
    public static void WorldToGrid(Vector3 position, ref Coords result) {
      var config = GameManager.Instance.GameConfig;
      // Shift position back to grid coordinates
      var col = Mathf.RoundToInt(position.x / config.CellSizeX) + config.OriginCol;
      col = Mathf.Clamp(col, 0, config.ChunkSizeX - 1);
      var row = Mathf.RoundToInt(position.y / (-config.CellSizeY)) + config.OriginRow;
      row = Mathf.Clamp(row, 0, config.ChunkSizeY - 1);
      result.X = col;
      result.Y = row;
    }
    
    //can get coord outside the array boundaries for cell object
    //use to place building on the same level then player spawned
    public static Coords MouseToGridPosition(Vector3 position) {
      var col = Mathf.RoundToInt(position.x / GameManager.Instance.GameConfig.CellSizeX) +
                GameManager.Instance.GameConfig.OriginCol;
      var row = Mathf.RoundToInt(position.y / (-GameManager.Instance.GameConfig.CellSizeY)) +
                GameManager.Instance.GameConfig.OriginRow;
      return new Coords(col, row);
    }
    
    //buildings converter
    public static Vector3 GridToWorldBuildings(int col, int rowOffset) {
      int row = rowOffset - GameManager.Instance.GameConfig.BuildingAreaYDiff;
      //Debug.Log("GridToWorldBuildings : "+col + "," + row);
      return GridToWorld(col, row);
    }
    
    public static Coords WorldToGridBuildings(Vector3 position) {
      var original = MouseToGridPosition(position);
      return new Coords(original.X, original.Y + GameManager.Instance.GameConfig.BuildingAreaYDiff); // convert back from real to offset
    }
    
    public static Coords GridToBuildingsGrid(Coords original) {
      return new Coords(original.X, original.Y + GameManager.Instance.GameConfig.BuildingAreaYDiff);
    }
    
    public static Coords GridToBuildingsGrid(int x_original, int y_original) {
      return new Coords(x_original, y_original + GameManager.Instance.GameConfig.BuildingAreaYDiff);
    }
    
    public static Coords BuildingsGridToGrid(Coords original) {
      return new Coords(original.X, original.Y - GameManager.Instance.GameConfig.BuildingAreaYDiff);
    }

    public static Vector3 OutOfGridToWorls(int x_original, int y_original) {
      var x = (x_original - GameManager.Instance.GameConfig.OriginCol) * GameManager.Instance.GameConfig.CellSizeX;
      var y = (y_original - GameManager.Instance.GameConfig.OriginRow) * (-GameManager.Instance.GameConfig.CellSizeY);
      return new Vector3(x, y, 0);
    }
  }
}