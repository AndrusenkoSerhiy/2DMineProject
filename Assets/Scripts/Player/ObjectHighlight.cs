using UnityEngine;

namespace Player {
  public class ObjectHighlight : MonoBehaviour {
    public SpriteRenderer spriteRendererRef;

    public void ClearHighlight() {
      spriteRendererRef.material.SetFloat("_Thickness", 0f);
    }
  }
}