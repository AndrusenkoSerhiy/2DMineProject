using System.Runtime.InteropServices.WindowsRuntime;
using Actors;
using SaveSystem;
using Scriptables;
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
    [SerializeField] private bool hasObstacle;
    private GameManager gameManager;
    public bool HasArrived => hasArrived;
    void Start() {
      rb = GetComponent<Rigidbody2D>();
      boxCollider2D = GetComponent<BoxCollider2D>();
      localScale = transform.localScale;
      gameManager = GameManager.Instance;
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
      if (gameManager.Paused || targetTransform == null && target.Equals(Vector3.zero)) {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        return;
      }
      
      IsGrounded();
      
      MoveTowardsTarget();
      MoveTowardsTargetTransform();

      CheckObstacles();
    }

    private void CheckObstacles() {
      if (!IsGrounded() && !CheckDown())
        return;

      // Check for obstacles in front of the NPC
      var dir = transform.localScale.x * Vector2.right;
      var actorCoords = actor.GetCoordsOutOfBounds;
      
      
      //for door 
      var buildCoord = CoordsTransformer.GridToBuildingsGrid(actorCoords.X - (int)dir.x, actorCoords.Y);
      var door = GetBuildingDataObject(buildCoord.X, buildCoord.Y);
      //Debug.DrawRay(CoordsTransformer.GridToWorld(actorCoords.X - (int)dir.x, actorCoords.Y), Vector3.up, Color.yellow, 1f);
      if (door != null) {
        var damageable = door.GetComponent<IDamageable>();
        AttackDoor(damageable);
        return;
      }
      
      var obstacle = GetCellObject(actorCoords.X - (int)dir.x, actorCoords.Y);
      
      var distanceToObstacle = -1f;
      if (obstacle != null) {
        var obstaclePos = CoordsTransformer.OutOfGridToWorls(obstacle.CellData.x, obstacle.CellData.y);
        //Debug.DrawRay(obstaclePos, Vector3.up, Color.red,1f);
        distanceToObstacle = Vector3.Distance(transform.position, obstaclePos);
      }
      
      var currPlayer = gameManager.CurrPlayerController.PlayerCoords.GetCoordsOutOfBounds();

      //if we don't have obstacle or distance is too big then return
      if (obstacle == null || distanceToObstacle > 3.5f) {
        //zombie attack cell under self
        if (actorCoords.X == currPlayer.X && actorCoords.Y < currPlayer.Y) {
          StraightDown(actorCoords.X, actorCoords.Y);
        }
        hasObstacle = false;
        return;
      }
      
      //Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.magenta);
      hasObstacle = true;
      var cellCoords = new Coords(obstacle.CellData.x, obstacle.CellData.y);
      //Debug.DrawRay(hit.transform.position, Vector3.up, Color.blue, 2f);
      
      //cell above the cast cell
      var upCellCoords = new Coords(cellCoords.X, cellCoords.Y - 1);
      //move up
      if (actorCoords.Y > currPlayer.Y) {
        MoveUp(obstacle, actorCoords, upCellCoords);
      }
      //move forward
      else if (actorCoords.Y == currPlayer.Y) {
        MoveForward(cellCoords, obstacle, upCellCoords);
      }
      //move down
      else if (actorCoords.Y < currPlayer.Y) {
        MoveDown(cellCoords, obstacle, dir);
      }
    }

    private void MoveUp(CellObject obstacle, Coords actorCoords, Coords upCellCoords) {
      //Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.blue);
      if (obstacle != null) {
        //if we have some cell above zombie need to destroy that
        var aboveCell = new Coords(actorCoords.X, actorCoords.Y - 1);
        if (CheckUP() && gameManager.ChunkController.ChunkData.GetCellFill(aboveCell.X, aboveCell.Y) == 1 &&
            gameManager.ChunkController.ChunkData.GetCellData(aboveCell.X, aboveCell.Y).canTakeDamage) {
          //Destroy cell above
          AttackCell(GetCellObject(aboveCell.X, aboveCell.Y));
        }
        else {
          if (gameManager.ChunkController.ChunkData.GetCellFill(upCellCoords.X, upCellCoords.Y) == 1 && 
              gameManager.ChunkController.ChunkData.GetCellData(upCellCoords.X, upCellCoords.Y).canTakeDamage) {
            AttackCell(GetCellObject(upCellCoords.X, upCellCoords.Y));
          }
          else {
            Jump();
          }
        }
      }
    }

    private void MoveForward(Coords cellCoords, CellObject obstacle, Coords upCellCoords) {
      var forwardCell = new Coords(cellCoords.X, cellCoords.Y);
      //Debug.DrawRay(CoordsTransformer.GridToWorld(forwardCell.X, forwardCell.Y), Vector3.up, Color.yellow, 2f);
      //Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.blue);
      if (!CheckUP() && gameManager.ChunkController.ChunkData.GetCellFill(upCellCoords.X, upCellCoords.Y) == 0) {
        Jump();
      }
      else if (obstacle != null) {
        //Destroy cell under
        AttackCell(GetCellObject(forwardCell.X, forwardCell.Y));
      }
    }

    private void MoveDown(Coords cellCoords, CellObject obstacle, Vector2 dir) {
      var downCell = new Coords(cellCoords.X + (int)dir.x, cellCoords.Y + 1);
      //Debug.DrawRay(CoordsTransformer.GridToWorld(downCell.X, downCell.Y), Vector3.up, Color.yellow, 2f);
      //Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.blue);
      if (obstacle != null) {
        //Destroy cell under
        AttackCell(GetCellObject(downCell.X, downCell.Y));
      }
    }
    
    //use only zombie on the player and need dig down
    private void StraightDown(int x, int y) {
      if (gameManager.ChunkController.ChunkData.GetCellFill(x, y + 1) == 0 ||
          !gameManager.ChunkController.ChunkData.GetCellData(x, y + 1).canTakeDamage)
        return;
      
      AttackCell(GetCellObject(x, y + 1));
    }

    private bool CheckUP() {
      // Define the starting position (origin) and direction
      Vector3 origin = transform.position + new Vector3(0, 3.7f, 0);
      //Debug.DrawRay(origin, transform.up, Color.red, 2f);
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

    private CellObject GetCellObject(int x, int y) {
      return gameManager.ChunkController.GetCell(x, y);
    }

    private BuildingDataObject GetBuildingDataObject(int x, int y) {
      return gameManager.ChunkController.GetBuildingData(x, y);
    }
    //need for zombie can jump from head of other zombie
    private bool CheckDown() {
      // Define the starting position (origin) and direction
      Vector3 origin = transform.position + new Vector3(0, -.5f, 0);
      //Debug.DrawRay(origin, -transform.up, Color.red, 2f);
      Vector3 direction = -transform.up;

      // Store hit information
      RaycastHit2D hit = Physics2D.CircleCast(origin, sphereRadius, direction, maxDistance, downLayer);
      if (hit.collider != null) {
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
    
    public void AttackPlayer() {
      actor?.TriggerAttack(gameManager.CurrPlayerController.Actor);
    }

    public void AttackCell(IDamageable cell) {
      actor.TriggerAttack(cell);
    }
    
    public void AttackDoor(IDamageable door) {
      actor.TriggerAttack(door);
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

    public void StopAnimator() {
      rb.linearVelocity = new Vector2(0, 0);
      SetAnimVelocityX(0);
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
      if (IsGrounded() || CheckDown()) {
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
