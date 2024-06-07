using System;
using UnityEngine;

namespace World{
  public class ChunkController : MonoBehaviour{
    [SerializeField] private CellObjectsPool _cellObjectsPool;
    [SerializeField] private ChunkGenerator _chunkGenerator;

    private void Awake(){
      _cellObjectsPool.Init();
      _chunkGenerator.Init();
      InitStartChunk();
    }

    void InitStartChunk(){
      for (int k = 0; k < 3; k++){
        for (int n = 0; n < 3; n++){
          var startChunk = _chunkGenerator.GetChunk(k, n);
          float stepX = 16f;
          float stepY = 16f;
          Vector3 coords = Vector3.zero;
          coords.x = k * 1600f;
          for (int i = 0; i < startChunk.width; i++){
            for (int j = 0; j < startChunk.height; j++){
              if (startChunk.GetCellData(i, j).perlin > 0.4f){
                var cellObject = _cellObjectsPool.GetObject();
                cellObject.transform.position = coords;
              }

              coords.x += stepX;
            }
            coords.x = k * 1600f;
            coords.y -= stepY;
          }
        }
      }

    }
  }
}