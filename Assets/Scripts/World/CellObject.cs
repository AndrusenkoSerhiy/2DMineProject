using UnityEngine;

namespace World {
  public class CellObject : MonoBehaviour, IDamageable {
    private CellData _cellData;
    [SerializeField] private float _maxHealth = 10f;
    private UnitHealth _unitHealth;

    private void Start() {
      _unitHealth = new UnitHealth(_maxHealth);
    }

    public bool HasTakenDamage {
      get { return _unitHealth.HasTakenDamage; }
      set { _unitHealth.HasTakenDamage = value; }
    }

    public void Damage(float damage) {
      _unitHealth.TakeDamage(damage);
    }

    public float GetHealth() {
      return _unitHealth.Health;
    }
  }
}