using UnityEngine;

namespace Scriptables.CameraController{
  [CreateAssetMenu(fileName = "CameraConfigData", menuName = "Camera/CameraConfigData")]
  public class CameraConfigData : ScriptableObject {
    public float ZoomFactor;
  }
}
