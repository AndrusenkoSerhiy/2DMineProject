using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace World.Jobs
{
    [BurstCompile]
    public struct POIPlacementWithCheckJob : IJob
    {
        public int width;
        public int height;
        public int poiCount;
        public int minBorder;
        public int radiusX;
        public int radiusY;

        public NativeArray<float> smoothedNoiseMap;
        public NativeArray<int> cellFillDatas;
        public NativeArray<byte> occupied;

        [ReadOnly] public NativeArray<POICellData> allPoiCells;
        [ReadOnly] public NativeArray<int2> templateSizes;
        //public NativeArray<POIInstanceData> placedInstances; // для збереження даних про розміщені POI

        public uint randomSeed;

        public void Execute()
        {
            var rand = new Random(randomSeed);

            for (int instIndex = 0; instIndex < poiCount; instIndex++)
            {
                int templateIndex = rand.NextInt(0, templateSizes.Length);
                int2 size = templateSizes[templateIndex];

                int tryCount = 0;
                bool placed = false;

                while (tryCount < 100 && !placed)
                {
                    int startX = rand.NextInt(minBorder, width - size.x - minBorder);
                    int startY = rand.NextInt(minBorder, height - size.y - minBorder);

                    // Перевірка на перекриття + радіус
                    bool overlaps = false;
                    for (int dx = -radiusX; dx < size.x + radiusX && !overlaps; dx++)
                    {
                        for (int dy = -radiusY; dy < size.y + radiusY; dy++)
                        {
                            int checkX = startX + dx;
                            int checkY = startY + dy;

                            if (checkX < 0 || checkX >= width || checkY < 0 || checkY >= height)
                                continue;

                            int idx = checkX + checkY * width;
                            if (occupied[idx] == 1)
                            {
                                overlaps = true;
                                break;
                            }
                        }
                    }

                    if (!overlaps)
                    {
                        // Позначаємо зайняті клітинки з урахуванням радіусу
                        for (int dx = -radiusX; dx < size.x + radiusX; dx++)
                        {
                            for (int dy = -radiusY; dy < size.y + radiusY; dy++)
                            {
                                int markX = startX + dx;
                                int markY = startY + dy;
                                if (markX < 0 || markX >= width || markY < 0 || markY >= height)
                                    continue;

                                int idx = markX + markY * width;
                                occupied[idx] = 1;
                            }
                        }

                        // Очищаємо місце під весь прямокутник POI
                        for (int dx = 0; dx < size.x; dx++)
                        {
                            for (int dy = 0; dy < size.y; dy++)
                            {
                                int x = startX + dx;
                                int y = startY + dy;
                                if (x < 0 || x >= width || y < 0 || y >= height)
                                    continue;

                                int idx = x + y * width;
                                smoothedNoiseMap[idx] = -10000f; // позначаємо як пустоту
                                cellFillDatas[idx] = 0;
                            }
                        }

                        // Малюємо POI поверх очищеної області
                        for (int i = 0; i < allPoiCells.Length; i++)
                        {
                            var cell = allPoiCells[i];
                            if (cell.templateIndex != templateIndex) continue;

                            int x = startX + cell.localX;
                            int y = startY + cell.localY;
                            if (x < 0 || x >= width || y < 0 || y >= height)
                                continue;

                            int idx = x + y * width;
                            smoothedNoiseMap[idx] = cell.perlin;
                            cellFillDatas[idx] = cell.durability > 0 ? 1 : 0;
                        }

                        placed = true;

                        /*placedInstances[instIndex] = new POIInstanceData
                        {
                          startX = startX,
                          startY = startY,
                          templateIndex = templateIndex
                        };*/
                    }

                    tryCount++;
                }
            }
        }
    }
}
