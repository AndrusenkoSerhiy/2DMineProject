using System;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;

public class VirtualMouseUI : MonoBehaviour {
  [SerializeField] private VirtualMouseInput virtualMouseInput;

  public Vector3 VirtualMousePos;

  private void Start() {
    Hide();
    UserInput.instance.OnGameDeviceChanged += OnGameDeviceChanged;
  }

  private void LateUpdate() {
    // if(UserInput.instance.GetActiveGameDevice() != UserInput.GameDevice.Gamepad)
    //     return;
    if (Input.GetKeyDown(KeyCode.U)) {
      SetPosition();
    }
    ClampVirtualMouse();
  }

  private void ClampVirtualMouse() {
    Vector2 virtualMousePosition = virtualMouseInput.virtualMouse.position.value;
    var minX = 30f;
    var maxX = Screen.width - 30f;
    var minY = 30f;
    var maxY = Screen.height - 30f;
    //Debug.LogError($"width {Screen.width} | height {Screen.height}");
    virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, minX, maxX);
    virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, minY, maxY);
    InputState.Change(virtualMouseInput.virtualMouse.position, virtualMousePosition);
    VirtualMousePos = new Vector3(virtualMousePosition.x, virtualMousePosition.y, 0);
  }

  private void SetPosition() {
    Debug.LogError("set position");
    var virtualMousePosition = new Vector2(Screen.width/2, Screen.height/2);
    InputState.Change(virtualMouseInput.virtualMouse.position, virtualMousePosition);
  }

  private void OnGameDeviceChanged(object sender, EventArgs e) {
    UpdateVisibility();
  }

  private void UpdateVisibility() {
    if (UserInput.instance.GetActiveGameDevice() == UserInput.GameDevice.Gamepad) {
      Show();
    }
    else Hide();
  }

  private void Show() {
    Debug.LogError("show");
    gameObject.SetActive(true);
  }

  private void Hide() {
    Debug.LogError("hide");
    gameObject.SetActive(false);
  }
}