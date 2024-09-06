using UnityEngine;
using DG.Tweening;
using System;

namespace World {
  public class CellObject : MonoBehaviour, IDamageable {
    [SerializeField] private float maxHealth = 10f;
    [SerializeField] private CellStats cellStats;
    [SerializeField] private Transform sprite;

    private CellData _cellData;
    private UnitHealth unitHealth;

    private Renderer cellRenderer;
    private Tween currentShakeTween;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private int originalSortingOrder;

    private void Start() {
      unitHealth = new UnitHealth(maxHealth);
    }

    public bool HasTakenDamage {
      get { return unitHealth.HasTakenDamage; }
      set { unitHealth.HasTakenDamage = value; }
    }

    public void Damage(float damage) {
      unitHealth.TakeDamage(damage);
    }

    public float GetHealth() {
      return unitHealth.Health;
    }

    public float GetMaxHealth() {
      return unitHealth.MaxHealth;
    }

    public void AfterDamageReceived() {
      Shake();
    }

    private void Shake() {
      ResetShake();

      originalPosition = sprite.position;
      originalScale = sprite.localScale;
      float healthPercentage = GetHealth() / GetMaxHealth();

      cellRenderer = sprite.GetComponent<Renderer>();
      originalSortingOrder = cellRenderer.sortingOrder;
      cellRenderer.sortingOrder += 1;

      float shakeDuration = Mathf.Lerp(cellStats.minShakeDuration, cellStats.maxShakeDuration, 1 - healthPercentage);
      float shakeIntensity = Mathf.Lerp(cellStats.minShakeIntensity, cellStats.maxShakeIntensity, 1 - healthPercentage);
      int vibrato = Mathf.RoundToInt(Mathf.Lerp(cellStats.minVibrato, cellStats.maxVibrato, 1 - healthPercentage));

      float scaleFactorX = Mathf.Lerp(originalScale.x, cellStats.scaleFactor.x, 1 - healthPercentage);
      float scaleFactorY = Mathf.Lerp(originalScale.y, cellStats.scaleFactor.y, 1 - healthPercentage);
      Vector3 shakeScale = originalScale;
      shakeScale.x *= scaleFactorX;
      shakeScale.y *= scaleFactorY;
      sprite.localScale = shakeScale;

      currentShakeTween = sprite.DOShakePosition(shakeDuration, shakeIntensity, vibrato, cellStats.randomness, false, true)
        .OnComplete(() => ResetShake());
    }

    private void ResetShake() {
      if (currentShakeTween == null) {
        return;
      }

      currentShakeTween.Kill();
      sprite.position = originalPosition;
      sprite.localScale = originalScale;
      cellRenderer.sortingOrder = originalSortingOrder;
    }
  }
}