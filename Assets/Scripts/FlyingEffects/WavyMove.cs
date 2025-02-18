using System;
using UnityEngine;

public class WavyMove : MonoBehaviour {
  public Vector3 pointA;
  public Transform pointB;
  public float frequency = 2f;
  public float amplitude = 1f;
  private float t = 0f;
  public float speed = 1f;
  private bool isMoveComplete;
  [SerializeField] private SpriteRenderer spriteRenderer;
  public bool IsMoveComplete => isMoveComplete;

  private void OnEnable() {
    isMoveComplete = false;
    t = 0;
  }

  public void SetPositions(Vector3 a, Transform b) {
    pointA = a;
    pointB = b;
  }

  void Update() {
    t += Time.deltaTime * speed;
    if (t > 1f) {
      t = 1f;
      isMoveComplete = true;
    }

    // Linear interpolation between A and B
    Vector3 pos = Vector3.Lerp(pointA, pointB.position, t);

    // Apply sinusoidal wave effect
    pos.y += Mathf.Sin(t * Mathf.PI * frequency) * amplitude;

    transform.position = pos;
  }

  public void UpdateSprite(Sprite sprite) {
    spriteRenderer.sprite = sprite;
  }
}