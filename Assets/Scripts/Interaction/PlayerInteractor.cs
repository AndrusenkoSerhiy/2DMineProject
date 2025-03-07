using UnityEngine;

namespace Interaction
{
  public class PlayerInteractor : MonoBehaviour {
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private float radius = .5f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private InteractionPrompt interactionPromtUI;

    private Collider2D[] colliders = new Collider2D[3];
    [SerializeField] private int numFound;

    private IInteractable interactable;
    private void Update() {
      //id any window is open don't allow to find items and show interaction message
      if (GameManager.Instance.WindowsController.IsAnyWindowOpen) {
        interactionPromtUI.ShowPrompt(false);
        return; 
      }
      
      UpdateInteractionPrompt();
    }

    private void UpdateInteractionPrompt() {
      colliders = Physics2D.OverlapCircleAll(interactionPoint.position, radius, interactableMask);
      numFound = colliders.Length;

      if (numFound <= 0) {
        interactable = null;
        interactionPromtUI.ShowPrompt(false);
        return;
      }

      interactable = colliders[0].GetComponent<IInteractable>();
      if (interactable == null)
        return;

      interactionPromtUI.ShowPrompt(true, interactable.InteractionText);
      if (GameManager.Instance.UserInput.controls.GamePlay.Interact.WasPressedThisFrame()) {
        interactable.Interact(this);
      }
    }

    private void OnDrawGizmos() {
      Gizmos.color = Color.red;
      Gizmos.DrawWireSphere(interactionPoint.position, radius);
    }
  }
}
