using System;
using Scriptables;

namespace World {
  [Serializable]
  public class CellData {
    public int x;
    public int y;
    public float perlin;
    private ChunkData _chunk;
    private ResourceData resData;
    public int NeighboursIndex => neighboursIndex();
    public bool HasAnyNeighbours => hasAnyNeighbours();
    public bool HasStandPoint => hasStandPoint();

    public CellData(int x, int y, float perlin, ChunkData chunk) {
      this.x = x;
      this.y = y;
      this.perlin = perlin;
      _chunk = chunk;
    }

    private int neighboursIndex() {
      //allNeighbours
      if (_chunk.GetCellFill(x, y - 1, perlin) == 1 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 1 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 1 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 1) {
        return 0;
      }

      //up free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 0 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 1 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 1 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 1) {
        return 1;
      }

      //left free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 1 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 1 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 0 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 1) {
        return 2;
      }

      //right free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 1 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 1 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 1 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 0) {
        return 3;
      }

      //bottom free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 1 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 0 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 1 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 1) {
        return 4;
      }

      //left up free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 0 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 1 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 0 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 1) {
        return 5;
      }

      //right up free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 0 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 1 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 1 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 0) {
        return 6;
      }

      //left bottom free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 1 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 0 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 0 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 1) {
        return 7;
      }

      //right bottom free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 1 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 0 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 1 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 0) {
        return 8;
      }

      //left right free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 1 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 1 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 0 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 0) {
        return 9;
      }

      //up bottom free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 0 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 0 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 1 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 1) {
        return 10;
      }

      //bottom free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 0 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 1 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 0 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 0) {
        return 11;
      }

      //right free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 0 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 0 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 0 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 1) {
        return 12;
      }

      //left free
      if (_chunk.GetCellFill(x, y - 1, perlin) == 0 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 0 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 1 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 0) {
        return 13;
      }

      //upfree
      if (_chunk.GetCellFill(x, y - 1, perlin) == 1 &&
          _chunk.GetCellFill(x, y + 1, perlin) == 0 &&
          _chunk.GetCellFill(x - 1, y, perlin) == 0 &&
          _chunk.GetCellFill(x + 1, y, perlin) == 0) {
        return 14;
      }

      return 15;
    }

    private bool hasAnyNeighbours() {
      return _chunk.GetCellFill(x, y - 1) == 1 ||
             _chunk.GetCellFill(x, y + 1) == 1 ||
             _chunk.GetCellFill(x - 1, y) == 1 ||
             _chunk.GetCellFill(x + 1, y) == 1;
    }
    
    private bool hasStandPoint() {
      return _chunk.GetCellFill(x, y + 1) == 1;
    }

    public void Destroy() {
      _chunk.SetCellFill(x, y, 0);
      perlin = -10000f;
    }
  }
}