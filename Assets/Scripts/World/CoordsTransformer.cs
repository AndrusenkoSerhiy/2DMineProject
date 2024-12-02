using Game;
using UnityEngine;
using Utils;

namespace World {
  public static class CoordsTransformer {
    public static Vector3 GridToWorld(int col, int row) {
      // Shift column and row by their respective origins
      var x = (col - GameManager.instance.GameConfig.OriginCol) * GameManager.instance.GameConfig.CellSizeX;
      var y = (row - GameManager.instance.GameConfig.OriginRow) * (-GameManager.instance.GameConfig.CellSizeY);
      return new Vector3(x, y, 0);
    }

    // Convert from world position (Vector3) to grid coordinates (col, row)
    public static Coords WorldToGrid(Vector3 position) {
      // Shift position back to grid coordinates
      var col = Mathf.RoundToInt(position.x / GameManager.instance.GameConfig.CellSizeX) +
                GameManager.instance.GameConfig.OriginCol;
      col = Mathf.Clamp(col, 0, GameManager.instance.GameConfig.ChunkSizeX - 1);
      var row = Mathf.RoundToInt(position.y / (-GameManager.instance.GameConfig.CellSizeY)) +
                GameManager.instance.GameConfig.OriginRow;
      row = Mathf.Clamp(row, 0, GameManager.instance.GameConfig.ChunkSizeY - 1);
      return new Coords(col, row);
    }
  }
}