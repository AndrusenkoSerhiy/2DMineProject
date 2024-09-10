using System;
using UnityEngine;

namespace World {
  [Serializable]
  public class CellData {
    public int x;
    public int y;
    public float perlin;
    private ChunkData _chunk;
    public bool HasNeighbours => hasNeighbours();

    public CellData(int x, int y, float perlin, ChunkData chunk) {
      this.x = x;
      this.y = y;
      this.perlin = perlin;
      _chunk = chunk;
    }

    private bool hasNeighbours() {
      if (x == 0 || x == _chunk.height - 1) return false;
      if (y == 0 || y == _chunk.width - 1) return false;

      if (_chunk.CellFillDatas[x - 1, y] != 1)
        return false;
      if (_chunk.CellFillDatas[x, y - 1] != 1)
        return false;
      if (_chunk.CellFillDatas[x + 1, y] != 1)
        return false;
      if (_chunk.CellFillDatas[x, y + 1] != 1)
        return false;
      return true;
    }

    public void Destroy() {
      _chunk.SetCellFill(x, y, 0);
    }
  }
}