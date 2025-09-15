using System;
using System.Collections.Generic;
using SaveSystem;
using Scriptables;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
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

    /*void GenerateNoise() {
      noiseMap = new NativeArray<float>(width * height, Allocator.TempJob);
      var perlinJob = new PerlinNoiseParallelJob {
        width = width,
        height = height,
        scale = GameManager.Instance.GameConfig.PerlinScale,
        seed = GameManager.Instance.ChunkController.Seed,
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
    }*/

    void GenerateNoise()
{
    // 1. Перлін шум
    noiseMap = new NativeArray<float>(width * height, Allocator.TempJob);
    var perlinJob = new PerlinNoiseParallelJob
    {
        width = width,
        height = height,
        scale = GameManager.Instance.GameConfig.PerlinScale,
        seed = GameManager.Instance.ChunkController.Seed,
        noiseMap = noiseMap
    };
    var perlinHandle = perlinJob.Schedule(width * height, 64);

    // 2. Згладжування (Cellular Automata)
    smoothedNoiseMap = new NativeArray<float>(width * height, Allocator.TempJob);
    var caSmoothingJob = new CellularAutomataSmoothingJob
    {
        width = width,
        height = height,
        noiseMap = noiseMap,
        smoothedNoiseMap = smoothedNoiseMap
    };
    var caHandle = caSmoothingJob.Schedule(width * height, 64, perlinHandle);

    // 3. Масиви для результатів та перевірки зайнятих клітинок
    var cellFillNative = new NativeArray<int>(width * height, Allocator.TempJob);
    var occupiedNative = new NativeArray<byte>(width * height, Allocator.TempJob);

    // 4. Підготовка всіх POI клітинок у NativeArray
    var poiLib = GameManager.Instance.POIDataLibrary;
    var allPoiCellsList = new List<POICellData>();
    var templateSizesList = new List<int2>();

    for (int t = 0; t < poiLib.POIDataList.Count; t++)
    {
        var poi = poiLib.POIDataList[t];
        templateSizesList.Add(new int2(poi.SizeX, poi.SizeY));

        foreach (var src in poi.Cells)
        {
            allPoiCellsList.Add(new POICellData
            {
                localX = src.localX,
                localY = src.localY,
                perlin = GameManager.Instance.ChunkController.ResourceDataLibrary.GetPerlinValue(src.resourceData),
                durability = src.resourceData.Durability,
                templateIndex = t,
                sizeX = poi.SizeX,
                sizeY = poi.SizeY
            });
        }
    }

    var allPoiCellsNative = new NativeArray<POICellData>(allPoiCellsList.Count, Allocator.TempJob);
    for (int i = 0; i < allPoiCellsList.Count; i++)
        allPoiCellsNative[i] = allPoiCellsList[i];

    var templateSizesNative = new NativeArray<int2>(templateSizesList.Count, Allocator.TempJob);
    for (int i = 0; i < templateSizesList.Count; i++)
        templateSizesNative[i] = templateSizesList[i];
    
    // 5. Запуск джоба з перевіркою перекриття та розміщенням POI
    var poiJob = new POIPlacementWithCheckJob
    {
        width = width,
        height = height,
        poiCount = poiLib.POICountForGeneration,
        minBorder = 20,
        radiusX = poiLib.RadiusX,
        radiusY = poiLib.RadiusY,
        smoothedNoiseMap = smoothedNoiseMap,
        cellFillDatas = cellFillNative,
        occupied = occupiedNative,
        allPoiCells = allPoiCellsNative,
        templateSizes = templateSizesNative,
        randomSeed = (uint)(GameManager.Instance.ChunkController.Seed ^ (id.X * 73856093) ^ (id.Y * 19349663)),
        //placedInstances = placedInstancesNative
    };
    var poiHandle = poiJob.Schedule(caHandle);
    poiHandle.Complete();

    // 6. Копіюємо cellFillNative у _cellFillDatas
    for (int i = 0; i < width; i++)
    {
        for (int j = 0; j < height; j++)
        {
            _cellFillDatas[i, j] = cellFillNative[i + j * width];
        }
    }

    // 7. Dispose усіх NativeArray
    cellFillNative.Dispose();
    occupiedNative.Dispose();
    allPoiCellsNative.Dispose();
    templateSizesNative.Dispose();
}

    
    void ApplyCells() {
      var chunkController = GameManager.Instance.ChunkController;

      for (var i = 0; i < width; i++) {
        for (var j = 0; j < height; j++) {
          var changed = chunkController.GetChanged(i, j);

          if (changed != null) {
            ApplyChangedCell(i, j, changed);
          }
          else {
            ApplyDefaultOrRemovedCell(i, j, chunkController);
          }
        }
      }
    }

    private void ApplyChangedCell(int i, int j, ChangedCellData changed) {
      _cellDatas[i, j] = new CellData(i, j, changed.Perlin, changed.Durability, this);
      SetCellFill(i, j);
    }

    private void ApplyDefaultOrRemovedCell(int x, int y, ChunkController chunkController) {
      var isRemoved = chunkController.IsRemoved(x, y);
      var index = x + y * width;

      var perlin = smoothedNoiseMap[index];
      var resourceData = isRemoved ? null : chunkController.ResourceDataLibrary.GetData(perlin);
      var durability = resourceData != null ? resourceData.Durability : 0;

      _cellDatas[x, y] = new CellData(x, y, perlin, durability, this);

      if (resourceData != null) {
        SetCellFill(x, y);
      }
    }

    public CellData GetCellData(int xCoord, int yCoord) {
      if (xCoord < 0 || xCoord >= width || yCoord < 0 || yCoord >= height) return null;
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
      if (dataObject == null) return null;
      var cell = GetCellData(x, y);
      cell.perlin = (dataObject.PerlinRange.x + dataObject.PerlinRange.y) / 2;
      cell.alreadyDroped = 0;
      cell.durability = data.Durability;
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