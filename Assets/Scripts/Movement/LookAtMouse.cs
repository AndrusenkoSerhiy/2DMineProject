using UnityEngine;

namespace Movement {
  public class LookAtMouse : MonoBehaviour
  {
    // Define the maximum and minimum angles for rotation
    [SerializeField]private float maxAngle = 45f;
    [SerializeField]private float minAngle = -45f;
    private Camera _camera;
    private float rotationCoef = 1f;
    
    private void Awake() {
      _camera = Camera.main;
      rotationCoef = 1f;
    }
    
    // Update is called once per frame
    void Update()
    {
      // Get the mouse position in world coordinates
      Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);

      // Calculate the direction from the sprite to the mouse position
      Vector3 direction = mousePosition - transform.position;
      rotationCoef = (transform.parent.localScale.x < 0f) ? -1f : 1f;
      direction.x *= rotationCoef;
      // Calculate the angle between the sprite's forward direction and the direction to the mouse
      float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
      // Clamp the angle within the defined bounds
      angle = Mathf.Clamp(angle, minAngle, maxAngle);
      // Apply the rotation to the sprite in the Z-axis
      transform.rotation = Quaternion.Euler(0f, 0f,  rotationCoef * angle);
    }
  }
}