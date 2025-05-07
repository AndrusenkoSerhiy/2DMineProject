using Actors;
using UnityEngine;

namespace NPCMovement
{
  public class NPCMovement : MonoBehaviour {
    [SerializeField] private Vector3 target;
    [SerializeField] private Transform targetTransform;
    public LayerMask groundLayer;
    private Rigidbody2D rb;
    BoxCollider2D boxCollider2D;
    [SerializeField] private bool isGrounded;
    public float sphereRadius = 1f;
    public float maxDistance = 1f;
    private Vector3 localScale;
    [SerializeField] private bool hasArrived;
    [SerializeField] private Animator animator;
    [SerializeField] private ActorEnemy actor;
    
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
    public void SetTargetTransform(Transform transform) {
      targetTransform = transform;
      hasArrived = false;
    }

    private void FixedUpdate() {
      IsGrounded();
      
      MoveTowardsTarget();
      MoveTowardsTargetTransform();

      // Check for obstacles in front of the NPC
      var dir = transform.localScale.x * Vector2.right;
      RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(dir.x * -1, 1, 0), dir, 1, groundLayer);
      
      //Debug.DrawRay(transform.position + new Vector3(dir.x * -1, 1, 0), dir, Color.blue);
      if (hit.collider != null && !CheckUP()) {
        //Debug.LogError($"hit {gameObject.name}");
        Jump();
      }
    }

    private bool CheckUP() {
      // Define the starting position (origin) and direction
      Vector3 origin = transform.position + new Vector3(0, 3.3f, 0);
      Vector3 direction = transform.up;

      // Store hit information
      RaycastHit2D hit = Physics2D.CircleCast(origin, sphereRadius, direction, maxDistance, groundLayer);
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
      actor?.TriggerAttack();
    }
    
    private void MoveTowardsTargetTransform() {
      if (targetTransform == null || actor != null && actor.IsDead) {
        return;
      }
      
      if (Vector2.Distance(transform.position, targetTransform.position) <= actor.GetStats().AttackRange) {
        //Debug.LogError("has arrived!!!!!!!!!!");
        hasArrived = true;
        rb.linearVelocity = new Vector2(0, 0);
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

      if (IsGrounded()) {
        //Debug.LogError("add force");
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
