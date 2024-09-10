using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Settings {
  public class UserInput : MonoBehaviour {
    private static UserInput _instance;
    public static UserInput instance {
      get { return _instance; }
    }

    [HideInInspector] public Controls controls;

    private bool attacking;

    private void Awake() {
      if (_instance != null && _instance != this) {
        Destroy(this.gameObject);
      }
      else {
        _instance = this;
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
}
