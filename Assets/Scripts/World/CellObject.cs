using UnityEngine;
using DG.Tweening;
using System;
using Scriptables;

namespace World {
  public class CellObject : MonoBehaviour, IDamageable {
    [SerializeField] private CellStats cellStats;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Transform damageOverlay;
    [SerializeField] private Sprite[] damageOverlays;

    private ResourceData resourceData;
    private SpriteRenderer damageOverlayRenderer;
    private CellData _cellData;
    private UnitHealth unitHealth;
    private Renderer cellRenderer;
    private Tween currentShakeTween;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private int originalSortingOrder;

    public void Init(ResourceData data) {
      resourceData = data;
      unitHealth = new UnitHealth(resourceData.Durability);
      sprite.sprite = data.Sprite;
    }

    public bool HasTakenDamage {
      get { return unitHealth.HasTakenDamage; }
      set { unitHealth.HasTakenDamage = value; }
    }

    public void Damage(float damage) {
      unitHealth.TakeDamage(damage);

      UpdateDamageOverlay(damage);
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

      originalPosition = sprite.transform.position;
      originalScale = sprite.transform.localScale;
      float healthPercentage = GetHealth() / GetMaxHealth();

      cellRenderer = GetCellRenderer();
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
      sprite.transform.localScale = shakeScale;

      currentShakeTween = sprite.transform.DOShakePosition(shakeDuration, shakeIntensity, vibrato, cellStats.randomness, false, true)
        .OnComplete(() => ResetShake());
    }

    private void ResetShake() {
      if (currentShakeTween == null) {
        return;
      }

      currentShakeTween.Kill();
      sprite.transform.position = originalPosition;
      sprite.transform.localScale = originalScale;
      cellRenderer.sortingOrder = originalSortingOrder;
    }

    private Renderer GetCellRenderer() {
      if (cellRenderer == null) {
        return sprite.GetComponent<Renderer>();
      }
      return cellRenderer;
    }

    private void UpdateDamageOverlay(float damage) {
      if (unitHealth.Health == unitHealth.MaxHealth) {
        return;
      }
      damageOverlayRenderer = GetDamageOverlayRenderer();

      float healthPercentage = unitHealth.Health / unitHealth.MaxHealth;
      int hpSteps = (int)(unitHealth.MaxHealth / damage);

      int overlayIndex;

      if (hpSteps >= damageOverlays.Length) {
        // Normal case: show one overlay for each HP step
        overlayIndex = Mathf.FloorToInt((1f - healthPercentage) * damageOverlays.Length);
      }
      else {
        // Calculate the overlay index by skipping initial overlays
        int skippedOverlays = damageOverlays.Length - hpSteps;
        overlayIndex = skippedOverlays + Mathf.FloorToInt((1f - healthPercentage) * hpSteps);
      }

      // Ensure the overlayIndex is within the valid range
      overlayIndex = Mathf.Clamp(overlayIndex, 0, damageOverlays.Length - 1);

      if (overlayIndex < damageOverlays.Length && overlayIndex >= 0) {
        damageOverlayRenderer.sprite = damageOverlays[overlayIndex];
        damageOverlay.gameObject.SetActive(true);
      }
      else {
        damageOverlayRenderer.sprite = null;
        damageOverlay.gameObject.SetActive(false);
      }
    }

    private SpriteRenderer GetDamageOverlayRenderer() {
      if (damageOverlayRenderer == null) {
        return damageOverlay.GetComponent<SpriteRenderer>();
      }
      return damageOverlayRenderer;
    }
  }
}