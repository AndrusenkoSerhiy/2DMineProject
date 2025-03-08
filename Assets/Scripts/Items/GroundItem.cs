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

    public int Count {
      get => count;
      set => count = value;
    }

    public string InteractionText => $"{interactName} {item.name}";
    public string InteractionHeader => interactHeader;

    public bool Interact(PlayerInteractor playerInteractor) {
      //Debug.LogError("Interact");
      if (isPicked) {
        return true;
      }

      var gameManager = GameManager.Instance;

      var addedAmount = gameManager.PlayerInventory.AddItemToInventoryWithOverflowDrop(new Item(item), Count);
      gameManager.RecipesManager.DiscoverMaterial(item);
      gameManager.MessagesManager.ShowPickupResourceMessage(item, addedAmount);

      isPicked = true;
      Destroy(gameObject);

      return true;
    }
  }
}