using UnityEngine;
using Utils;
using World;

namespace Enemy {
  public class EnemyCoords : MonoBehaviour {
    public Coords Coords;
    [SerializeField] private Transform trForGrid;
    private Coords tempCoords;
    
    public Coords GetCoords() {
      if (Coords.X == -1 || Coords.Y == -1) {
        SetCoords();
      }

      return Coords;
    }
    
    private void Update() {
      SetCoords();
    }
    
    public Coords GetCoordsOutOfBounds() {
      return CoordsTransformer.MouseToGridPosition(trForGrid.position);
    }

    private void SetCoords() {
      CoordsTransformer.WorldToGrid(trForGrid.position, ref tempCoords);
      //var coords = CoordsTransformer.MouseToGridPosition(trForGrid.position);
      Coords = tempCoords;
    }
  }
}