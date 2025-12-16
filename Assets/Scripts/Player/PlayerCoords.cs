using System;
using UnityEngine;
using Utils;
using World;

namespace Player {
  public class PlayerCoords : MonoBehaviour {
    public Coords Coords;
    [SerializeField] private Transform trForGrid;
    private Coords tempCoords;
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
      CoordsTransformer.WorldToGrid(trForGrid.position, ref tempCoords);
      
      if (tempCoords.X == lastCoords.x && tempCoords.Y == lastCoords.y)
        return;
      //Debug.LogError($"Update coords {coords.X} | {coords.Y}");
      lastCoords.x = tempCoords.X;
      lastCoords.y = tempCoords.Y;
      
      Coords = tempCoords;
      GameManager.Instance.ChunkController.CheckArea();
    }
  }
}