using System;
using System.Collections.Generic;
using UnityEngine;

namespace World{
  [Serializable]
  public class ChunkData{
    public int id;
    public int x;
    public int y;
    public int width = 20;
    public int height = 10;
    private CellData[,] _cellDatas;
    [SerializeField] private List<CellData> debugList = new ();

    public ChunkData(int id, int x, int y){
      this.id = id;
      this.x = x;
      this.y = y;
      _cellDatas = new CellData[width, height];
      for (int i = 0; i < width; i++){
        for (int j = 0; j < height; j++){
          float xCoord = (float)x / width * 5f;
          float yCoord = (float)y / height * 5f;
          var perlin = Mathf.PerlinNoise(xCoord, yCoord);
          _cellDatas[i, j] = new CellData(i, j, perlin);
          debugList.Add(_cellDatas[i, j]);
        }
      }
    }
  }
}