using System;
using UnityEngine;

namespace World {
  public class ChunkController : MonoBehaviour {
    [SerializeField] private CellObjectsPool _cellObjectsPool;
    [SerializeField] private ChunkGenerator _chunkGenerator;

    private void Awake() {
      _cellObjectsPool.Init();
      _chunkGenerator.Init();
      InitStartChunk();
    }

    void InitStartChunk() {
      Vector3 coords = Vector3.zero;
      for (int k = -_chunkGenerator.SectorsStartRangeX; k <= _chunkGenerator.SectorsStartRangeX; k++) {
        for (int n = 0; n <= _chunkGenerator.SectorsStartRangeX; n++) {
          var startChunk = _chunkGenerator.GetChunk(k, n);
          if(startChunk == null) continue;
          float stepX = 1.32f;
          float stepY = 1.3f;
          coords.x = k * (startChunk.width * stepX);
          coords.y = n * -(startChunk.height * stepY);
          for (int i = 0; i < startChunk.height; i++) {
            for (int j = 0; j < startChunk.width; j++) {
              if (startChunk.GetCellData(i, j).perlin > 0.45f) {
                var cellObject = _cellObjectsPool.GetObject();
                cellObject.transform.position = coords;
              }
              coords.x += stepX;
            }
            coords.x = k * (startChunk.width * stepX);
            coords.y -= stepY;
          }
        }
      }
    }
  }
}