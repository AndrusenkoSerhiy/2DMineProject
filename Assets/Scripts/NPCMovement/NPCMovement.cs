using Game.Actors;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCMovement : MonoBehaviour {
    public Transform target;
    public float speed = 3f;
    public float jumpForce = 5f;
    public float stopingDistance = 1;
    public LayerMask groundLayer;
    private Rigidbody2D rb;
    BoxCollider2D boxCollider2D;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private ActorEnemy _actor;
    public float sphereRadius = 1f;
    public float maxDistance = 1f;
    private Vector3 localScale;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        localScale = transform.localScale;
    }

    private void FixedUpdate() {
        MoveTowardsTarget();

        // Check for obstacles in front of the NPC
        RaycastHit2D hit = Physics2D.Raycast(transform.position + new Vector3(1, 1, 0), transform.right, 1, groundLayer);

        if (hit.collider != null && !CheckUP()) {//&& hit.collider != boxCollider2D
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
        if (target == null || _actor != null && _actor.IsDead) {
            return;
        }
        if (Vector2.Distance(transform.position, target.transform.position) <= stopingDistance) {
            rb.linearVelocity = new Vector2(0, 0);
            _actor?.TriggerAttack();
            return;
        }

        Vector2 direction = (target.position - transform.position).normalized;
        FlipX(direction.x);
        rb.linearVelocity = new Vector2(direction.x * speed, rb.linearVelocity.y);
    }

    private void FlipX(float direction) {
        localScale.x = Mathf.Sign(-direction);
        transform.localScale = localScale;
    }

    private void Jump() {
        if (target == null || _actor != null && _actor.IsDead) {
            return;
        }

        if (IsGrounded()) {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private bool IsGrounded() {
        // Check if NPC is on the ground
        _isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 0.3f, groundLayer);
        return _isGrounded;
    }
}
