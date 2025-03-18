using System.Collections;
using Interaction;
using Scriptables.Items;
using UnityEngine;

namespace Items {
  public class GroundItem : MonoBehaviour, IInteractable {
    [SerializeField] private int count = 1;
    [SerializeField] private string interactName;
    [SerializeField] private string interactHeader;
    [SerializeField] private ItemObject item;
    [SerializeField] private bool isPicked;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool startMerging;

    private Coroutine destructionCoroutine;

    public int Count {
      get => count;
      set => count = value;
    }

    public string InteractionText => $"Pickup {item.name}";
    public string InteractionHeader => interactHeader;

    public ItemObject Item => item;

    private void OnEnable() {
      var destroyAfter = GameManager.Instance.GroundItemPool.autoDestroyDelay;
      destructionCoroutine = StartCoroutine(AutoDestroyAfterDelay(destroyAfter));

      SetRigidbody();
      AddForce();
    }

    private void OnDisable() {
      if (destructionCoroutine != null) {
        StopCoroutine(destructionCoroutine);
      }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
      if (startMerging) {
        return;
      }

      if (collision.gameObject.layer == LayerMask.NameToLayer("Cell")) {
        isGrounded = true;
      }

      if (!isGrounded) {
        return;
      }

      if (!collision.gameObject.CompareTag("GroundItem")) {
        return;
      }

      var otherGroundItem = collision.gameObject.GetComponent<GroundItem>();
      if (otherGroundItem.isPicked || otherGroundItem.startMerging) {
        return;
      }

      if (item.Id != otherGroundItem.item.Id) {
        return;
      }

      RestartCoroutine();

      startMerging = true;
      otherGroundItem.startMerging = true;

      count += otherGroundItem.count;

      otherGroundItem.isPicked = true;
      GameManager.Instance.GroundItemPool.ReturnItem(otherGroundItem);

      startMerging = false;
    }

    private void AddForce() {
      if (rb == null) {
        return;
      }

      var randomForce = new Vector2(Random.Range(-1f, 1f), 1f) * 5f;
      rb.AddForce(randomForce, ForceMode2D.Impulse);
    }

    private void SetRigidbody() {
      if (rb != null) {
        return;
      }

      rb = GetComponent<Rigidbody2D>();
    }

    private IEnumerator AutoDestroyAfterDelay(float delay) {
      yield return new WaitForSeconds(delay);
      if (!isPicked) {
        GameManager.Instance.GroundItemPool.ReturnItem(this);
      }
    }

    private void RestartCoroutine() {
      if (destructionCoroutine != null) {
        StopCoroutine(destructionCoroutine);
      }

      var destroyAfter = GameManager.Instance.GroundItemPool.autoDestroyDelay;
      destructionCoroutine = StartCoroutine(AutoDestroyAfterDelay(destroyAfter));
    }

    public void ResetState() {
      isPicked = false;
      count = 1;
      isGrounded = false;
      startMerging = false;
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      if (isPicked) {
        return true;
      }

      var gameManager = GameManager.Instance;

      var addedAmount = gameManager.PlayerInventory.AddItemToInventoryWithOverflowDrop(new Item(item), Count);
      // gameManager.RecipesManager.DiscoverMaterial(item);
      gameManager.MessagesManager.ShowPickupResourceMessage(item, addedAmount);

      isPicked = true;
      gameManager.GroundItemPool.ReturnItem(this);

      return true;
    }
  }
}