using System;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;

public class MoveWithCurve : MonoBehaviour
{
  public Transform pointA;
  public Transform pointB;

  private void Update() {
    if (Input.GetKeyDown(KeyCode.P)) {
      StartMove();
    }
  }

  void StartMove() {
   
    Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
    Vector3 controlPoint = (pointA.position + pointB.position) / 2 + randomOffset;

    // Move along a curved path
    transform.DOPath(new Vector3[] { pointA.position, controlPoint, pointB.position }, 2f, PathType.CatmullRom)
      .SetEase(Ease.InOutSine);
  }
}