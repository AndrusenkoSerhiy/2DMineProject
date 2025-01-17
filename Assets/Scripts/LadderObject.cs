using UnityEngine;

public class LadderObject : MonoBehaviour {
  [SerializeField] private Transform topPoint;
  [SerializeField] private Transform bottomPoint;
  [SerializeField] private float delta;
  
  public Transform GetTopPoint() => topPoint;
  public Transform GetBottomPoint() => bottomPoint;
  public float GetDelta() => delta;
}