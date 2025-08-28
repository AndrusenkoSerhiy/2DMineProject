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
    [SerializeField] private AudioData openDoorAudioData;
    [SerializeField] private AudioData closeDoorAudioData;
    [SerializeField] private SpriteRenderer spriteRenderer;
    public string InteractionText => IsOpened ? interactCloseText : interactOpenText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;

    public Bounds GetBounds() {
      return spriteRenderer ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.zero);
    }

    public bool IsOpen => IsOpened;

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
      }
    }

    public float GetHealth() {
      return unitHealth.health;
    }

    public Vector3 GetPosition() {
      return transform.position;
    }

    public string GetName() {
      return name;
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