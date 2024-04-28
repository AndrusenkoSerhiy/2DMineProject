using UnityEngine;

namespace Movement {
  public class LookFlip : MonoBehaviour {
    private Camera _camera;
    private Vector3 _right;
    private Vector3 _left;
    private void Awake() {
      _camera = Camera.main;
      _left = _right = Vector3.one;
      _left.x = -1f;
    }

    // Update is called once per frame
    void Update() {
      Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);

      // Flip the sprite based on mouse position
      if (mousePosition.x >= transform.position.x) {
        // Mouse is to the right of the sprite
        //_spriteRenderer.flipX = false; // No flipping
        transform.localScale = _right;
        //Debug.Log("Right");
      }
      else {
        // Mouse is to the left of the sprite
        //_spriteRenderer.flipX = true; // Flip horizontally
        transform.localScale = _left;
        //Debug.Log("Left");
      }
    }
  }
}