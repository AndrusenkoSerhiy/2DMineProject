using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace World.Jobs {
  [BurstCompile]
  public struct CellularAutomataSmoothingJob : IJobParallelFor {
    public int width;
    public int height;
    [ReadOnly] public NativeArray<float> noiseMap; // Original noise map
    public NativeArray<float> smoothedNoiseMap; // Smoothed noise map

    public void Execute(int index) {
      int x = index % width;
      int y = index / width;
      // Get the average of the current cell and its 8 neighbors
      float sum = 0;
      int count = 0;
      
      // Iterate through the 3x3 grid around the cell (itself and its neighbors)
      for (int offsetX = -1; offsetX <= 1; offsetX++) {
        for (int offsetY = -1; offsetY <= 1; offsetY++) {
          int nx = x + offsetX;
          int ny = y + offsetY;
          // Ensure the neighbor is within bounds
          
          if (nx >= 0 && nx < width && ny >= 0 && ny < height) {
            sum += noiseMap[nx + ny * height];
            count++;
          }
        }
      }

      // Calculate the average and assign it to the smoothed map
      smoothedNoiseMap[index] = sum / count;
    }
  }
}