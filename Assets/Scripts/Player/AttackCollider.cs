using Settings;
using UnityEngine;

namespace Player {
  public class AttackCollider : MonoBehaviour {
    [SerializeField] protected BoxCollider2D attackCollider;
    [SerializeField] protected Transform attackTransform;
    [SerializeField] private Transform colliderTR;
    [SerializeField] private float minDistance = 2f;
    [SerializeField] private float maxDistance = 5f;
    protected virtual void Update() {
      UpdateColliderPos();
    }
    
    private void UpdateColliderPos() {
      var mousePos = GetMousePosition();
      // Calculate direction and distance from parent
      var parentPosition = attackTransform.position;
      var direction = (mousePos - parentPosition).normalized;
      var distance = Vector3.Distance(parentPosition, mousePos);
      // Clamp the distance between minDistance and maxDistance
      var clampedDistance = Mathf.Clamp(distance, minDistance, maxDistance);
      // Set the new position of the child collider
      var newPosition = parentPosition + direction * clampedDistance;
      newPosition.z = 0f;
      colliderTR.position = newPosition;
    }

    protected void UpdateParams(float minDist, float maxDist, float sizeX, float sizeY) {
      minDistance = minDist;
      maxDistance = maxDist;
      attackCollider.size = new Vector2(sizeX, sizeY);
    }
    
    private Vector3 GetMousePosition() {
      var mousePos = GameManager.instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
      mousePos.z = 0f;
      return mousePos;
    }
  }
}