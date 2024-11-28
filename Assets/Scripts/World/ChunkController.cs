using System;
using System.Collections.Generic;
using Game;
using Scriptables;
using UnityEngine;

namespace World {
  public class ChunkController : MonoBehaviour {
    [SerializeField] private ChunkGenerator _chunkGenerator;
    [SerializeField] private ResourceDataLibrary _resourceDataLib;

    private Dictionary<Tuple<int,int>, ChunkObject> _activeChunkObjects = new();

    private void Awake() {
      getCellObjectsPool().Init();
      getChunkObjectsPool().Init();
      _chunkGenerator.Init();
      InitStartChunk();
    }

    void SpawnChunk(int x, int y) {
      Debug.LogError("SpawnChunk");
      var startChunk = _chunkGenerator.GetChunk(x, y);
      if (startChunk == null) return;
      var chunkObject = getChunkObjectsPool().GetObject();
      chunkObject.Init(startChunk);
      chunkObject.name = x + " " + y;
      for (int i = 0; i < startChunk.height; i++) {
        for (int j = 0; j < startChunk.width; j++) {
          var cellData = startChunk.GetCellData(i, j);
          var data = _resourceDataLib.GetData(cellData.perlin);
          if (data) {
            startChunk.SetCellFill(i, j);
          }
        }
      }

      _activeChunkObjects[new Tuple<int, int>(x, y)] = chunkObject;
      SpawnNearbyCells();
    }

    void SpawnNearbyCells() {
      var chunkObject = _activeChunkObjects[new Tuple<int, int>(0, 0)];
      var playerCoords = GameManager.instance.PlayerController.PlayerCoords.GetCoords;
      var cols = GameManager.instance.GameConfig.ChunkSizeX;
      var rows = GameManager.instance.GameConfig.ChunkSizeY;
      var visionOffsetX = GameManager.instance.GameConfig.PlayerAreaWidth/2;
      var visionOffsetY = GameManager.instance.GameConfig.PlayerAreaHeight/2;
      int min_x = Mathf.Clamp(playerCoords.Item1 -visionOffsetX,0,cols-1);
      int max_x = Mathf.Clamp(playerCoords.Item1 + visionOffsetX,0,cols-1);
      int min_y = Mathf.Clamp(playerCoords.Item2 - visionOffsetY,0,rows-1);
      int max_y = Mathf.Clamp(playerCoords.Item2 + visionOffsetY,0,rows-1);
      var count = 0;
      for (int i = min_x; i < max_x; i++) {
        for (int j = min_y; j < max_y; j++) {
          count++;
          if (chunkObject.ChunkData.CellFillDatas[i, j] == 0) continue;
          var pos = CoordsTransformer.GridToWorld(i, j);
          var cell = getCellObjectsPool().Get(pos);
          if (!cell) continue;
          var cellData = chunkObject.ChunkData.GetCellData(i, j);
          var data = _resourceDataLib.GetData(cellData.perlin);
          cell.Init(cellData,data,chunkObject);
          cell.InitSprite();
        }
      }
      Debug.LogError("COunt : "+count);
    }

    void InitStartChunk() {
      SpawnChunk(0, 0);
    }

    private CellObjectsPool getCellObjectsPool() {
      return GameManager.instance.cellObjectsPool;
    }

    private ChunkObjectsPool getChunkObjectsPool() {
      return GameManager.instance.chunkObjectsPool;
    }
  }
}