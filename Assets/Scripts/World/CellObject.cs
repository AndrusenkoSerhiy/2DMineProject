using UnityEngine;
using DG.Tweening;
using Player;
using Scriptables;
using Pool;
using UnityEngine.U2D;

namespace World {
  public class CellObject : PoolObjectBase, IDamageable {
    public bool IsActive = false;
    [SerializeField] private CellStats cellStats;
    [SerializeField] private SpriteAtlas atlasRef;
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private ObjectHighlight highlight;
    [SerializeField] private Transform damageOverlay;
    [SerializeField] private Sprite[] damageOverlays;

    public ResourceData resourceData;
    private SpriteRenderer damageOverlayRenderer;
    [SerializeField] private CellData _cellData;
    [SerializeField] private UnitHealth unitHealth;
    private Renderer cellRenderer;
    private Tween currentShakeTween;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private int originalSortingOrder;
    
    //in building prefab boxCollider is disabled by default and enabled 
    //when you place it in the cell
    public BoxCollider2D BoxCollider2D => boxCollider2D;

    public CellData CellData => _cellData;

    public void Init(CellData cellData, ResourceData data) {
      _cellData = cellData;
      resourceData = data;
      InitUnitHealth();
    }

    private void InitUnitHealth() {
      unitHealth = new UnitHealth(resourceData.Durability);
      //if cell have not full hp we need to update overlayDamage for cell
      if (_cellData.durability > 0 && !Mathf.Approximately(resourceData.Durability, _cellData.durability)) {
        unitHealth.SetCurrentHealth(_cellData.durability);
        UpdateDamageOverlay(resourceData.Durability - _cellData.durability); 
      }
      
      unitHealth.OnTakeDamage += AddItemToInventory;
    }

    public void InitSprite() {
      if(resourceData.IsBuilding)
        return;
      
      var neighbourIndex = _cellData.NeighboursIndex;
      var targetSprite = resourceData.Sprite(neighbourIndex); 
      sprite.sprite = atlasRef.GetSprite(targetSprite.name);
      sprite.sortingOrder = resourceData.SortingOrder(neighbourIndex);
      boxCollider2D.offset = resourceData.ColOffset();
      boxCollider2D.size = resourceData.ColSize();
    }

    public bool hasTakenDamage {
      get { return unitHealth.hasTakenDamage; }
      set { unitHealth.hasTakenDamage = value; }
    }

    public void Damage(float damage) {
      unitHealth.TakeDamage(damage);
      UpdateDamageOverlay(damage);
    }

    private void AddItemToInventory(float damage) {
      if (resourceData.ItemData == null) {
        //Debug.LogError($"You need to add itemData in resourceData {resourceData}");
        return;
      }
      _cellData.UpdateDurability(damage);
      GameManager.Instance.PlayerInventory.AddItemToInventory(resourceData.ItemData, (int)damage, transform.position);
    }

    public float GetHealth() {
      return unitHealth.health;
    }

    public float GetMaxHealth() {
      return unitHealth.maxHealth;
    }

    public void DestroyObject() {
      var pos = transform.position;
      ResetShake();
      GameManager.Instance.ChunkController.TriggerCellDestroyed(this);
      GameManager.Instance.CellObjectsPool.ReturnObject(this);
      GameManager.Instance.PoolEffects.SpawnFromPool("CellDestroyDustEffect", pos, Quaternion.identity);
      GameManager.Instance.TaskManager.DelayAsync(
        () => GameManager.Instance.PoolEffects.SpawnFromPool("CellDestroyEffect", pos, Quaternion.identity), 0.25f);
      highlight.ClearHighlight();
    }

    public void AfterDamageReceived() {
      //Shake();
    }

    private void Shake() {
      ResetShake();

      originalPosition = sprite.transform.position;
      originalScale = sprite.transform.localScale;
      float healthPercentage = GetHealth() / GetMaxHealth();

      cellRenderer = GetCellRenderer();
      originalSortingOrder = cellRenderer.sortingOrder;
      cellRenderer.sortingOrder += 1;

      GetDamageOverlayRenderer().sortingOrder = originalSortingOrder + 2;

      float shakeDuration = Mathf.Lerp(cellStats.minShakeDuration, cellStats.maxShakeDuration, 1 - healthPercentage);
      float shakeIntensity = Mathf.Lerp(cellStats.minShakeIntensity, cellStats.maxShakeIntensity, 1 - healthPercentage);
      int vibrato = Mathf.RoundToInt(Mathf.Lerp(cellStats.minVibrato, cellStats.maxVibrato, 1 - healthPercentage));

      float scaleFactorX = Mathf.Lerp(originalScale.x, cellStats.scaleFactor.x, 1 - healthPercentage);
      float scaleFactorY = Mathf.Lerp(originalScale.y, cellStats.scaleFactor.y, 1 - healthPercentage);
      Vector3 shakeScale = originalScale;
      shakeScale.x *= scaleFactorX;
      shakeScale.y *= scaleFactorY;
      sprite.transform.localScale = shakeScale;

      currentShakeTween = sprite.transform
        .DOShakePosition(shakeDuration, shakeIntensity, vibrato, cellStats.randomness, false, true)
        .OnComplete(() => ResetShake());
    }

    private void ResetShake() {
      if (currentShakeTween == null) {
        return;
      }

      currentShakeTween.Kill();
      currentShakeTween = null;
      sprite.transform.position = originalPosition;
      sprite.transform.localScale = originalScale;
      cellRenderer.sortingOrder = originalSortingOrder;
      GetDamageOverlayRenderer().sortingOrder = originalSortingOrder + 1;
    }

    public void ResetAll() {
      ResetShake();
      damageOverlay.gameObject.SetActive(false);
    }

    private Renderer GetCellRenderer() {
      if (cellRenderer == null) {
        return sprite.GetComponent<Renderer>();
      }

      return cellRenderer;
    }

    private void UpdateDamageOverlay(float damage) {
      if (Mathf.Approximately(_cellData.durability, unitHealth.maxHealth)) {
        return;
      }
      
      damageOverlayRenderer = GetDamageOverlayRenderer();

      float healthPercentage = _cellData.durability / unitHealth.maxHealth; //unitHealth.health / unitHealth.maxHealth;
      int hpSteps = (int)(unitHealth.maxHealth / damage);

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