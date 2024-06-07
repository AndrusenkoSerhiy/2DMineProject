using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using World.Jobs;

namespace World{
  [Serializable]
  public class ChunkData{
    public int id;
    public int x;
    public int y;
    public int width = 20;
    public int height = 10;
    private CellData[,] _cellDatas;
    [SerializeField] private List<CellData> debugList = new();

    public ChunkData(int id, int x, int y){
      this.id = id;
      this.x = x;
      this.y = y;
      _cellDatas = new CellData[width, height];
      GenerateNoise();
    }

    void GenerateNoise(){
      NativeArray<float> noiseMap = new NativeArray<float>(width * height, Allocator.TempJob);

      PerlinNoiseParallelJob noiseJob = new PerlinNoiseParallelJob{
        width = width,
        scale = 25f,
        noiseMap = noiseMap
      };

      JobHandle jobHandle = noiseJob.Schedule(width * height, 64);
      jobHandle.Complete();

      for (int i = 0; i < width; i++){
        for (int j = 0; j < height; j++){
          var perlin = noiseMap[i + j * width];
          _cellDatas[i, j] = new CellData(i, j, perlin);
          debugList.Add(_cellDatas[i, j]);
        }
      }

      noiseMap.Dispose();
    }
  }
}