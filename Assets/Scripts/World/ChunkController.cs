using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Pool;
using Scriptables;
using Scriptables.POI;
using UnityEngine;
using Utils;

namespace World {
  public class ChunkController : MonoBehaviour {
    [SerializeField] private ChunkGenerator _chunkGenerator;
    [SerializeField] private ResourceDataLibrary _resourceDataLib;
    public ResourceDataLibrary ResourceDataLibrary => _resourceDataLib;

    [SerializeField] private POIDataLibrary _poiDataLibrary;
    public POIDataLibrary POIDataLibrary => _poiDataLibrary;

    private Dictionary<Coords, CellObject> _activeCellObjects = new();
    private ChunkData chunkData;
    public ChunkData ChunkData => chunkData;
    private bool isInited = false;

    [SerializeField] private ObjectPooler testPool;

    private void Awake() {
      getCellObjectsPool().Init();
      _chunkGenerator.Init();
    }

    private void Start() {
      InitStartChunk();
      GameManager.Instance.MapController.GenerateTexture();
    }
    
    void SpawnChunk(int x, int y) {
      var startChunk = _chunkGenerator.GetChunk(x, y);
      if (startChunk == null) return;
      SpawnNearbyCells();
    }

    async Task SpawnNearbyCells() {
      var _proxyCoords = new Coords(-1, -1);
      var playerCoords = GameManager.Instance.PlayerController.PlayerCoords.GetCoords();
      var cols = GameManager.Instance.GameConfig.ChunkSizeX;
      var rows = GameManager.Instance.GameConfig.ChunkSizeY;
      var visionOffsetX = GameManager.Instance.GameConfig.PlayerAreaWidth / 2;
      var visionOffsetY = GameManager.Instance.GameConfig.PlayerAreaHeight / 2;
      var min_x = Mathf.Clamp(playerCoords.X - visionOffsetX, 0, cols - 1);
      var max_x = Mathf.Clamp(playerCoords.X + visionOffsetX, 0, cols - 1);
      var min_y = Mathf.Clamp(playerCoords.Y - visionOffsetY, 0, rows - 1);
      var max_y = Mathf.Clamp(playerCoords.Y + visionOffsetY, 0, rows - 1);
      //var keys = _activeCellObjects.Keys;
      for (var i = min_x; i < max_x; i++) {
        for (var j = min_y; j < max_y; j++) {
          _proxyCoords.X = i;
          _proxyCoords.Y = j;
          _proxyCoords.GetHashCode();
          if (_activeCellObjects.ContainsKey(_proxyCoords)) {
            continue;
          }

          if (chunkData.CellFillDatas[i, j] == 0) {
            continue;
          }

          var cellData = chunkData.GetCellData(i, j);
          var data = _resourceDataLib.GetData(cellData.perlin);
          if (!data.IsBuilding) {
            var pos = CoordsTransformer.GridToWorld(i, j);
            var cell = getCellObjectsPool().Get(pos);
            if (!cell) {
              continue;
            }

            cell.Init(cellData, data);
            cell.InitSprite();
            _activeCellObjects[_proxyCoords] = cell;
          }
          //use to place building
          else {
            var cell = (CellObject)testPool.SpawnFromPool(data.name, Vector3.zero, Quaternion.identity);
            if (!cell) {
              continue;
            }

            cell.gameObject.SetActive(true);
            cell.Init(cellData, data);
            cell.transform.position = CoordsTransformer.GridToWorld(i, j);
            //cell.InitSprite();
            _activeCellObjects[_proxyCoords] = cell;
          }
        }
      }


      isInited = true;
    }

    //get building from another pool
    public CellObject SpawnBuild(Coords coords, ResourceData resourceData) {
      var cell = (CellObject)testPool.SpawnFromPool(resourceData.name, Vector3.zero, Quaternion.identity);
      cell.transform.position = CoordsTransformer.GridToWorld(coords.X, coords.Y);
      var cellData = chunkData.GetCellData(coords.X, coords.Y);
      cell.Init(cellData, resourceData);
      _activeCellObjects[new Coords(coords.X, coords.Y)] = cell;
      return cell;
    }

    private List<Coords> clearList = new();

    public void CheckArea() {
      if (!isInited) return;
      var playerCoords = GameManager.Instance.PlayerController.PlayerCoords.GetCoords();
      var visionOffsetX = GameManager.Instance.GameConfig.PlayerAreaWidth / 2;
      var visionOffsetY = GameManager.Instance.GameConfig.PlayerAreaHeight / 2;
      foreach (var coord in _activeCellObjects.Keys) {
        if (Mathf.Abs(playerCoords.X - coord.X) > visionOffsetX ||
            Mathf.Abs(playerCoords.Y - coord.Y) > visionOffsetY) {
          if (!_activeCellObjects[coord].resourceData.IsBuilding)
            getCellObjectsPool().ReturnObject(_activeCellObjects[coord]);
          else _activeCellObjects[coord].ReturnToPool();
          clearList.Add(coord);
        }
      }

      for (int i = 0; i < clearList.Count; i++) {
        _activeCellObjects.Remove(clearList[i]);
      }

      clearList.Clear();
      SpawnNearbyCells();
    }

    private void RemoveCellFromActives(Coords coords) {
      _activeCellObjects.Remove(coords);
    }

    public void TriggerCellDestroyed(CellObject cellObject) {
      cellObject.CellData.Destroy();
      RemoveCellFromActives(new Coords(cellObject.CellData.x, cellObject.CellData.y));
      var x = cellObject.CellData.x;
      var y = cellObject.CellData.y;
      UpdateCellAround(x, y);
    }

    public void UpdateCellAround(int x, int y) {
      var cellUp = GetCell(x - 1, y);
      var cellDown = GetCell(x + 1, y);
      var cellLeft = GetCell(x, y - 1);
      var cellRight = GetCell(x, y + 1);

      if (cellUp) cellUp.InitSprite();
      if (cellDown) cellDown.InitSprite();
      if (cellLeft) cellLeft.InitSprite();
      if (cellRight) cellRight.InitSprite();
    }

    public CellObject GetCell(int x, int y) {
      var id = new Coords(x, y);
      CellObject result = null;
      _activeCellObjects.TryGetValue(id, out result);
      return result;
    }

    private void InitStartChunk() {
      chunkData = _chunkGenerator.GetChunk(0, 0);
      //fill first row
      var data = ResourceDataLibrary.GetData(0.3f);
      for (int i = 248; i < 258; i++) {
        var cell = chunkData.GetCellData(i, 0);
        if (cell.perlin <= 0.3f) {
          cell.perlin = 0.3f;
          cell.durability = data.Durability;
          chunkData.SetCellFill(i, 0);
        }
      }
      //set first help
      var dataWood = ResourceDataLibrary.GetData(0.3f);
      var cellWood = chunkData.GetCellData(258, 0);
      cellWood.perlin = -1f;
      cellWood.durability = dataWood.Durability;
      
      //POI
      GeneratePOI(chunkData);
      SpawnChunk(0, 0);
    }

    private void GeneratePOI(ChunkData chunkData) {
      //Get all empty points
      var emptyCells = new List<CellData>();
      for (int i = 0; i < chunkData.width; i++) {
        for (int j = 0; j < chunkData.width; j++) {
          if (chunkData.CellFillDatas[i, j] == 0) {
            var data = chunkData.GetCellData(i, j);
            if (data.x == 0 || data.y == 0 || data.x == chunkData.width - 1 || data.y == chunkData.height - 1) continue;
            var cellData = chunkData.GetCellData(i, j);
            if (!cellData.HasStandPoint) continue;
            emptyCells.Add(chunkData.GetCellData(i, j));
          }
        }
      }

      //Get POI variants
      for (int i = 0; i < _poiDataLibrary.POIDataList.Count; i++) {
        for (int j = 0; j < _poiDataLibrary.POIDataList[i].minCount; j++) {
          var randIndex = Random.Range(0, emptyCells.Count);
          var startCell = emptyCells[randIndex];
          for (int k = 0; k < _poiDataLibrary.POIDataList[i].Cells.Count; k++) {
            var targetData = _poiDataLibrary.POIDataList[k].Cells[k];
            if (targetData == null) continue;
            var xCoord = startCell.x + targetData.localX;
            var yCoord = startCell.y + targetData.localY;
            var cell = chunkData.ForceCellFill(targetData.resourceData, xCoord, yCoord);
            if (cell == null) continue;
            emptyCells.Remove(cell);
            if (emptyCells.Count == 0) return;
          }
        }
      }
    }


    private CellObjectsPool getCellObjectsPool() {
      return GameManager.Instance.CellObjectsPool;
    }
  }
}