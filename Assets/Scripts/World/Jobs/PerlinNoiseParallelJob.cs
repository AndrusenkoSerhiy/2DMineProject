using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

namespace World.Jobs{
  public struct PerlinNoiseParallelJob : IJobParallelFor{
    public int width;
    public float scale;
    public NativeArray<float> noiseMap;

    public void Execute(int index)
    {
      int x = index % width;
      int y = index / width;

      float xCoord = (float)x / width * scale;
      float yCoord = (float)y / width * scale;
      float sample = Mathf.PerlinNoise(xCoord, yCoord);
      noiseMap[index] = sample;
    }
  }
}