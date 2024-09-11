using System.Collections.Generic;
using UnityEngine;

namespace World {
  public class ChunkObject : MonoBehaviour {
    private ChunkData _chunkData;
    private List<CellObject> _cellObjects = new();

    public ChunkData ChunkData => _chunkData;

    public void Init(ChunkData chunkData) {
      _chunkData = chunkData;
    }

    CellObject GetCell(int x, int y) {
      for (var i = 0; i < _cellObjects.Count; i++) {
        if (_cellObjects[i].CellData.x == x &&
            _cellObjects[i].CellData.y == y) {
          return _cellObjects[i];
        }
      }

      return null;
    }

    public void AddCellObject(CellObject cellObject) {
      _cellObjects.Add(cellObject);
    }

    private void RemoveCellObject(CellObject cellObject) {
      _cellObjects.Remove(cellObject);
    }

    public void FillCells() {
      for (var i = 0; i < _cellObjects.Count; i++) {
        _cellObjects[i].InitSprite();
      }
    }

    public void TriggerCellDestroyed(CellObject cellObject) {
      RemoveCellObject(cellObject);
      cellObject.CellData.Destroy();
      var x = cellObject.CellData.x;
      var y = cellObject.CellData.y;
      var cellUp = GetCell(x - 1, y);
      var cellDown = GetCell(x + 1, y);
      var cellLeft = GetCell(x, y - 1);
      var cellRight = GetCell(x, y + 1);

      if (cellUp) cellUp.InitSprite();
      if (cellDown) cellDown.InitSprite();
      if (cellLeft) cellLeft.InitSprite();
      if (cellRight) cellRight.InitSprite();
    }
  }
}