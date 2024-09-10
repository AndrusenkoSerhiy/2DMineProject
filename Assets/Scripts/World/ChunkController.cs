using System;
using Game;
using Scriptables;
using UnityEngine;

namespace World {
  public class ChunkController : MonoBehaviour {
    [SerializeField] private ChunkGenerator _chunkGenerator;
    [SerializeField] private ResourceDataLibrary _resourceDataLib;

    private void Awake() {
      getCellObjectsPool().Init();
      _chunkGenerator.Init();
      InitStartChunk();
    }

    void InitStartChunk() {
      Vector3 coords = Vector3.zero;
      for (int k = -_chunkGenerator.SectorsStartRangeX; k <= _chunkGenerator.SectorsStartRangeX; k++) {
        for (int n = 0; n <= _chunkGenerator.SectorsStartRangeX; n++) {
          var startChunk = _chunkGenerator.GetChunk(k, n);
          if (startChunk == null) continue;
          var go = new GameObject();
          go.name = k + " " + n;
          float stepX = 1.32f;
          float stepY = 1.3f;
          coords.x = k * (startChunk.width * stepX);
          coords.y = n * -(startChunk.height * stepY);
          for (int i = 0; i < startChunk.height; i++) {
            for (int j = 0; j < startChunk.width; j++) {
              var data = _resourceDataLib.GetData(startChunk.GetCellData(i, j).perlin);
              if (data /* > 0.45f*/) {
                var cellObject = getCellObjectsPool().GetObject();
                cellObject.transform.position = coords;
                cellObject.transform.SetParent(go.transform);
                cellObject.Init(data);
              }

              coords.x += stepX;
            }

            coords.x = k * (startChunk.width * stepX);
            coords.y -= stepY;
          }
        }
      }
    }

    private CellObjectsPool getCellObjectsPool() {
      return GameManager.instance.cellObjectsPool;
    }
  }
}