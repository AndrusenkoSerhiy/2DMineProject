using System;
using UnityEngine;
using Utils;
using World;

namespace Player {
  public class PlayerCoords : MonoBehaviour {
    public Coords Coords;

    private Vector3 prevPos;

    public Coords GetCoords() {
      if (Coords.X == -1 || Coords.Y == -1) {
        SetCoords();
      }

      return Coords;
    }

    // Update is called once per frame
    void Update() {
      if (Vector3.Distance(transform.position, prevPos) >= GameManager.instance.GameConfig.CheckAreaStep) {
        SetCoords();
        prevPos = transform.position;
      }
    }

    void SetCoords() {
      var coords = CoordsTransformer.WorldToGrid(transform.position);
      Coords.X = coords.X;
      Coords.Y = coords.Y;
      GameManager.instance.ChunkController.CheckArea();
    }
  }
}