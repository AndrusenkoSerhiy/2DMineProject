using Settings;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class StaminaBase : MonoBehaviour {
  [SerializeField] protected StaminaBar staminaBar;
  [SerializeField] protected float maxStamina = 100f;
  [SerializeField] protected float currentStamina;
  [SerializeField] protected float staminaDrain = 20f;
  [SerializeField] protected float staminaRecovery = 10f;

  [Tooltip("min amount of stamina when we can start sprinting")] [SerializeField]
  protected float minStamina = 10f;

  [SerializeField] protected bool isSprinting;
    
  protected float previousStamina;
  protected UserInput userInput;
  public bool IsSprinting => isSprinting;
    
  public virtual void Start() {
    currentStamina = maxStamina;
    staminaBar.SetMaxStamina(maxStamina);
    userInput = GameManager.Instance.UserInput; 
    userInput.controls.GamePlay.Sprint.performed += SprintPerformed;
    userInput.controls.GamePlay.Sprint.canceled += SprintCanceled;
  }

  public void EnableSprintScript(bool state) {
    enabled = state;
  }
  public void SetStaminaBarRef() {
    staminaBar = GameManager.Instance.StaminaBar;
  }
    
  private void Update() {
    StopSprinting();
    UpdateStaminaValue();
    UpdateStaminaUI();
  }
    
  private void SprintPerformed(InputAction.CallbackContext context) => SetSprinting(true);

  private void SprintCanceled(InputAction.CallbackContext context) => SetSprinting(false);

  public virtual void SetSprinting(bool value) {
    isSprinting = value;
  }
    
  private void StopSprinting() {
    if (GameManager.Instance.UserInput.GetMovement().magnitude <=0) {
      SetSprinting(false);
    }
  }

  private void UpdateStaminaValue() {
    previousStamina = currentStamina;
    if (isSprinting && currentStamina > 0) {
      currentStamina -= staminaDrain * Time.deltaTime;
    }
    else {
      currentStamina += staminaRecovery * Time.deltaTime;
    }

    currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
    if (currentStamina <= 0) {
      SetSprinting(false);
    }
  }
    
  private void UpdateStaminaUI() {
    if (!Mathf.Approximately(previousStamina, currentStamina)) {
      staminaBar.SetStamina(currentStamina);
    }
  }
    
  private void OnDestroy() {
    if (!GameManager.HasInstance || userInput == null)
      return;
      
    userInput.controls.GamePlay.Sprint.performed -= SprintPerformed;
    userInput.controls.GamePlay.Sprint.canceled -= SprintCanceled;
  }
}