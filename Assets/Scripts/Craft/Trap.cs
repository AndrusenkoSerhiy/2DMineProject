using System.Collections.Generic;
using Interaction;
using Scriptables;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Craft {
  public class Trap : MonoBehaviour, IInteractable, IBaseCellHolder {
    [SerializeField] private string holdInteractText;
    [SerializeField] private bool hasHoldInteraction = true;
    [SerializeField] private BuildingDataObject buildObject;
    [SerializeField] protected ItemObject itemObject;
    [SerializeField] private UnitHealth unitHealth;
    [SerializeField] private int startHealth;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AudioData damageAudioData;
    [SerializeField] private Color destroyEffectColor = new(148, 198, 255, 255);
    [SerializeField] private Recipe trapRecipe;
    [SerializeField] private bool canDestroyCellsBelow = true;

    public string InteractionText { get; }
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;
    public string HoldProcessText => holdInteractText;

    private CellHolderHandler cellHandler;
    protected GameManager gameManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake() {
      gameManager = GameManager.Instance;
      cellHandler = new CellHolderHandler(OnAllBaseCellsDestroyed, trapRecipe, transform.position);
    }

    void Start() {
      InitUnitHealth();
    }

    private void InitUnitHealth() {
      unitHealth = new UnitHealth(startHealth);
    }

    private void OnAllBaseCellsDestroyed() {
      Destroy();
    }

    private void Destroy() {
      var psGo = GameManager.Instance.PoolEffects
        .SpawnFromPool("CellDestroyEffect", transform.position, Quaternion.identity)
        .gameObject;
      var ps = psGo.GetComponent<ParticleSystem>();

      if (ps != null) {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(destroyEffectColor);
      }

      gameManager.PlaceCell.RemoveBuilding(buildObject, itemObject);
      gameManager.AudioController.PlayWorkstationDestroyed();
      unitHealth.SetCurrentHealth(startHealth);
    }
    
    public void Damage(int damage, bool isPlayer) {
      unitHealth.TakeDamage(damage, isPlayer);
      gameManager.AudioController.PlayAudio(damageAudioData);
      if (GetHealth() <= 0) {
        Destroy();
      }
    }
    
    public float GetHealth() {
      return unitHealth.health;
    }


    public bool Interact(PlayerInteractor playerInteractor) {
      return false;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      GameManager.Instance.PlayerInventory.TakeBuildingToInventory(buildObject, itemObject);
      return true;
    }

    public Bounds GetBounds() {
      return spriteRenderer ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.zero);
    }

    public bool CanDestroyCellsBelow { get; set; }

    public void SetBaseCells(List<CellData> cells) {
      CanDestroyCellsBelow = canDestroyCellsBelow;
      cellHandler.SetBaseCells(cells, transform.position, CanDestroyCellsBelow);
    }

    public void ClearBaseCells() {
      cellHandler.ClearBaseCells();
    }
  }
}