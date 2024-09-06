using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour {
  private static UserInput instance;
  public static UserInput Instance {
    get { return instance; }
  }

  [HideInInspector] public Controls controls;

  private bool attacking;

  private void Awake() {
    if (instance != null && instance != this) {
      Destroy(this.gameObject);
    }
    else {
      instance = this;
    }

    controls = new Controls();

    controls.GamePlay.Attack.performed += AttackPerformed;
    controls.GamePlay.Attack.canceled += AttackCanceled;
  }

  public bool IsAttacking() {
    return attacking;
  }

  private void AttackPerformed(InputAction.CallbackContext context) {
    attacking = true;
  }

  private void AttackCanceled(InputAction.CallbackContext context) {
    attacking = false;
  }

  private void OnEnable() {
    controls?.Enable();
  }

  private void OnDisable() {
    controls?.Disable();
  }
}
