using UnityEngine;

namespace World {
  public class CellObject : MonoBehaviour, IDamageable {
    private CellData _cellData;

    [SerializeField] private float _maxHealth = 10f;

    private float _currentHealth;

    public bool HasTakenDamage { get; set; }

    private void Start() {
      _currentHealth = _maxHealth;
    }

    public void Damage(float damage) {
      HasTakenDamage = true;
      _currentHealth -= damage;
    }

    public float GetHealth() {
      return _currentHealth;
    }
  }
}