using System;

namespace World{
  [Serializable]
  public class CellData{
    public int x;
    public int y;
    public float perlin;

    public CellData(int x, int y, float perlin){
      this.x = x;
      this.y = y;
      this.perlin = perlin;
    }
  }
}