using UnityEngine;

namespace World {
  public class ChunkObject : MonoBehaviour {
    private ChunkData _chunkData;
    private CellObject[] _cellObjects;

    public void Init(ChunkData chunkData) {
      _chunkData = chunkData;
    }

    CellObject GetCell(int x, int y) {
      for (var i = 0; i < _cellObjects.Length; i++) {
        if (_cellObjects[i].CellData.x == x &&
            _cellObjects[i].CellData.y == y) {
          return _cellObjects[i];
        }
      }

      return null;
    }

    public void AddCellObject(CellObject cellObject) {
      ResizeArray();
      _cellObjects[_cellObjects.Length - 1] = cellObject;
    }

    private void RemoveCellObject(CellObject cellObject) {
      for (var i = 0; i < _cellObjects.Length; i++) {
        if (_cellObjects[i].CellData.x == cellObject.CellData.x &&
            _cellObjects[i].CellData.y == cellObject.CellData.y) {
          _cellObjects[i] = null;
          break;
        }
      }

      ShrinkArray();
    }

    public void FillCells() {
      for (var i = 0; i < _cellObjects.Length; i++) {
        _cellObjects[i].InitSprite();
      }
    }

    private void ResizeArray() {
      var justCreated = _cellObjects == null;
      var newSize = justCreated ? 1 : _cellObjects.Length + 1;
      var newArray = new CellObject[newSize];

      if (!justCreated)
        for (var i = 0; i < _cellObjects.Length; i++) {
          newArray[i] = _cellObjects[i];
        }

      _cellObjects = newArray;
    }

    private void ShrinkArray() {
      var newSize = _cellObjects.Length - 1;
      var newArray = new CellObject[newSize];
      var j = 0;
      for (var i = 0; i < _cellObjects.Length; i++) {
        if (_cellObjects[i]) {
          newArray[j] = _cellObjects[i];
          j++;
        }
      }

      _cellObjects = newArray;
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