using Game;
using Scriptables.Items;
using UnityEngine;

namespace Items {
  public class GroundItem : MonoBehaviour, IInteractable {
    [SerializeField] private int _count = 1;
    [SerializeField] private string _interactName;
    public ItemObject item;
    public bool IsPicked;

    public int Count {
      get { return _count; }
      set { _count = value; }
    }

    public string InteractionPrompt => $"{_interactName} {item.name}";

    public bool Interact(Interactor interactor) {
      Debug.LogError("Interact");
      if (!IsPicked) {
        if (GameManager.instance.PlayerInventory.inventory.AddItem(new Item(item), Count, null, this)) {
          IsPicked = true;
          Destroy(gameObject);
        }
      }
      return true;
    }
  }
}
