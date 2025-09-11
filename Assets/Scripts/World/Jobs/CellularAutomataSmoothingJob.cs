using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

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

      // 1. усереднюємо сусідів
      float sum = 0;
      int count = 0;

      for (int offsetX = -1; offsetX <= 1; offsetX++) {
        for (int offsetY = -1; offsetY <= 1; offsetY++) {
          int nx = x + offsetX;
          int ny = y + offsetY;

          if (nx >= 0 && nx < width && ny >= 0 && ny < height) {
            sum += noiseMap[nx + ny * width];
            count++;
          }
        }
      }

      smoothedNoiseMap[index] = sum / count;
    }
  }
}