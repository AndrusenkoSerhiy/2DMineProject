using System;
using Scriptables;
using Unity.Collections;
using Unity.Jobs;
using Utils;
using World.Jobs;

namespace World {
  [Serializable]
  public class ChunkData {
    //Chunk data info
    public Coords id;
    public int x;
    public int y;
    //All cells data
    private CellData[,] _cellDatas;
    //Cells fill data array
    private int[,] _cellFillDatas;
    public int[,] CellFillDatas => _cellFillDatas;
    //Generation job arrays
    private NativeArray<float> noiseMap;
    private NativeArray<float> smoothedNoiseMap;

    public int width => GameManager.Instance.GameConfig.ChunkSizeX;
    public int height => GameManager.Instance.GameConfig.ChunkSizeY;

    public ChunkData(Coords id, int x, int y) {
      this.id = id;
      this.x = x;
      this.y = y;
      _cellDatas = new CellData[width, height];
      _cellFillDatas = new int[width, height];
      for (var i = 0; i < width; i++) {
        for (var j = 0; j < height; j++) {
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
      var randomSeed = UnityEngine.Random.Range(0f, 10000f);
      var perlinJob = new PerlinNoiseParallelJob {
        width = width,
        height = height,
        scale = GameManager.Instance.GameConfig.PerlinScale,
        seed = randomSeed,
        noiseMap = noiseMap
      };

      var perlinHandle = perlinJob.Schedule(width * height, 64);
      perlinHandle.Complete();

      smoothedNoiseMap = new NativeArray<float>(width * height, Allocator.Persistent);

      var caSmoothingJob = new CellularAutomataSmoothingJob {
        width = width,
        height = height,
        noiseMap = noiseMap,
        smoothedNoiseMap = smoothedNoiseMap
      };

      var caHandle = caSmoothingJob.Schedule(width * height, 64, perlinHandle);
      caHandle.Complete();
    }

    void ApplyCells() {
      for (var i = 0; i < width; i++) {
        for (var j = 0; j < height; j++) {
          var perlin = smoothedNoiseMap[i + j * width];
          _cellDatas[i, j] = new CellData(i, j, perlin, this);
          var data = GameManager.Instance.ChunkController.ResourceDataLibrary.GetData(perlin);
          if (data) {
            SetCellFill(i, j);
          }
        }
      }
    }

    public CellData GetCellData(int xCoord, int yCoord) {
      return _cellDatas[xCoord, yCoord];
    }

    public void SetCellFill(int xCoord, int yCoord, int value = 1) {
      _cellFillDatas[xCoord, yCoord] = value;
    }

    public int GetCellFill(int x, int y, float perlin) {
      if (x < 0 || x > width || y < 0 || y > height) return 0;
      var targetRes = GameManager.Instance.ChunkController.ResourceDataLibrary.GetData(perlin);
      var curRes = GameManager.Instance.ChunkController.ResourceDataLibrary.GetData(_cellDatas[x, y].perlin);
      var sameResource = targetRes != null && curRes != null && targetRes.SortingOrder() == curRes.SortingOrder();
      return sameResource ? _cellFillDatas[x, y] : 0;
    }

    public int GetCellFill(int x, int y) {
      if (x < 0 || x > width || y < 0 || y > height) return 0;
      return _cellFillDatas[x, y];
    }

    public CellData ForceCellFill(ResourceData data, int x, int y) {
      if (!data) return null;
      var dataObject = GameManager.Instance.ChunkController.ResourceDataLibrary.GetDataObject(data);
      if(dataObject == null) return null;
      var cell = GetCellData(x, y);
      cell.perlin = (dataObject.PerlinRange.x + dataObject.PerlinRange.y) / 2;
      SetCellFill(cell.x, cell.y);
      return cell;
    }

    private void OnDestroy() {
      // Dispose of NativeArrays when done
      if (noiseMap.IsCreated) noiseMap.Dispose();
      if (smoothedNoiseMap.IsCreated) smoothedNoiseMap.Dispose();
    }
  }
}