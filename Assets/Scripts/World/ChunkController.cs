using System.Collections.Generic;
using System.IO;
using Scriptables;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

namespace World {
  public class ChunkController : MonoBehaviour {
    [SerializeField] private ChunkGenerator _chunkGenerator;
    [SerializeField] private ResourceDataLibrary _resourceDataLib;
    public ResourceDataLibrary ResourceDataLibrary => _resourceDataLib;

    private Dictionary<Coords, CellObject> _activeCellObjects = new();
    private ChunkData chunkData;
    private bool isInited = false;

    private void Awake() {
      getCellObjectsPool().Init();
      _chunkGenerator.Init();
    }

    private void Start() {
      InitStartChunk();
    }

    void SpawnChunk(int x, int y) {
      var startChunk = _chunkGenerator.GetChunk(x, y);
      if (startChunk == null) return;
      SpawnNearbyCells();
    }


    #region TextureMap
    [ContextMenu("Create TextureMap In Assets(PLAYMODE ONLY)")]
    private void GenerateTexture() {
      var width = GameManager.instance.GameConfig.ChunkSizeX;
      var height = GameManager.instance.GameConfig.ChunkSizeY;
      Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
      for (int i = 0; i < width; i++) {
        for (int j = 0; j < height; j++) {
          // Check the Perlin noise value
          Color color = GameManager.instance.ChunkController.ResourceDataLibrary.GetColor(chunkData.GetCellData(i, j).perlin);

          // Set the pixel color at (j, i)
          texture.SetPixel(j, i, color);
        }
      }

      // Apply all SetPixel changes to the texture
      texture.Apply();

      byte[] pngData = texture.EncodeToPNG();
      if (pngData != null) {
        // Write the PNG file to the specified path
        File.WriteAllBytes("Assets/GeneratedTexture.png", pngData);
        Debug.Log("Texture saved");
      }
      else {
        Debug.LogError("Failed to encode texture to PNG.");
      }
    }

    #endregion

    void SpawnNearbyCells() {
      var playerCoords = GameManager.instance.PlayerController.PlayerCoords.GetCoords();
      var cols = GameManager.instance.GameConfig.ChunkSizeX;
      var rows = GameManager.instance.GameConfig.ChunkSizeY;
      var visionOffsetX = GameManager.instance.GameConfig.PlayerAreaWidth / 2;
      var visionOffsetY = GameManager.instance.GameConfig.PlayerAreaHeight / 2;
      var min_x = Mathf.Clamp(playerCoords.X - visionOffsetX, 0, cols - 1);
      var max_x = Mathf.Clamp(playerCoords.X + visionOffsetX, 0, cols - 1);
      var min_y = Mathf.Clamp(playerCoords.Y - visionOffsetY, 0, rows - 1);
      var max_y = Mathf.Clamp(playerCoords.Y + visionOffsetY, 0, rows - 1);
      var keys = _activeCellObjects.Keys;
      for (var i = min_x; i < max_x; i++) {
        for (var j = min_y; j < max_y; j++) {
          if (_activeCellObjects.ContainsKey(new Coords(i, j))) {
            continue;
          }

          if (chunkData.CellFillDatas[i, j] == 0) {
            continue;
          }

          var pos = CoordsTransformer.GridToWorld(i, j);
          var cell = getCellObjectsPool().Get(pos);
          if (!cell) {
            continue;
          }

          var cellData = chunkData.GetCellData(i, j);
          var data = _resourceDataLib.GetData(cellData.perlin);
          cell.Init(cellData, data);
          cell.InitSprite();
          _activeCellObjects[new Coords(i, j)] = cell;
        }
      }

      isInited = true;
    }
    
    //player place this cell
    public void SpawnCell(Coords coords, ResourceData resourceData) {
      //if cell already exist don't spawn another
      if (GetCell(coords.X, coords.Y))
        return;
      
      var pos = CoordsTransformer.GridToWorld(coords.X, coords.Y);
      var cell = getCellObjectsPool().Get(pos);
      if (!cell) 
        return;

      var cellData = chunkData.GetCellData(coords.X, coords.Y);
      cell.Init(cellData, resourceData);
      cell.InitSprite();
      _activeCellObjects[new Coords(coords.X, coords.Y)] = cell;
    }

    private List<Coords> clearList = new();

    public void CheckArea() {
      if (!isInited) return;
      var playerCoords = GameManager.instance.PlayerController.PlayerCoords.GetCoords();
      var visionOffsetX = GameManager.instance.GameConfig.PlayerAreaWidth / 2;
      var visionOffsetY = GameManager.instance.GameConfig.PlayerAreaHeight / 2;
      foreach (var coord in _activeCellObjects.Keys) {
        if (Mathf.Abs(playerCoords.X - coord.X) > visionOffsetX ||
            Mathf.Abs(playerCoords.Y - coord.Y) > visionOffsetY) {
          getCellObjectsPool().ReturnObject(_activeCellObjects[coord]);
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
      RemoveCellFromActives(new Coords(cellObject.CellData.x,cellObject.CellData.y));
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

    public CellObject GetCell(int x, int y) {
      var id = new Coords(x, y);
      CellObject result = null;
      _activeCellObjects.TryGetValue(id, out result);
      return result;
    }

    private void InitStartChunk() {
      chunkData = _chunkGenerator.GetChunk(0, 0);
      SpawnChunk(0, 0);
      GeneratePOI(chunkData);
    }

    private void GeneratePOI(ChunkData chunkData) {
      //Get all empty points
      var emptyCells = new List<CellData>();
      for (int i = 0; i < chunkData.width; i++) {
        for (int j = 0; j < chunkData.width; j++) {
          if (chunkData.CellFillDatas[i, j] == 0) {
            emptyCells.Add(chunkData.GetCellData(i, j));
          }
        }
      }
      //Get POI variants
    }

    private CellObjectsPool getCellObjectsPool() {
      return GameManager.instance.CellObjectsPool;
    }
  }
}