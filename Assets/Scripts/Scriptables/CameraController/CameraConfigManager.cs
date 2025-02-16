using System;
using System.Collections.Generic;
using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;

namespace Scriptables.CameraController {
  public class CameraConfigManager : MonoBehaviour {
    [SerializeField] public CinemachineCamera CameraRef;
    private CameraConfigData _currentConfig;
    [SerializeField] private CameraConfigData CameraConfigDefault;
    [SerializeField] private CameraConfigData CameraConfigLow;
    [SerializeField] private CameraConfigData CameraConfigMedium;
    [SerializeField] private CameraConfigData CameraConfigHigh;
    private Tween zoomTween;

    private void Update() {
      if (Input.GetKeyDown(KeyCode.F5)) {
        SetCameraDefault();
        return;
      }
      if (Input.GetKeyDown(KeyCode.F6)) {
        SetCameraLow();
        return;
      }
      if (Input.GetKeyDown(KeyCode.F7)) {
        SetCameraMedium();
        return;
      }
      if (Input.GetKeyDown(KeyCode.F8)) {
        SetCameraHigh();
        return;
      }
    }

    private void SetConfig(CameraConfigData newConfig) {
      if (_currentConfig == newConfig) return;
      _currentConfig = newConfig;
      //apply zoom
      if (CameraRef != null) {
        zoomTween?.Kill(); // Kill any previous tween to prevent conflicts
        zoomTween = DOTween.To(
          () => CameraRef.Lens.OrthographicSize,
          x => CameraRef.Lens.OrthographicSize = x,
          newConfig.ZoomFactor,
          0.5f // Duration of tween (adjust as needed)
        ).SetEase(Ease.OutQuad);
      }
    }

    public void SetCameraDefault() {
      SetConfig(CameraConfigDefault);
    }

    public void SetCameraLow() {
      SetConfig(CameraConfigLow);
    }

    public void SetCameraMedium() {
      SetConfig(CameraConfigMedium);
    }

    public void SetCameraHigh() {
      SetConfig(CameraConfigHigh);
    }
  }
}