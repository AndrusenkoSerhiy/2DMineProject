using UnityEngine;

namespace Player {
  public class ObjectHighlight : MonoBehaviour {
    private static readonly int Thickness = Shader.PropertyToID("_Thickness");
    public SpriteRenderer spriteRendererRef;
    public IDamageable damageableRef;
    public float thickness = .5f;

    private void Awake() {
      damageableRef = GetComponent<IDamageable>();
    }

    public void SetHighlight(bool state) {
      var newThickness = state &&  CanShowHighlight() ? thickness : 0;
      spriteRendererRef.material.SetFloat(Thickness, newThickness);
    }

    public virtual bool CanShowHighlight() {
      return true;
    }
  }
}