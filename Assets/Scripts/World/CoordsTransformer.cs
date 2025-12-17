
using UnityEngine;
using Utils;

namespace World {
  public static class CoordsTransformer {
    public static Vector3 GridToWorld(int col, int row) {
      // Shift column and row by their respective origins
      var config = GameManager.Instance.GameConfig;
      var x = (col - config.OriginCol) * config.CellSizeX;
      var y = (row - config.OriginRow) * (-config.CellSizeY);
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
      var config = GameManager.Instance.GameConfig;
      
      return new Coords(Mathf.RoundToInt(position.x / config.CellSizeX) +
                        config.OriginCol,
        Mathf.RoundToInt(position.y / (-config.CellSizeY)) +
        config.OriginRow);
    }
    
    //buildings converter
    public static Vector3 GridToWorldBuildings(int col, int rowOffset) {
      int row = rowOffset - GameManager.Instance.GameConfig.BuildingAreaYDiff;
      //Debug.Log("GridToWorldBuildings : "+col + "," + row);
      return GridToWorld(col, row);
    }
    
    public static Coords WorldToGridBuildings(Vector3 position) {
      var original = MouseToGridPosition(position);
      original.Y += GameManager.Instance.GameConfig.BuildingAreaYDiff;
      return original; // convert back from real to offset
    }

    public static Coords GridToBuildingsGrid(Coords original) {
      return new Coords(original.X,
        original.Y + GameManager.Instance.GameConfig.BuildingAreaYDiff);
    }
    
    public static Coords GridToBuildingsGrid(int x_original, int y_original) {
      return new Coords(x_original, y_original + GameManager.Instance.GameConfig.BuildingAreaYDiff);
    }
    
    public static Coords BuildingsGridToGrid(Coords original) {
      return new Coords(original.X, original.Y - GameManager.Instance.GameConfig.BuildingAreaYDiff);
    }

    public static Vector3 OutOfGridToWorls(int x_original, int y_original) {
      var config = GameManager.Instance.GameConfig;
      var x = (x_original - config.OriginCol) * config.CellSizeX;
      var y = (y_original - config.OriginRow) * (-config.CellSizeY);
      return new Vector3(x, y, 0);
    }
  }
}