using UnityEngine;

namespace Movement{
  public class LookAtMouse : MonoBehaviour{
    // Define the maximum and minimum angles for rotation
    [SerializeField] private Vector2 MinMaxAngle;
    [SerializeField] private Transform FlipTarget;
    private Camera _camera;
    private bool isFlipped = false;
    private float rotationCoef = 1f;
    private float angleOffset = 80f;

    private void Awake(){
      _camera = Camera.main;
      isFlipped = false;
      rotationCoef = 1f;
    }

    // Update is called once per frame
    void Update(){
      // Get the mouse position in world coordinates
      Vector3 mousePosition = _camera.ScreenToWorldPoint(Input.mousePosition);
      mousePosition.z = 0f;
      //Debug.Log("Mouse pos : "+mousePosition + " | "+Input.mousePosition);
      // Calculate the direction from the sprite to the mouse position
      Vector3 direction = mousePosition - transform.position;
      isFlipped = (FlipTarget.localScale.x < 0f);
      rotationCoef = isFlipped ? -1f : 1f;
      direction.x *= rotationCoef;

      // Calculate the angle between the sprite's forward direction and the direction to the mouse
      float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
      angleOffset = angle > 0 ? 88f : 92f;
      if (angle > MinMaxAngle.y || angle < MinMaxAngle.x) {
        //todo FlipToMouse
        //return;
      }
      // Clamp the angle within the defined bounds
      angle = Mathf.Clamp(angle, MinMaxAngle.x, MinMaxAngle.y);
      // Apply the rotation to the sprite in the Z-axis
      transform.rotation = Quaternion.Euler(0f, 0f, rotationCoef * (angle + angleOffset));
    }
  }
}