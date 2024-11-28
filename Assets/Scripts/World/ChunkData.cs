using System;
using Game;
using Unity.Collections;
using Unity.Jobs;
using World.Jobs;

namespace World {
  [Serializable]
  public class ChunkData {
    public Tuple<int, int> id;
    public int x;
    public int y;
    private CellData[,] _cellDatas;
    private int[,] _cellFillDatas;
    public int[,] CellFillDatas => _cellFillDatas;
    private NativeArray<float> noiseMap;
    private NativeArray<float> smoothedNoiseMap;
    
    public int width => GameManager.instance.GameConfig.ChunkSizeX;
    public int height => GameManager.instance.GameConfig.ChunkSizeY;

    public ChunkData(Tuple<int, int> id, int x, int y) {
      this.id = id;
      this.x = x;
      this.y = y;
      _cellDatas = new CellData[height, width];
      _cellFillDatas = new int[height, width];
      for (int i = 0; i < height; i++) {
        for (int j = 0; j < width; j++) {
          _cellFillDatas[i, j] = 0;
        }
      }

      GenerateNoise();
      ApplyCells();
      noiseMap.Dispose();
      smoothedNoiseMap.Dispose();
    }

    void GenerateNoise() {
      noiseMap = new NativeArray<float>(width * height, Allocator.TempJob);
      float randomSeed = UnityEngine.Random.Range(0f, 10000f);
      PerlinNoiseParallelJob perlinJob = new PerlinNoiseParallelJob {
        width = width,
        height = height,
        scale = GameManager.instance.GameConfig.PerlinScale,
        seed = randomSeed,
        noiseMap = noiseMap
      };

      JobHandle perlinHandle = perlinJob.Schedule(width * height, 64);
      perlinHandle.Complete();

      smoothedNoiseMap = new NativeArray<float>(width * height, Allocator.Persistent);

      CellularAutomataSmoothingJob caSmoothingJob = new CellularAutomataSmoothingJob {
        width = width,
        height = height,
        noiseMap = noiseMap,
        smoothedNoiseMap = smoothedNoiseMap
      };

      JobHandle caHandle = caSmoothingJob.Schedule(width * height, 64, perlinHandle);
      caHandle.Complete();
    }

    void ApplyCells() {
      for (int i = 0; i < height; i++) {
        for (int j = 0; j < width; j++) {
          var perlin = smoothedNoiseMap[i + j * height];
          _cellDatas[i, j] = new CellData(i, j, perlin, this);
        }
      }
    }

    public CellData GetCellData(int x, int y) {
      return _cellDatas[x, y];
    }

    public void SetCellFill(int x, int y, int value = 1) {
      _cellFillDatas[x, y] = value;
    }

    void OnDestroy() {
      // Dispose of NativeArrays when done
      if (noiseMap.IsCreated) noiseMap.Dispose();
      if (smoothedNoiseMap.IsCreated) smoothedNoiseMap.Dispose();
    }
  }
}