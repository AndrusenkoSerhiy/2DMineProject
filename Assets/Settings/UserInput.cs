using System;
using System.Collections.Generic;
using Scriptables;
using Scriptables.Stats;
using Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Settings {
  public class UserInput : MonoBehaviour {
    public event EventHandler OnAttackPerformed;
    public event EventHandler OnAttackCanceled;
    public event EventHandler OnGameDeviceChanged;

    [HideInInspector] public Controls controls;

    private bool attacking;
    // [SerializeField] private PlayerStats _stats;
    [SerializeField] private PlayerStatsObject statsObject;
    [SerializeField] private VirtualMouseUI _virtualMouse;
    private Dictionary<string, List<string>> blockedActions = new ();

    public enum GameDevice {
      KeyboardAndMouse = 0,
      Gamepad = 1,
    }

    private GameDevice _activeGameDevice;

    public GameDevice ActiveGameDevice => _activeGameDevice;
    private void Awake() {
      controls = new Controls();

      controls.GamePlay.Attack.performed += AttackPerformed;
      controls.GamePlay.Attack.canceled += AttackCanceled;
      //InputSystem.onActionChange += InputActionChangeCallback;
      SubscribeToChangeInputType();
    }

    //check what a key is pressed and change activeDevice
    private void SubscribeToChangeInputType() {
      controls.InputType.Keyboard.performed += ctx => ChangeActiveGameDevice(GameDevice.KeyboardAndMouse);
      controls.InputType.Gamepad.performed += ctx => ChangeActiveGameDevice(GameDevice.Gamepad);
    }

    public void BlockAction(string actionName, string reason) {
      if (!blockedActions.ContainsKey(actionName)) {
        blockedActions[actionName] = new List<string>();
      }

      if (!blockedActions[actionName].Contains(reason)) {
        blockedActions[actionName].Add(reason);
        var action = controls.FindAction(actionName);
        action.Disable();
      }
    }

    public void UnblockAction(string actionName, string reason) {
      if (!blockedActions.ContainsKey(actionName)) return;

      blockedActions[actionName].Remove(reason);

      if (blockedActions[actionName].Count == 0) {
        blockedActions.Remove(actionName);
        var action = controls.FindAction(actionName);
        action.Enable();
      }
    }

    /*private void InputActionChangeCallback(object arg1, InputActionChange inputActionChange) {
      if (inputActionChange == InputActionChange.ActionPerformed && arg1 is InputAction) {
        //
        InputAction inputAction = arg1 as InputAction;
        //Debug.LogError($"InputActionChangeCallback {inputAction.activeControl.device.displayName}");
        if (inputAction.activeControl.device.displayName == "VirtualMouse") {
          return;
        }

        if (inputAction.activeControl.device is Gamepad) {
          //(lastDevice.name.Equals("Keyboard") || lastDevice.name.Equals("Mouse")){//
          if (_activeGameDevice != GameDevice.Gamepad) {
            ChangeActiveGameDevice(GameDevice.Gamepad);
          }
        }
        else {
          if (_activeGameDevice != GameDevice.KeyboardAndMouse) {
            ChangeActiveGameDevice(GameDevice.KeyboardAndMouse);
          }
        }
      }
    }*/

    private void ChangeActiveGameDevice(GameDevice activeGameDevice) {
      if(_activeGameDevice == activeGameDevice)
        return;
      
      _activeGameDevice = activeGameDevice;
      //Debug.LogError($"activeGameDevice {activeGameDevice} | _activeGameDevice {_activeGameDevice}");
      Cursor.visible = _activeGameDevice == GameDevice.KeyboardAndMouse;

      OnGameDeviceChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ShowCursor(bool state) {
      if (state && _activeGameDevice != GameDevice.KeyboardAndMouse)
        return;
      
      Cursor.visible = state;
    }

    public GameDevice GetActiveGameDevice() {
      return _activeGameDevice;
    }

    public Vector3 GetMousePosition() {
      return _activeGameDevice == GameDevice.KeyboardAndMouse ? Input.mousePosition : _virtualMouse.GetVirtualMousePosition();
    }

    public Vector2 GetMovement() {
      var movement = controls.GamePlay.Movement.ReadValue<Vector2>();
      if (statsObject.snapInput) {
        movement.x = Mathf.Abs(movement.x) < statsObject.horizontalDeadZoneThreshold ? 0 : Mathf.Sign(movement.x);
        movement.y = Mathf.Abs(movement.y) < statsObject.verticalDeadZoneThreshold ? 0 : Mathf.Sign(movement.y);
      }

      //Debug.LogError($"movement {movement}");
      return movement;
    }

    #region Attack

    public bool IsAttacking() {
      return attacking;
    }

    private void AttackPerformed(InputAction.CallbackContext context) {
      attacking = true;
      OnAttackPerformed?.Invoke(this, EventArgs.Empty);
    }

    private void AttackCanceled(InputAction.CallbackContext context) {
      attacking = false;
      OnAttackCanceled?.Invoke(this, EventArgs.Empty);
    }

    #endregion

    #region Controls

    public void EnableUIControls(bool val) {
      if (val) {
        controls.UI.Enable();
      }
      else {
        controls.UI.Disable();
      }
    }

    public void EnableGamePlayControls(bool val) {
      if (val) {
        EnableAllExceptBlocked();
      }
      else {
        controls.GamePlay.Disable();
      }
    }

    void EnableAllExceptBlocked() {
      var gameplayMap = controls.GamePlay.Get();

      foreach (var action in gameplayMap) {
        if (!blockedActions.ContainsKey(action.name)) {
          action.Enable();
        }
      }
    }

    #endregion


    private void OnEnable() {
      controls?.Enable();
    }

    private void OnDisable() {
      controls?.Disable();
      //InputSystem.onActionChange -= InputActionChangeCallback;
    }
  }
}