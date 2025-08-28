using System;
using System.Collections;
using Interaction;
using Scriptables.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Items {
  public class GroundItem : MonoBehaviour, IInteractable {
    [SerializeField] private int count = 1;
    [SerializeField] private string interactName;
    [SerializeField] private string holdInteractText;
    [SerializeField] private ItemObject item;
    [SerializeField] private bool isPicked;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private bool startMerging;
    private Coroutine destructionCoroutine;

    private float? durability;

    public int Count {
      get => count;
      set => count = value;
    }

    public float? Durability {
      get => durability;
      set => durability = value;
    }

    public string InteractionText => $"Pickup {item.name}";
    public bool HasHoldInteraction { get; }
    public string HoldInteractionText => holdInteractText;

    public Bounds GetBounds() {
      return spriteRenderer ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.zero);
    }

    public ItemObject Item => item;

    private void Awake() {
      rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable() {
      var destroyAfter = GameManager.Instance.GroundItemPool.autoDestroyDelay;
      destructionCoroutine = StartCoroutine(AutoDestroyAfterDelay(destroyAfter));

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
      startMerging = false;
      durability = null;
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      if (isPicked) {
        return true;
      }

      var gameManager = GameManager.Instance;

      if (!gameManager.PlayerInventory.CanAddItemToInventory(item)) {
        gameManager.MessagesManager.ShowSimpleMessage("Inventory is full");
        return false;
      }

      var addedAmount =
        gameManager.PlayerInventory.AddItemToInventoryWithOverflowDrop(new Item(item, Durability), Count);
      gameManager.MessagesManager.ShowPickupResourceMessage(item, addedAmount);

      isPicked = true;
      gameManager.GroundItemPool.ReturnItem(this);

      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      return false;
    }
  }
}