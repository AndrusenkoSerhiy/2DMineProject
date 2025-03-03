using Player;
using UnityEngine;

public class LadderObject : MonoBehaviour {
  [SerializeField] private Transform topPoint;
  [SerializeField] private Transform bottomPoint;
  [SerializeField] private float delta;
  
  private PlayerController player;
  public Transform GetTopPoint() => topPoint;
  public Transform GetBottomPoint() => bottomPoint;
  public float GetDelta() => delta;

  private void OnTriggerEnter2D(Collider2D collision) {
    if (!CompareTag(collision))
      return;
    
    SetLadderObject(this);
  }

  private void OnTriggerExit2D(Collider2D collision) {
    if (!CompareTag(collision))
      return;
    
    SetLadderObject(null);
  }

  private bool CompareTag(Collider2D collision) {
    return collision.CompareTag("Player");
  }

  private void SetLadderObject(LadderObject ladderObject) {
    if (GameManager.Instance == null)
      return;
    
    player = GameManager.Instance.PlayerController;
    player.LadderMovement.SetLadder(ladderObject);
  }
}