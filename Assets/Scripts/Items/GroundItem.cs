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

      var inventory = GameManager.Instance.PlayerInventory.GetInventory();
      inventory.AddItem(new Item(item), Count, null, this);
      
      GameManager.Instance.RecipesManager.DiscoverMaterial(item);
      GameManager.Instance.MessagesManager.ShowPickupResourceMessage(item, Count);
      
      isPicked = true;
      Destroy(gameObject);

      return true;
    }
  }
}