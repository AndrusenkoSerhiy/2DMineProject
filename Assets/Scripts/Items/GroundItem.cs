using Interaction;
using Scriptables.Items;
using UnityEngine;

namespace Items {
  public class GroundItem : MonoBehaviour, IInteractable {
    [SerializeField] private int count = 1;
    [SerializeField] private string interactName;
    public ItemObject item;
    public bool isPicked;

    public int Count {
      get => count;
      set => count = value;
    }

    public string InteractionPrompt => $"{interactName} {item.name}";

    public bool Interact(PlayerInteractor playerInteractor) {
      //Debug.LogError("Interact");
      if (isPicked) {
        return true;
      }

      var inventory = GameManager.Instance.PlayerInventory.inventory;
      inventory.AddItem(new Item(item), Count, null, this);
      isPicked = true;
      Destroy(gameObject);

      return true;
    }
  }
}