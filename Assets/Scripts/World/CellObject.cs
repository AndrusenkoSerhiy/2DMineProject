using UnityEngine;
using DG.Tweening;
using System;

namespace World {
  public class CellObject : MonoBehaviour, IDamageable {
    [SerializeField] private float _maxHealth = 10f;
    [SerializeField] private CellStats _cellStats;
    [SerializeField] private Transform _spriteTr;

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
      Vector3 originalPosition = _spriteTr.position;
      Vector3 originalScale = _spriteTr.localScale;
      float healthPercentage = GetHealth() / GetMaxHealth();

      Renderer renderer = _spriteTr.GetComponent<Renderer>();
      int originalSortingOrder = renderer != null ? renderer.sortingOrder : 0;
      if (renderer != null) {
        renderer.sortingOrder += 1;
      }

      float shakeDuration = Mathf.Lerp(_cellStats.minShakeDuration, _cellStats.maxShakeDuration, 1 - healthPercentage);
      float shakeIntensity = Mathf.Lerp(_cellStats.minShakeIntensity, _cellStats.maxShakeIntensity, 1 - healthPercentage);
      int vibrato = Mathf.RoundToInt(Mathf.Lerp(_cellStats.minVibrato, _cellStats.maxVibrato, 1 - healthPercentage));

      float scaleFactorX = Mathf.Lerp(originalScale.x, _cellStats.scaleFactor.x, 1 - healthPercentage);
      float scaleFactorY = Mathf.Lerp(originalScale.y, _cellStats.scaleFactor.y, 1 - healthPercentage);
      Vector3 shakeScale = originalScale;
      shakeScale.x *= scaleFactorX;
      shakeScale.y *= scaleFactorY;
      _spriteTr.localScale = shakeScale;

      _spriteTr.DOShakePosition(shakeDuration, shakeIntensity, vibrato, _cellStats.randomness, false, true)
        .OnComplete(() => {
          _spriteTr.position = originalPosition;
          _spriteTr.localScale = originalScale;
          isShaking = false;
          if (renderer != null) {
            renderer.sortingOrder = originalSortingOrder;
          }
        });
    }
  }
}