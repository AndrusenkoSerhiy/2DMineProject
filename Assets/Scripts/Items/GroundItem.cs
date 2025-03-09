using System.Collections;
using Interaction;
using Scriptables.Items;
using UnityEngine;

namespace Items {
  public class GroundItem : MonoBehaviour, IInteractable {
    [SerializeField] private int count = 1;
    [SerializeField] private string interactName;
    [SerializeField] private string interactHeader;
    public ItemObject item;
    public bool isPicked;

    private Coroutine destructionCoroutine;

    public int Count {
      get => count;
      set => count = value;
    }

    public string InteractionText => $"{interactName} {item.name}";
    public string InteractionHeader => interactHeader;

    private void OnEnable() {
      var destroyAfter = GameManager.Instance.GroundItemPool.autoDestroyDelay;
      destructionCoroutine = StartCoroutine(AutoDestroyAfterDelay(destroyAfter));
    }

    private void OnDisable() {
      if (destructionCoroutine != null) {
        StopCoroutine(destructionCoroutine);
      }
    }

    private IEnumerator AutoDestroyAfterDelay(float delay) {
      yield return new WaitForSeconds(delay);
      if (!isPicked) {
        GameManager.Instance.GroundItemPool.ReturnItem(this);
      }
    }

    public void ResetState() {
      isPicked = false;
      count = 1;
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