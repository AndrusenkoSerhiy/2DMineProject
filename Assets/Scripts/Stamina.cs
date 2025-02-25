using Player;
using Settings;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace {
  public class Stamina : MonoBehaviour {
    [SerializeField] private StaminaBar staminaBar;
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina;
    [SerializeField] private float staminaDrain = 20f;
    [SerializeField] private float staminaRecovery = 10f;
    [Tooltip("min amount of stamina when we can start sprinting")][SerializeField] private float minStamina = 10f;
    [SerializeField] private bool isSprinting;
    private PlayerController playerController;
    private float previousStamina;
    public bool IsSprinting => isSprinting;


    private void Start() {
      currentStamina = maxStamina;
      staminaBar.SetMaxStamina(maxStamina);
      UserInput.instance.controls.GamePlay.Sprint.performed += SprintPerformed;
      UserInput.instance.controls.GamePlay.Sprint.canceled += SprintCanceled;
      playerController = GameManager.Instance.PlayerController;
    }
    
    private void SprintPerformed(InputAction.CallbackContext context) {
      SetSprinting(true);
    }

    private void SprintCanceled(InputAction.CallbackContext context) {
      SetSprinting(false);
    }

    private void SetSprinting(bool value) {
      //block use stamina if she not enough 
      if (value && (currentStamina < minStamina || Mathf.Sign(playerController.GetMoveForward()) < 0) ||
          UserInput.instance.GetMovement().Equals(Vector2.zero) ||
          !playerController.Grounded && !playerController.WasSprintingOnJump)
        
        return;
      
      isSprinting = value;
    }

    private void Update() {
      UpdateStaminaValue();
      UpdateStaminaUI();
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
      UserInput.instance.controls.GamePlay.Sprint.performed -= SprintPerformed;
      UserInput.instance.controls.GamePlay.Sprint.canceled -= SprintCanceled;
    }
  }
}