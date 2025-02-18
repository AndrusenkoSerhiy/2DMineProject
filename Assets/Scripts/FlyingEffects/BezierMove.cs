using UnityEngine;

public class BezierMove : MonoBehaviour
{
  public Transform pointA;
  public Transform pointB;
  private Vector3 controlPoint;
  private float t = 0f;
  public float speed = 1f;
  public bool allowMove; 
  private void GenerateRndPoint() {
    // Generate a random control point
    float randomOffset = Random.Range(-2f, 2f);
    controlPoint = (pointA.position + pointB.position) / 2 + new Vector3(randomOffset, randomOffset, 0);
    transform.position = pointA.position;
    allowMove = true;
  }

  void Update()
  {
    if (Input.GetKeyDown(KeyCode.B)) {
      GenerateRndPoint();
    }
    if(!allowMove)
      return;
    
    t += Time.deltaTime * speed;
    if (t > 1f) t = 1f;

    // Quadratic Bezier formula
    transform.position = (1 - t) * (1 - t) * pointA.position +
                         2 * (1 - t) * t * controlPoint +
                         t * t * pointB.position;
  }
}