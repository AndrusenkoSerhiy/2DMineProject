using UnityEngine;

namespace Player {
  public class ObjectHighlight : MonoBehaviour {
    public SpriteRenderer spriteRendererRef;
    public IDamageable damageableRef;

    private void Awake() {
      damageableRef = GetComponent<IDamageable>();
    }
    public void ClearHighlight() {
      spriteRendererRef.material.SetFloat("_Thickness", 0f);
    }
  }
}