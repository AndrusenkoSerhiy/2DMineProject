using UnityEngine;
using DG.Tweening;
using System;

namespace World {
  public class CellObject : MonoBehaviour, IDamageable {
    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private CellStats _cellStats;

    private CellData _cellData;
    private UnitHealth _unitHealth;

    private bool isShaking = false;

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

    public float GetMaxHealth() {
      return _unitHealth.MaxHealth;
    }

    public void AfterDamageReceived() {
      Shake();
    }

    private void Shake() {
      if (isShaking) {
        return;
      }

      isShaking = true;
      Vector3 originalPosition = transform.position;
      float healthPercentage = GetHealth() / GetMaxHealth();

      float shakeDuration = Mathf.Lerp(_cellStats.minShakeDuration, _cellStats.maxShakeDuration, 1 - healthPercentage);
      float shakeIntensity = Mathf.Lerp(_cellStats.minShakeIntensity, _cellStats.maxShakeIntensity, 1 - healthPercentage);

      int vibrato = (int)(_cellStats.vibrato / healthPercentage);

      transform.DOShakePosition(shakeDuration, shakeIntensity, vibrato, _cellStats.randomness, false, true)
        .OnComplete(() => {
          transform.position = originalPosition;
          isShaking = false;
        });
    }
  }
}