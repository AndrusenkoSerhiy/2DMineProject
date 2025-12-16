using System;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace SaveSystem {
  [Serializable]
  public class BuildingData {
    public int X;
    public int Y;
    public string BuildId;
  }

  [Serializable]
  public class CellFill {
    public int X;
    public int Y;
    public int Value;
  }

  [Serializable]
  public class ChangedCellData {
    public float Perlin;
    public float Durability;
  }

  [Serializable]
  public class WorldData {
    public int Seed = -1;
    public List<BuildingData> BuildDatas = new();

    public List<CellFill> BuildFillDatas = new();
    public List<int> RemovedCells = new();
    public SerializedDictionary<int, ChangedCellData> ChangedCells = new();

    public static int GetCellIndex(int x, int y, int width) {
      return x + y * width;
    }
  }
}