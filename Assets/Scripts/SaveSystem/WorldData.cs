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
    public List<string> RemovedCells = new(); //x_y format
    public SerializedDictionary<string, ChangedCellData> ChangedCells = new();

    public static string GetCellKey(int x, int y) {
      return $"{x}_{y}";
    }
  }
}