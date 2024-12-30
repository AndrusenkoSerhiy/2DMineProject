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
    
    public bool IsSprinting => isSprinting;


    private void Start() {
      currentStamina = maxStamina;
      staminaBar.SetMaxStamina(maxStamina);
      UserInput.instance.controls.GamePlay.Sprint.performed += SprintPerformed;
      UserInput.instance.controls.GamePlay.Sprint.canceled += SprintCanceled;
    }
    
    private void SprintPerformed(InputAction.CallbackContext context) {
      SetSprinting(true);
    }

    private void SprintCanceled(InputAction.CallbackContext context) {
      SetSprinting(false);
    }

    private void SetSprinting(bool value) {
      //block use stamina if she not enough 
      if (value && (currentStamina < minStamina || Mathf.Sign(GameManager.instance.PlayerController.GetMoveForward()) < 0) ||
          UserInput.instance.GetMovement().Equals(Vector2.zero))
        
        return;
      
      isSprinting = value;
    }

    private void Update() {
      if (isSprinting && currentStamina > 0) {
        currentStamina -= staminaDrain * Time.deltaTime;
      }
      else {
        currentStamina += staminaRecovery * Time.deltaTime;
      }

      // Clamp stamina between 0 and maxStamina
      currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
      if (currentStamina <= 0) {
        SetSprinting(false);
      }
      // Update the UI
      staminaBar.SetStamina(currentStamina);
    }

    private void OnDestroy() {
      UserInput.instance.controls.GamePlay.Sprint.performed -= SprintPerformed;
      UserInput.instance.controls.GamePlay.Sprint.canceled -= SprintCanceled;
    }
  }
}