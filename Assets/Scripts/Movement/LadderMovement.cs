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
    [SerializeField]private Vector2 movement;
    public bool IsClimbing => isClimbing;
    public bool IsOnLadder => isOnLadder;
    [SerializeField] private bool stayOnLadderAfterJump;

    public void SetClimbing(bool state, string id) {
      if(!state) SetIsClimbing(false);
      if (GetVerticalMovement() > 0) {
        stayOnLadderAfterJump = true;
      }
      //якщо після стрибка ми починаємо падати, але маємо залишитись на драбині
      if (id.Equals("fall") && stayOnLadderAfterJump){//(state && stayOnLadderAfterJump) {
        stayOnLadderAfterJump = false;
        SetIsClimbing(true);
      }
    }

    private void ChangeValue() {
      //canClimbAgain = true;
      if (isOnLadder) {
        SetIsClimbing(true);
        stayOnLadderAfterJump = false;
      }
    }
    
    private void Start() {
      playerController = GameManager.instance.PlayerController;
      playerController.GroundedChanged += ChangeGround;
    }

    private void ChangeGround(bool state, float velocity) {
      if (state) ChangeValue();
    }

    private void FixedUpdate() {
      //
      if (stayOnLadderAfterJump) {
        SetIsClimbing(false);
        return;
      }
      if (!ladderObject)
        return;

      if (isOnLadder) {
        movement = UserInput.instance.GetMovement();
        if (movement.magnitude > 0) {
          SetIsClimbing(true);
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
          //Debug.LogError("stop movement");
          rb.linearVelocity = new Vector2(0, 0f);
        }
        //if we are moving on ground through ladder set the same speed
        if (playerController.Grounded && movement.x != 0) {
          //Debug.LogError("move horizontally");
          rb.linearVelocity = new Vector2(playerController.FrameVelocity.x, playerController.Stats.GroundingForce);
        }
      }
    }

    public float GetVerticalMovement() {
      return movement.y;
    }

    //how far we from the top on ladder
    private bool CheckTopDistance(float scale = 1f) {
      return Vector3.Distance(ladderObject.GetTopPoint().position, transform.position) <
             scale * ladderObject.GetDelta();
    }

    public void SetLadder(LadderObject ladder) {
      ladderObject = ladder;
      if (ladderObject != null) {
        isOnLadder = true;
      }
      else {
        ExitFromLadder();
      }
    }

    private void ExitFromLadder() {
      isOnLadder = false;
      SetIsClimbing(false);
    }

    private void SetIsClimbing(bool state) {
      isClimbing = state;
    }

    private void OnDestroy() {
      playerController.GroundedChanged -= ChangeGround;
    }
  }
}