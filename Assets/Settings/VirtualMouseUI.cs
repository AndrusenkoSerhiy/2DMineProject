using System;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class VirtualMouseUI : MonoBehaviour {
  private VirtualMouseInput _virtualMouseInput;

  public Vector3 VirtualMousePos;

  private void Awake() {
    _virtualMouseInput = GetComponent<VirtualMouseInput>();
  }

  private void Start() {
    GameManager.Instance.UserInput.OnGameDeviceChanged += OnGameDeviceChanged;
  }

  private void LateUpdate() {
    // if(UserInput.instance.GetActiveGameDevice() != UserInput.GameDevice.Gamepad)
    //     return;

    Vector2 virtualMousePosition = _virtualMouseInput.virtualMouse.position.value;
    virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 30f, Screen.width - 30);
    virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 30f, Screen.height - 30);
    InputState.Change(_virtualMouseInput.virtualMouse.position, virtualMousePosition);
    VirtualMousePos = new Vector3(virtualMousePosition.x, virtualMousePosition.y, 0);
  }

  private void OnGameDeviceChanged(object sender, EventArgs e) {
    UpdateVisibility();
  }

  private void UpdateVisibility() {
    if (GameManager.Instance.UserInput.GetActiveGameDevice() == UserInput.GameDevice.Gamepad) {
      Show();
    }
    else Hide();
  }

  private void Show() {
    gameObject.SetActive(true);
  }

  private void Hide() {
    gameObject.SetActive(false);
  }
}