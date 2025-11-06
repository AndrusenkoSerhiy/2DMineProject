using System;
using System.Collections.Generic;
using Interaction;
using Scriptables;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Craft {
  public class Door : MonoBehaviour, IInteractable, IDamageable, IBaseCellHolder {
    [SerializeField] private string interactOpenText;
    [SerializeField] private string interactCloseText;
    [SerializeField] private string holdInteractText;
    [SerializeField] private bool hasHoldInteraction = true;
    [SerializeField] private bool IsOpened = false;
    [SerializeField] private Animator animator;
    [SerializeField] private BuildingDataObject buildObject;
    [SerializeField] protected ItemObject itemObject;
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private UnitHealth unitHealth;
    [SerializeField] private int startHealth;
    [SerializeField] private AudioData openDoorAudioData;
    [SerializeField] private AudioData closeDoorAudioData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color destroyEffectColor = new(148, 198, 255, 255);
    [SerializeField] private Recipe stationRecipe;
    public string InteractionText => IsOpened ? interactCloseText : interactOpenText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;
    public string HoldProcessText => holdInteractText;

    public Bounds GetBounds() {
      return spriteRenderer ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.zero);
    }

    private CellHolderHandler cellHandler;
    public bool IsOpen => IsOpened;
    protected GameManager gameManager;
    
    private void Awake() {
      gameManager = GameManager.Instance;
      cellHandler = new CellHolderHandler(OnAllBaseCellsDestroyed, stationRecipe, transform.position);
    }
    private void Start() {
      DamageableType = DamageableType.Door;
      CanGetDamage = true;
      InitUnitHealth();
    }

    private void InitUnitHealth() {
      unitHealth = new UnitHealth(startHealth);
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      if (IsOpened) {
        animator.SetBool("IsOpened", false);
        CanGetDamage = true;
        IsOpened = false;
        boxCollider.enabled = true;
        GameManager.Instance.AudioController.PlayAudio(closeDoorAudioData);
      }
      else {
        animator.SetBool("IsOpened", true);
        CanGetDamage = false;
        IsOpened = true;
        boxCollider.enabled = false;
        GameManager.Instance.AudioController.PlayAudio(openDoorAudioData);
      }

      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      GameManager.Instance.PlayerInventory.TakeBuildingToInventory(buildObject, itemObject);
      return true;
    }

    public DamageableType DamageableType { get; set; }
    public AudioData OnTakeDamageAudioData { get; set; }
    public bool CanGetDamage { get; set; }

    public void Damage(float damage, bool isPlayer) {
      unitHealth.TakeDamage(damage, isPlayer);
      if (GetHealth() <= 0) {
        GameManager.Instance.ChunkController.RemoveBuild(buildObject);
        unitHealth.SetCurrentHealth(startHealth);
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

    public void AfterDamageReceived() {
    }

    public void DestroyObject() {
    }

    public void SetBaseCells(List<CellData> cells) {
      cellHandler.SetBaseCells(cells, transform.position);
    }
    
    private void OnAllBaseCellsDestroyed() {
      var psGo = GameManager.Instance.PoolEffects
        .SpawnFromPool("CellDestroyEffect", transform.position, Quaternion.identity)
        .gameObject;
      var ps = psGo.GetComponent<ParticleSystem>();

      if (ps != null) {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(destroyEffectColor);
      }

      gameManager.PlaceCell.RemoveBuilding(buildObject, itemObject);
      gameManager.MessagesManager.ShowSimpleMessage("Door destroyed");
      gameManager.AudioController.PlayWorkstationDestroyed();
    }

    public void ClearBaseCells() {
      cellHandler.ClearBaseCells();
    }
  }
}