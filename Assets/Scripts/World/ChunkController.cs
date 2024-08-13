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
      Vector3 coords = Vector3.zero;
      for (int k = 0; k < 1; k++){
        for (int n = 0; n < 1; n++){
          var startChunk = _chunkGenerator.GetChunk(k, n);
          float stepX = 1.32f;
          float stepY = 1.3f;
          coords.x = k * (startChunk.width * stepX);
          coords.y = n * -(startChunk.height * stepY);
          for (int i = 0; i < startChunk.width; i++){
            for (int j = 0; j < startChunk.height; j++){
              if (startChunk.GetCellData(i, j).perlin > 0.45f){
                var cellObject = _cellObjectsPool.GetObject();
                cellObject.transform.position = coords;
              }

              coords.x += stepX;
            }

            coords.x = k * startChunk.width * stepX;
            coords.y -= stepY;
          }
        }
      }
    }
  }
}