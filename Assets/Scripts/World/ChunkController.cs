using System;
using System.Collections.Generic;
using Game;
using Scriptables;
using UnityEngine;

namespace World {
  public class ChunkController : MonoBehaviour {
    [SerializeField] private ChunkGenerator _chunkGenerator;
    [SerializeField] private ResourceDataLibrary _resourceDataLib;

    [Header("START SPAWN PARAMS")] [SerializeField]
    private int StartSpawnSectorsX = 1;

    [SerializeField] private int StartSpawnSectorsY = 1;

    private List<ChunkObject> _activeChunkObjects = new();
    private float stepX = 1.32f;
    private float stepY = 1.3f;

    private void Awake() {
      getCellObjectsPool().Init();
      getChunkObjectsPool().Init();
      _chunkGenerator.Init();
      InitStartChunk();
      InitCellFill();
    }
    /*private void Update() {
      if (Input.GetKeyDown(KeyCode.F1)) {
        SpawnChunk(5,5);
        for (int i = 0; i < _activeChunkObjects.Count; i++) {
          if (_activeChunkObjects[i].ChunkData.x == 5 && _activeChunkObjects[i].ChunkData.y == 5) {
            _activeChunkObjects[i].FillCells();
            break;
          }
        }
      }
    }*/

    void SpawnChunk(int x, int y) {
      var startChunk = _chunkGenerator.GetChunk(x, y);
      if (startChunk == null) return;
      var chunkObject = getChunkObjectsPool().GetObject();
      chunkObject.Init(startChunk);
      chunkObject.name = x + " " + y;
      AddChunkObject(chunkObject);
      var coords = Vector2.zero;
      coords.x = x * (startChunk.width * stepX);
      coords.y = y * -(startChunk.height * stepY);
      for (int i = 0; i < startChunk.height; i++) {
        for (int j = 0; j < startChunk.width; j++) {
          var cellData = startChunk.GetCellData(i, j);
          var data = _resourceDataLib.GetData(cellData.perlin);
          if (data) {
            var cellObject = getCellObjectsPool().GetObject();
            cellObject.transform.position = coords;
            cellObject.transform.SetParent(chunkObject.transform);
            cellObject.Init(cellData, data, chunkObject);
            startChunk.SetCellFill(i, j);
            chunkObject.AddCellObject(cellObject);
          }

          coords.x += stepX;
        }

        coords.x = x * (startChunk.width * stepX);
        coords.y -= stepY;
      }
    }

    void InitStartChunk() {
      for (int k = 0; k <= StartSpawnSectorsX; k++) {
        for (int n = 0; n <= StartSpawnSectorsY; n++) {
          SpawnChunk(k, n);
        }
      }
    }

    void InitCellFill() {
      for (int i = 0; i < _activeChunkObjects.Count; i++) {
        _activeChunkObjects[i].FillCells();
      }
    }

    private void AddChunkObject(ChunkObject chunkObject) {
      _activeChunkObjects.Add(chunkObject);
    }

    private CellObjectsPool getCellObjectsPool() {
      return GameManager.instance.cellObjectsPool;
    }

    private ChunkObjectsPool getChunkObjectsPool() {
      return GameManager.instance.chunkObjectsPool;
    }
  }
}