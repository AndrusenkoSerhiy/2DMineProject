using System;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class VirtualMouseUI : MonoBehaviour {
  [SerializeField] CanvasScaler canvasScaler;
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

  private void LateUpdate() {
    if(/*GameManager.Instance.UserInput.GetActiveGameDevice() != UserInput.GameDevice.Gamepad &&*/
       !image.enabled)
         return;
    
    var virtualMousePosition = _virtualMouseInput.virtualMouse.position.value;
    virtualMousePosition.x = Mathf.Clamp(virtualMousePosition.x, 30f, canvasScaler.referenceResolution.x - 30);
    virtualMousePosition.y = Mathf.Clamp(virtualMousePosition.y, 30f, canvasScaler.referenceResolution.y - 30);
    InputState.Change(_virtualMouseInput.virtualMouse.position, virtualMousePosition);
    virtualMousePos = new Vector3(virtualMousePosition.x, virtualMousePosition.y, 0);
  }

  //TODO
  //test variant virtual mouse work corect with diffrent resolution
  //but now you cant click on start game when resolution not fullHD
  private float GetPositionByResolution() {
    Vector2 referenceResolution = canvasScaler.referenceResolution;
    float scaleFactor = Screen.width / referenceResolution.x;
    //Debug.LogError($"referenceResolution {referenceResolution} | scaleFactor {scaleFactor}");
    return scaleFactor;
  }

  public Vector3 GetVirtualMousePosition() {
    //Debug.LogError($"virtualMousePos {virtualMousePos}");
    return virtualMousePos*GetPositionByResolution();
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
    InputState.Change(_virtualMouseInput.virtualMouse.position, Input.mousePosition);
    virtualMousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
    image.enabled = true;
  }

  private void Hide() {
    image.enabled = false;
  }
}