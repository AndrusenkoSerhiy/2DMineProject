using System;

namespace Utils {
  [Serializable]
  public struct Coords {
    public int X;
    public int Y;

    public Coords(int x, int y) {
      X = x;
      Y = y;
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