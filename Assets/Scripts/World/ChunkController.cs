using System;
using Game;
using Scriptables;
using UnityEngine;
using UnityEngine.Profiling;

namespace World {
  public class ChunkController : MonoBehaviour {
    [SerializeField] private ChunkGenerator _chunkGenerator;
    [SerializeField] private ResourceDataLibrary _resourceDataLib;

    private ChunkObject[] _activeChunkObjects;

    private void Awake() {
      getCellObjectsPool().Init();
      getChunkObjectsPool().Init();
      _chunkGenerator.Init();
      InitStartChunk();
      InitCellFill();
    }

    void InitStartChunk() {
      Vector3 coords = Vector3.zero;
      for (int k = 0; k <= _chunkGenerator.SectorsStartRangeX; k++) {
        for (int n = 0; n <= _chunkGenerator.SectorsStartRangeX; n++) {
          var startChunk = _chunkGenerator.GetChunk(k, n);
          if (startChunk == null) continue;
          var chunkObject = getChunkObjectsPool().GetObject();
          chunkObject.Init(startChunk);
          chunkObject.name = k + " " + n;
          AddChunkObject(chunkObject);
          float stepX = 1.32f;
          float stepY = 1.3f;
          coords.x = k * (startChunk.width * stepX);
          coords.y = n * -(startChunk.height * stepY);
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

            coords.x = k * (startChunk.width * stepX);
            coords.y -= stepY;
          }
        }
      }
    }

    void InitCellFill() {
      for (int i = 0; i < _activeChunkObjects.Length; i++) {
        _activeChunkObjects[i].FillCells();
      }
    }

    private void AddChunkObject(ChunkObject chunkObject) {
      ResizeChunksArray();
      _activeChunkObjects[_activeChunkObjects.Length - 1] = chunkObject;
    }

    private void ResizeChunksArray() {
      var justCreated = _activeChunkObjects == null;
      var newSize = justCreated ? 1 : _activeChunkObjects.Length + 1;
      var newArray = new ChunkObject[newSize];

      if (!justCreated)
        for (var i = 0; i < _activeChunkObjects.Length; i++) {
          newArray[i] = _activeChunkObjects[i];
        }

      _activeChunkObjects = newArray;
    }

    private CellObjectsPool getCellObjectsPool() {
      return GameManager.instance.cellObjectsPool;
    }

    private ChunkObjectsPool getChunkObjectsPool() {
      return GameManager.instance.chunkObjectsPool;
    }
  }
}