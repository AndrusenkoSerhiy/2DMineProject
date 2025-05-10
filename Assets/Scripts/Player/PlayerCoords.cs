using System;
using UnityEngine;
using Utils;
using World;

namespace Player {
  public class PlayerCoords : MonoBehaviour {
    public Coords Coords;
    [SerializeField] private Transform trForGrid;
    //private Vector3 prevPos;

    public Coords GetCoords() {
      if (Coords.X == -1 || Coords.Y == -1) {
        SetCoords();
      }

      return Coords;
    }

    public Coords GetCoordsOutOfBounds() {
      return CoordsTransformer.MouseToGridPosition(trForGrid.position);
    }
    
    private void Update() {
      //if (Vector3.Distance(transform.position, prevPos) >= GameManager.instance.GameConfig.CheckAreaStep) {
        SetCoords();
        //prevPos = transform.position;
      //}
    }

    private void SetCoords() {
      var coords = CoordsTransformer.WorldToGrid(trForGrid.position);
      Coords.X = coords.X;
      Coords.Y = coords.Y;
      GameManager.Instance.ChunkController.CheckArea();
    }
  }
}