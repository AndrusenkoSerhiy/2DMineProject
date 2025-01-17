using Player;
using Settings;
using UnityEngine;

namespace Movement {
  public class LadderMovement : MonoBehaviour {
    public float climbSpeed = 5f;
    [SerializeField] private bool isOnLadder;
    [SerializeField] private bool isClimbing;

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LadderObject ladderObject;
    private PlayerController playerController;
    private Vector2 movement;
    public bool IsClimbing => isClimbing;

    private void Start() {
      playerController = GameManager.instance.PlayerController;
      //playerController.GroundedChanged += ChangeGround;
    }

    /*private void ChangeGround(bool state, float velocity) {
      //if (state) ExitFromLadder();
    }*/

    private void FixedUpdate() {
      if (!ladderObject)
        return;

      if (isOnLadder) {
        movement = UserInput.instance.GetMovement();
        if (movement.magnitude > 0) {
          isClimbing = true;
          rb.linearVelocity = new Vector2(movement.x * climbSpeed, movement.y * climbSpeed);

          //if we near to the top of ladder then block moving higher
          if (movement.y > 0) {
            if (CheckTopDistance() ||
                playerController.transform.position.y > ladderObject.GetTopPoint().position.y)
              rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
          }
        }
        else if (isClimbing) {
          // Stop movement while on the ladder if there's no input
          rb.linearVelocity = new Vector2(0, 0f);
        }

        //if we are moving on ground through ladder
        if (playerController.Grounded && movement.y == 0) {
          rb.linearVelocity = new Vector2(playerController.FrameVelocity.x, playerController.Stats.GroundingForce);
        }
      }
    }

    //how far we from the top on ladder
    private bool CheckTopDistance(float scale = 1f) {
      return Vector3.Distance(ladderObject.GetTopPoint().position, transform.position) <
             scale * ladderObject.GetDelta();
    }

    private void OnTriggerEnter2D(Collider2D collision) {
      if (!collision.CompareTag("Ladder"))
        return;

      ladderObject = collision.GetComponent<LadderObject>();
      isOnLadder = true;
    }

    private void OnTriggerExit2D(Collider2D collision) {
      if (!collision.CompareTag("Ladder"))
        return;

      ExitFromLadder();
      ladderObject = null;
    }

    private void ExitFromLadder() {
      isOnLadder = false;
      isClimbing = false;
    }
  }
}