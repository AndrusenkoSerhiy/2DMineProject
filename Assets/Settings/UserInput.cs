using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Scriptables;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Settings {
  public class UserInput : MonoBehaviour {
    private static UserInput _instance;
    public static UserInput instance {
      get { return _instance; }
    }

    public event EventHandler OnGameDeviceChanged;
    public event EventHandler OnBuildClick;

    [HideInInspector] public Controls controls;

    private bool attacking;
    [SerializeField] private PlayerStats _stats;
    [SerializeField] private VirtualMouseUI _virtualMouse;
    [SerializeField] private bool _isBuildMode;
    public bool IsBuildMode => _isBuildMode;
    
    public enum GameDevice{
       KeyboardAndMouse = 0,
       Gamepad =1,
    }

    private GameDevice _activeGameDevice;
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
      SubscribeBuildClick();
      InputSystem.onActionChange += InputActionChangeCallback;
    }
    
    private void SubscribeBuildClick() {
      controls.GamePlay.Build.performed += BuildClickPerformed;
    }
    
    private void UnsubscribeBuidClick() {
      controls.GamePlay.Build.performed -= BuildClickPerformed;
    }
    private void BuildClickPerformed(InputAction.CallbackContext context) {
      _isBuildMode = !_isBuildMode;
      OnBuildClick?.Invoke(this, EventArgs.Empty);
    }
    
    
    private void InputActionChangeCallback(object arg1, InputActionChange inputActionChange) {
      if (inputActionChange == InputActionChange.ActionPerformed && arg1 is InputAction) {//
        InputAction inputAction = arg1 as InputAction;
        //Debug.LogError($"InputActionChangeCallback {inputAction.activeControl.device.displayName}");
        if(inputAction.activeControl.device.displayName == "VirtualMouse"){
            return;
        }

        if(inputAction.activeControl.device is Gamepad){//(lastDevice.name.Equals("Keyboard") || lastDevice.name.Equals("Mouse")){//
          if(_activeGameDevice != GameDevice.Gamepad){
            ChangeActiveGameDevice(GameDevice.Gamepad);
          }          
        } else {
          if(_activeGameDevice != GameDevice.KeyboardAndMouse){
            ChangeActiveGameDevice(GameDevice.KeyboardAndMouse);
          }
        }
      }
    }

    private void ChangeActiveGameDevice(GameDevice activeGameDevice){
      
      _activeGameDevice = activeGameDevice;
      //Debug.LogError($"activeGameDevice {activeGameDevice}");
      UnityEngine.Cursor.visible = _activeGameDevice == GameDevice.KeyboardAndMouse;

      OnGameDeviceChanged?.Invoke(this, EventArgs.Empty);
    }

    public GameDevice GetActiveGameDevice(){
      return _activeGameDevice;
    }

    public Vector3 GetMousePosition(){
      return _activeGameDevice == GameDevice.KeyboardAndMouse ? Input.mousePosition : _virtualMouse.VirtualMousePos;
      //return _virtualMouse.VirtualMousePos;
    }
    public Vector2 GetMovement() {
      var movement = controls.GamePlay.Movement.ReadValue<Vector2>();
      if (_stats.SnapInput) {
        movement.x = Mathf.Abs(movement.x) < _stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(movement.x);
        movement.y = Mathf.Abs(movement.y) < _stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(movement.y);
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
    }

    private void AttackCanceled(InputAction.CallbackContext context) {
      attacking = false;
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
        controls.GamePlay.Enable();
      }
      else {
        controls.GamePlay.Disable();
      }
    }
    #endregion
    

    private void OnEnable() {
      controls?.Enable();
    }

    private void OnDisable() {
      controls?.Disable();
      InputSystem.onActionChange -= InputActionChangeCallback;
      //UnsubscribeUIClick();
      UnsubscribeBuidClick();
    }
  }
}
