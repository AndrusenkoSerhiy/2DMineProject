using Interaction;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Craft {
  public class Door : MonoBehaviour, IInteractable {
    [SerializeField] private string interactOpenText;
    [SerializeField] private string interactCloseText;
    [SerializeField] private string holdInteractText;
    [SerializeField] private bool hasHoldInteraction = true;
    [SerializeField] private bool IsOpened = false;
    [SerializeField] private Animator animator;
    [SerializeField] private BuildingDataObject buildObject;
    [SerializeField] protected ItemObject itemObject;
    public string InteractionText => IsOpened ? interactCloseText : interactOpenText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;


    public bool Interact(PlayerInteractor playerInteractor) {
      if (IsOpened) {
        animator.SetBool("IsOpened", false);
        IsOpened = false;
      }
      else {
        animator.SetBool("IsOpened", true);
        IsOpened = true;
      }

      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      GameManager.Instance.PlayerInventory.TakeBuildingToInventory(buildObject, itemObject);
      return true;
    }
  }
}