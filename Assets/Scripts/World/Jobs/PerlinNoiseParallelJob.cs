using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Burst;

namespace World.Jobs{
  [BurstCompile]
  public struct PerlinNoiseParallelJob : IJobParallelFor{
    public int width;
    public int height;
    public float scale;
    public float seed;
    public NativeArray<float> noiseMap;

    public void Execute(int index)
    {
      int x = index % width;
      int y = index / height;
      float xCoord = (float)x / width * scale + seed;
      float yCoord = (float)y / height * scale + seed;
      
      float sample = Mathf.PerlinNoise(xCoord, yCoord);
      noiseMap[index] = sample;
    }
  }
}