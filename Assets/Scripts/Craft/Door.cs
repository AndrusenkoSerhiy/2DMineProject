using System;
using Interaction;
using Scriptables;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Craft {
  public class Door : MonoBehaviour, IInteractable, IDamageable {
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
    public string InteractionText => IsOpened ? interactCloseText : interactOpenText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;

    public bool hasTakenDamage {
      get { return unitHealth.hasTakenDamage; }
      set { unitHealth.hasTakenDamage = value; }
    }
    private void Start() {
      InitUnitHealth();
    }
    
    private void InitUnitHealth() {
      unitHealth = new UnitHealth(startHealth);
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      if (IsOpened) {
        animator.SetBool("IsOpened", false);
        IsOpened = false;
        boxCollider.enabled = true;
      }
      else {
        animator.SetBool("IsOpened", true);
        IsOpened = true;
        boxCollider.enabled = false;
      }

      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      GameManager.Instance.PlayerInventory.TakeBuildingToInventory(buildObject, itemObject);
      return true;
    }

    public DamageableType DamageableType { get; set; }
    public AudioData OnTakeDamageAudioData { get; set; }
    public void Damage(float damage, bool isPlayer) {
      unitHealth.TakeDamage(damage, isPlayer);
      if (GetHealth() <= 0) {
        GameManager.Instance.ChunkController.RemoveBuild(buildObject);
      }
    }

    public float GetHealth() {
      return unitHealth.health;
    }

    public float GetMaxHealth() {
      return unitHealth.maxHealth;
    }

    public void AfterDamageReceived() {
      
    }

    public void DestroyObject() {
      
    }
  }
}