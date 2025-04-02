using System;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class VirtualMouseUI : MonoBehaviour {
  [SerializeField] private RectTransform canvasRectTransform;
  private VirtualMouseInput _virtualMouseInput;

  private Vector3 virtualMousePos;
  [SerializeField] private Image image;
  private void Awake() {
    _virtualMouseInput = GetComponent<VirtualMouseInput>();
    Hide();
  }

  private void Start() {
    GameManager.Instance.UserInput.OnGameDeviceChanged += OnGameDeviceChanged;
  }

  private void Update() {
    transform.localScale = Vector3.one * (1f / canvasRectTransform.localScale.x);
  }

  private void LateUpdate() {
    /*if(/*GameManager.Instance.UserInput.GetActiveGameDevice() != UserInput.GameDevice.Gamepad &&#1#
       !image.enabled)
         return;*/
    
    var virtualMousePosition = _virtualMouseInput.virtualMouse.position.value;
    virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 30f, Screen.width - 30);
    virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 30f, Screen.height - 30);
    InputState.Change(_virtualMouseInput.virtualMouse.position, virtualMousePosition);
    virtualMousePos = new Vector3(virtualMousePosition.x, virtualMousePosition.y, 0);
  }

  public Vector3 GetVirtualMousePosition() {
    //Debug.LogError($"virtualMousePos {virtualMousePos}");
    return virtualMousePos;
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
    image.enabled = true;
  }

  private void Hide() {
    image.enabled = false;
  }
}