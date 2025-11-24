using Player;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem;

public class StaminaBase : MonoBehaviour {
  protected PlayerStats stats;

  [Tooltip("min amount of stamina when we can start sprinting")] [SerializeField]
  protected float minStamina = 10f;

  [SerializeField] protected bool isSprinting;

  protected UserInput userInput;
  public bool IsSprinting => isSprinting;

  public virtual void Start() {
    stats = GetComponent<PlayerControllerBase>().PlayerStats;
    userInput = GameManager.Instance.UserInput;
    userInput.controls.GamePlay.Sprint.performed += SprintPerformed;
    userInput.controls.GamePlay.Sprint.canceled += SprintCanceled;
  }
  
  private bool IsMoving() {
    var isMove = !userInput.GetMovement().Equals(Vector2.zero);
    return isMove;
  }

  public void EnableSprintScript(bool state) {
    enabled = state;
  }

  private void Update() {
    StopSprinting();
    UpdateStaminaValue();
  }

  private void SprintPerformed(InputAction.CallbackContext context) => SetSprinting(true);

  private void SprintCanceled(InputAction.CallbackContext context) => SetSprinting(false);

  protected virtual void SetSprinting(bool value) {
    isSprinting = value;
    stats.StaminaInUse = value;
  }

  private void StopSprinting() {
    return;
    if (GameManager.Instance.UserInput.GetMovement().magnitude <= 0) {
      SetSprinting(false);
    }
  }

  private void UpdateStaminaValue() {
    if (isSprinting && stats.Stamina > 0 && IsMoving()) {
      stats.UseStamina(Time.deltaTime);
    }

    if (stats.Stamina <= 0) {
      SetSprinting(false);
    }
  }

  private void OnDestroy() {
    if (!GameManager.HasInstance || userInput == null) {
      return;
    }

    userInput.controls.GamePlay.Sprint.performed -= SprintPerformed;
    userInput.controls.GamePlay.Sprint.canceled -= SprintCanceled;
  }
}