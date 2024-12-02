using System;

namespace Utils {
  [Serializable]
  public class Coords {
    public int X;
    public int Y;

    public Coords(int x, int y) {
      X = x;
      Y = y;
    }

    public override bool Equals(object obj) {
      return Equals(obj as Coords);
    }

    public bool Equals(Coords coords) {
      return coords.X == X && coords.Y == Y;
    }
    
    public override int GetHashCode() {
      unchecked {
        int hash = 17;
        hash = hash * 31 + X.GetHashCode();
        hash = hash * 31 + Y.GetHashCode();
        return hash;
      }
    }
  }
}