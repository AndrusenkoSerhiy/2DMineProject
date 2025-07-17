using System;
using UnityEngine;
using Utils;
using World;

namespace Player {
  public class PlayerCoords : MonoBehaviour {
    public Coords Coords;
    [SerializeField] private Transform trForGrid;
    private Vector2Int lastCoords;

    public Vector3 GetPosition() {
      return trForGrid.position;
    }

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
      SetCoords();
    }

    private void SetCoords() {
      var coords = CoordsTransformer.WorldToGrid(trForGrid.position);
      
      if (coords.X == lastCoords.x && coords.Y == lastCoords.y)
        return;
      //Debug.LogError($"Update coords {coords.X} | {coords.Y}");
      lastCoords.x = coords.X;
      lastCoords.y = coords.Y;
      
      Coords.X = coords.X;
      Coords.Y = coords.Y;
      GameManager.Instance.ChunkController.CheckArea();
    }
  }
}