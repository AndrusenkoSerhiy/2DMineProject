using Actors;
using UnityEngine;
using Utils;
using World;

namespace NPCMovement
{
  public class NPCMovement : MonoBehaviour {
    [SerializeField] private Vector3 target;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float actorBoundsWidth;
    public LayerMask groundLayer;
    [Tooltip("use for special check above zombie")]
    public LayerMask upLayer;
    [Tooltip("Can jump from the other zombie")]
    public LayerMask downLayer;
    private Rigidbody2D rb;
    BoxCollider2D boxCollider2D;
    [SerializeField] private bool isGrounded;
    public float sphereRadius = 1f;
    public float maxDistance = 1f;
    private Vector3 localScale;
    [SerializeField] private bool hasArrived;
    [SerializeField] private Animator animator;
    [SerializeField] private ActorEnemy actor;
    //
    [SerializeField] private bool hasObstacle;
    public bool HasArrived => hasArrived;
    void Start() {
      rb = GetComponent<Rigidbody2D>();
      boxCollider2D = GetComponent<BoxCollider2D>();
      localScale = transform.localScale;
    }

    //for patrol
    public void SetTarget(Vector3 pos) {
      target = pos;
      hasArrived = false;
    }

    //set player like a target
    public void SetTargetTransform(Transform transform, float actorBounds) {
      targetTransform = transform;
      hasArrived = false;
      actorBoundsWidth = actorBounds;
    }

    private void FixedUpdate() {
      IsGrounded();
      
      MoveTowardsTarget();
      MoveTowardsTargetTransform();

      CheckObstacles();
    }

    private void CheckObstacles() {
      if(!IsGrounded())
        return;
      // Check for obstacles in front of the NPC
      var dir = transform.localScale.x * Vector2.right;
      Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.magenta);
      RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(dir.x * -1, 1, 0), dir, 1.5f, groundLayer);
      if (hit.collider == null) {
        hasObstacle = false;
        return;
      }
      
      hasObstacle = true;
      var cell = CoordsTransformer.MouseToGridPosition(hit.transform.position);
      Debug.DrawRay(hit.transform.position, Vector3.up, Color.blue, 2f);
      var currPlayer = GameManager.Instance.CurrPlayerController.PlayerCoords.GetCoordsOutOfBounds();

      var actorCoords = actor.GetCoordsOutOfBounds;
      //cell above the cast cell
      var upperCell = new Coords(cell.X, cell.Y - 1);
      //move up
      if (actorCoords.Y > currPlayer.Y) {
        //Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.blue);
        if (hit.collider != null) {
          //if we have some cell above zombie need to destroy that
          if (CheckUP()) {
            //Destroy cell above
            var aboveCell = new Coords(actorCoords.X, actorCoords.Y - 1);
            AttackCell(GameManager.Instance.ChunkController.GetCell(aboveCell.X, aboveCell.Y));
          }
          else {
            if (GameManager.Instance.ChunkController.ChunkData.GetCellFill(upperCell.X, upperCell.Y) == 1) {
              AttackCell(GameManager.Instance.ChunkController.GetCell(upperCell.X, upperCell.Y));
            }
            else Jump();
          }
        }
      }

      //move forward
      else if (actorCoords.Y == currPlayer.Y) {
        var forwardCell = new Coords(cell.X, cell.Y);
        Debug.DrawRay(CoordsTransformer.GridToWorld(forwardCell.X, forwardCell.Y), Vector3.up, Color.yellow, 2f);
        //Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.blue);
        if (!CheckUP() && GameManager.Instance.ChunkController.ChunkData.GetCellFill(upperCell.X, upperCell.Y) == 0) {
          Jump();
        }
        else if (hit.collider != null) {
          //Destroy cell under
          AttackCell(GameManager.Instance.ChunkController.GetCell(forwardCell.X, forwardCell.Y));
        }
      }else

      //move down
      if (actorCoords.Y < currPlayer.Y) {
        var downCell = new Coords(cell.X + (int)dir.x, cell.Y + 1);
        Debug.DrawRay(CoordsTransformer.GridToWorld(downCell.X, downCell.Y), Vector3.up, Color.yellow, 2f);
        //Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.blue);
        if (hit.collider != null) {
            //Destroy cell under
            AttackCell(GameManager.Instance.ChunkController.GetCell(downCell.X, downCell.Y));
        }
      }
    }

    private bool CheckUP() {
      // Define the starting position (origin) and direction
      Vector3 origin = transform.position + new Vector3(0, 3.3f, 0);
      Vector3 direction = transform.up;

      // Store hit information
      RaycastHit2D hit = Physics2D.CircleCast(origin, sphereRadius, direction, maxDistance, upLayer);
      if (hit.collider != null) {
        // If we hit something, log its name
        Debug.Log($"npc {gameObject.name} | Hit: {hit.collider.name}");

        // Optionally, draw a debug line to visualize the cast
        Debug.DrawLine(origin, hit.point, Color.red);
        return true;
      }
      return false;
    }
    
    //need for zombie can jump from head of other zombie
    private bool CheckDown() {
      // Define the starting position (origin) and direction
      Vector3 origin = transform.position + new Vector3(0, 3.3f, 0);
      Vector3 direction = -transform.up;

      // Store hit information
      RaycastHit2D hit = Physics2D.CircleCast(origin, sphereRadius, direction, maxDistance, downLayer);
      if (hit.collider != null) {
        // If we hit something, log its name
        Debug.Log($"npc {gameObject.name} | Hit: {hit.collider.name}");

        // Optionally, draw a debug line to visualize the cast
        Debug.DrawLine(origin, hit.point, Color.red);
        return true;
      }
      return false;
    }

    void OnDrawGizmos() {
      // Optional: Visualize the sphere at the origin and along the direction
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireSphere(transform.position + new Vector3(0, 3.3f, 0), sphereRadius);

      Vector3 endPoint = transform.position + new Vector3(0, 3.3f, 0) + transform.up * maxDistance;
      Gizmos.DrawWireSphere(endPoint, sphereRadius);
    }

    
    private void MoveTowardsTarget() {
      // Calculate direction and move towards target
      if (target.Equals(Vector3.zero) || actor != null && actor.IsDead) {
        return;
      }
      //Debug.LogError($"{Vector2.Distance(transform.position, target)} | {stopingDistance}");
      if (Vector2.Distance(transform.position, target) <= actor.GetStats().AttackRange) {
        //Debug.LogError("has arrived!!!!!!!!!!");
        target = Vector3.zero;
        hasArrived = true;
        rb.linearVelocity = new Vector2(0, 0);
        SetAnimVelocityX(0);
        return;
      }

      hasArrived = false;
      Vector2 direction = (target - transform.position).normalized;
      FlipX(direction.x);
      rb.linearVelocity = new Vector2(direction.x * actor.GetStats().MaxSpeed, rb.linearVelocity.y);
      SetAnimVelocityX(rb.linearVelocity.x);
    }
    
    //attack only when target player
    public void AttackPlayer() {
      actor?.TriggerAttack(GameManager.Instance.CurrPlayerController.Actor);
    }

    public void AttackCell(IDamageable cell) {
      actor.TriggerAttack(cell);
    }
    
    private void MoveTowardsTargetTransform() {
      if (targetTransform == null || actor != null && actor.IsDead) {
        return;
      }
      
      if (Vector2.Distance(transform.position, targetTransform.position) <= actor.GetStats().AttackRange + actorBoundsWidth) {
        //Debug.LogError("has arrived!!!!!!!!!!");
        hasArrived = true;
        rb.linearVelocity = new Vector2(0, 0);
        SetAnimVelocityX(0);
        return;
      }
      //stop before the obstacle
      if (hasObstacle) {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        SetAnimVelocityX(0);
        return;
      }
      
      hasArrived = false;
      Vector2 direction = (targetTransform.position - transform.position).normalized;
      FlipX(direction.x);
      rb.linearVelocity = new Vector2(direction.x * actor.GetStats().MaxSpeed, rb.linearVelocity.y);
      SetAnimVelocityX(rb.linearVelocity.x);
    }
    
    private void SetAnimVelocityX(float value) {
      animator.SetFloat(actor.AnimParam.VelocityXHash, value);
    }

    private void FlipX(float direction) {
      localScale.x = Mathf.Sign(-direction);
      transform.localScale = localScale;
    }

    private void Jump() {
      if (/*target == null ||*/ actor != null && actor.IsDead) {
        return;
      }

      hasObstacle = false;
      if (IsGrounded() /*|| CheckDown()*/) {
        //Debug.LogError($"jump {gameObject.name}");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, actor.GetStats().StatsObject.jumpPower);
      }
    }

    private bool IsGrounded() {
      // Check if NPC is on the ground
      isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.3f, groundLayer);
      return isGrounded;
    }
  }
}
