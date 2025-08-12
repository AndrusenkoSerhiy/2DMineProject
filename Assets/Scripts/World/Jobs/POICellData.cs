using Unity.Mathematics;

namespace World.Jobs
{
  public struct POICellData
  {
    public int localX;
    public int localY;
    public float perlin;
    public int durability;
    public int templateIndex;
    public int sizeX;
    public int sizeY;
  }

  public struct POIInstanceData
  {
    public int startX;
    public int startY;
    public int templateIndex;
  }
}