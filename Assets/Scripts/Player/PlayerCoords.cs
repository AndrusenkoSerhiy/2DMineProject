using UnityEngine;
using World;

namespace Player {
  public class PlayerCoords : MonoBehaviour {
    public int x;
    public int y;

    public float step = 1f;

    private Vector3 prevPos;

    public (int, int) GetCoords => SetCoords();

    // Update is called once per frame
    void Update() {
      if (Vector3.Distance(transform.position, prevPos) >= step) {
        SetCoords();
        prevPos = transform.position;
      }
    }

    (int, int) SetCoords() {
      var coords = CoordsTransformer.WorldToGrid(transform.position);
      x = coords.col;
      y = coords.row;
      return coords;
    }
  }
}