using UnityEngine;

namespace Utils {
  public static class Collisions {
    public static bool CheckCircleCollision(Vector2 circleCenter, float circleRadius, Collider2D collider) {
      // Get the collider's bounds
      Bounds bounds = collider.bounds;

      // Find the closest point on the collider to the center of the circle
      float closestX = Mathf.Clamp(circleCenter.x, bounds.min.x, bounds.max.x);
      float closestY = Mathf.Clamp(circleCenter.y, bounds.min.y, bounds.max.y);

      // Calculate the distance between the closest point and the circle's center
      float distanceX = circleCenter.x - closestX;
      float distanceY = circleCenter.y - closestY;

      // Calculate the distance squared (avoid square root for performance)
      float distanceSquared = (distanceX * distanceX) + (distanceY * distanceY);

      // Check if the distance is less than or equal to the circle's radius squared
      return distanceSquared <= (circleRadius * circleRadius);
    }
  }
}