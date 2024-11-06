using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour {
    [SerializeField] private Transform _interactionPoint;
    [SerializeField] private float _interactionPointRadius = .5f;
    [SerializeField] private LayerMask _interactableMask;
    [SerializeField] private InteractUI _interactionPromtUI;

    private Collider2D[] _colliders = new Collider2D[3];
    [SerializeField] private int _numFound;

    private IInteractable _interactable;
    private void Update() {
        _colliders = Physics2D.OverlapCircleAll(_interactionPoint.position, _interactionPointRadius, _interactableMask);
        _numFound = _colliders.Count();

        if (_numFound > 0) {
            _interactable = _colliders[0].GetComponent<IInteractable>();
            if (_interactable != null) {
                if(!_interactionPromtUI.IsDisplayed) _interactionPromtUI.ShowPromt(_interactable.InteractionPrompt);

                if(Keyboard.current.eKey.wasPressedThisFrame) _interactable.Interact(this);
            }
        }else{
            _interactable = null;
            if(_interactionPromtUI.IsDisplayed) _interactionPromtUI.HidePromt();
        }

    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(_interactionPoint.position, _interactionPointRadius);
    }
}
