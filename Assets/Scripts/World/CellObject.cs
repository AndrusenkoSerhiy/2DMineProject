using UnityEngine;
using DG.Tweening;
using Player;
using Scriptables;
using Pool;
using UnityEngine.U2D;
using Utils;

namespace World {
  public class CellObject : PoolObjectBase, IDamageable {
    public bool IsActive = false;
    [SerializeField] private CellStats cellStats;
    [SerializeField] private SpriteAtlas atlasRef;
    public SpriteRenderer sprite;
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private ObjectHighlight highlight;
    [SerializeField] private Transform damageOverlay;
    [SerializeField] private Sprite[] damageOverlays;

    public DamageableType DamageableType { get; set; }
    public AudioData OnTakeDamageAudioData { get; set; }
    public bool CanGetDamage { get; set; }

    public ResourceData resourceData;
    private SpriteRenderer damageOverlayRenderer;
    [SerializeField] private CellData _cellData;
    [SerializeField] private UnitHealth unitHealth;
    private Renderer cellRenderer;
    private Tween currentShakeTween;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private int originalSortingOrder;

    //for spawn drop count
    private float resourcePerDurability;
    private float fractionalResource;

    //in building prefab boxCollider is disabled by default and enabled 
    //when you place it in the cell
    public BoxCollider2D BoxCollider2D => boxCollider2D;

    public CellData CellData => _cellData;

    public void Init(CellData cellData, ResourceData data) {
      DamageableType = DamageableType.Cell;
      if (data.OnTakeDamageAudioData) {
        OnTakeDamageAudioData = data.OnTakeDamageAudioData;
      }

      _cellData = cellData;
      resourceData = data;
      if (GameManager.Instance.ChunkSpecialPointsSpawner.CheckData(data, this)) return;
      CanGetDamage = resourceData.CanTakeDamage;
      InitUnitHealth();
    }

    private void InitUnitHealth() {
      unitHealth = new UnitHealth(resourceData.Durability);
      //if cell have not full hp we need to update overlayDamage for cell
      if (_cellData.durability > 0 && !Mathf.Approximately(resourceData.Durability, _cellData.durability)) {
        unitHealth.SetCurrentHealth(_cellData.durability);
        UpdateDamageOverlay(resourceData.Durability - _cellData.durability);
      }

      resourcePerDurability = (float)resourceData.DropCount / resourceData.Durability;
      //Debug.LogError($"resourceData {resourceData.name} | DropCount {resourceData.DropCount} | resourceData.Durability {resourceData.Durability} | resourcePerDurability {resourcePerDurability}");
      fractionalResource = _cellData.fractionalResource;
      unitHealth.OnTakeDamage += UpdateDurability;
      unitHealth.OnTakeDamage += AddItemToInventory;
    }

    public void InitSprite() {
      //if cell data null the all method is broke
      //and after save the visible area don't change when we move
      if(_cellData == null) 
        return;
      
      var neighbourIndex = _cellData.NeighboursIndex;
      var targetSprite = resourceData.Sprite(neighbourIndex);
      sprite.sprite = atlasRef.GetSprite(targetSprite.name);
      sprite.sortingOrder = resourceData.SortingOrder(neighbourIndex);
      boxCollider2D.offset = resourceData.ColOffset();
      boxCollider2D.size = resourceData.ColSize();
    }

    public void Damage(float damage, bool isPlayer) {
      DamageAudio();
      if (!CanGetDamage)
        return;
      unitHealth.TakeDamage(damage, isPlayer);
      UpdateDamageOverlay(damage);
    }

    private void DamageAudio() {
      if (!OnTakeDamageAudioData) {
        return;
      }

      GameManager.Instance.AudioController.PlayAudio(OnTakeDamageAudioData, transform.position);
    }

    private void UpdateDurability(float damage, bool isPlayer) {
      //if zombie attack cells and then player moving away
      //and cell disabled by distance
      if (_cellData == null)
        return;
        
      _cellData.UpdateDurability(damage);
    }

    private void AddItemToInventory(float damage, bool isPlayer) {
      if (resourceData.ItemData == null) {
        //Debug.LogError($"You need to add itemData in resourceData {resourceData}");
        return;
      }

      if (!isPlayer)
        return;

      CalculateCountToSpawn(damage);
      GameManager.Instance.ChunkController.AfterCellChanged(_cellData);
    }

    private void CalculateCountToSpawn(float damage) {
      var totalResourceGained = damage * resourcePerDurability + 0.001f;
      fractionalResource += totalResourceGained;
      int integerResource = (int)fractionalResource;
      fractionalResource -= integerResource;
      _cellData.alreadyDroped += integerResource;
      _cellData.fractionalResource = fractionalResource;
      if (integerResource <= 0) return;
      GameManager.Instance.PlayerInventory.AddItemToInventory(resourceData.ItemData, integerResource,
        transform.position);
      if (_cellData.durability <= 0) {
        GameManager.Instance.PlayerInventory.AddAdditionalItem(resourceData, transform.position);
        //if cell is destroyed need to check for spawn left item
        if (resourceData.DropCount - _cellData.alreadyDroped > 0) {
          GameManager.Instance.PlayerInventory.AddItemToInventory(resourceData.ItemData,
            resourceData.DropCount - _cellData.alreadyDroped, transform.position);
        }
      }
    }

    public float GetHealth() {
      return unitHealth.health;
    }

    public Vector3 GetPosition() {
      return transform.position;
    }

    public string GetName() {
      return gameObject.name;
    }

    public float GetMaxHealth() {
      return unitHealth.maxHealth;
    }

    public void DestroyObject() {
      //GameManager.Instance.QuestManager.StartQuest(1);
      var pos = transform.position;
      var psGo = GameManager.Instance.PoolEffects.SpawnFromPool("CellDestroyEffect", pos, Quaternion.identity)
        .gameObject;
      ParticleSystem ps = psGo.GetComponent<ParticleSystem>();
      //ps.startColor = resourceData.EffectColor;
      if (ps != null) {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(resourceData.EffectColor);
      }

      highlight.SetHighlight(false);
      
      GameManager.Instance.ChunkController.TriggerCellDestroyed(this);
      GameManager.Instance.CellObjectsPool.ReturnObject(this);
    }

    public void DestroySilent() {
      GameManager.Instance.ChunkController.TriggerCellDestroyed(this, true);
      GameManager.Instance.CellObjectsPool.ReturnObject(this);
    }

    public void AfterDamageReceived() {
      var pos = transform.position;
      var psGo = GameManager.Instance.PoolEffects.SpawnFromPool("CellDamageEffect", pos, Quaternion.identity)
        .gameObject;
      var ps = psGo.GetComponent<ParticleSystem>();
      if (ps != null) {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(resourceData.EffectColor);
      }
    }

    public void ResetAll() {
      _cellData = null;
      resourceData = null;
      CanGetDamage = true;
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