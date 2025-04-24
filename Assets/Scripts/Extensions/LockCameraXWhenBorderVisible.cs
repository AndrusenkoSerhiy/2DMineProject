using Unity.Cinemachine;
using UnityEngine;

[ExecuteAlways]
[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Custom/LockCameraXWhenBorderVisible")]
public class LockCameraXWhenBorderVisible : CinemachineExtension {
  [SerializeField] private bool lockX = false;
  [SerializeField] private float lockedXPosition;

  private Vector3? originalTrackedOffset;
  private Vector3? originalDamping;
  private CinemachinePositionComposer cinemachinePositionComposer;

  protected override void PostPipelineStageCallback(
    CinemachineVirtualCameraBase vcam,
    CinemachineCore.Stage stage,
    ref CameraState state,
    float deltaTime
  ) {
    if (stage != CinemachineCore.Stage.Body || !lockX) {
      return;
    }

    var pos = state.RawPosition;
    pos.x = lockedXPosition;
    state.RawPosition = pos;
  }

  public void LockCameraX(float positionX) {
    lockX = true;
    lockedXPosition = positionX;

    var composer = GetPositionComposer();
    if (!composer) {
      return;
    }

    originalTrackedOffset ??= composer.TargetOffset;

    composer.TargetOffset = new Vector3(0, composer.TargetOffset.y, composer.TargetOffset.z);

    originalDamping ??= composer.Damping;

    composer.Damping = new Vector3(0, composer.Damping.y, composer.Damping.z);
  }

  public void UnlockCameraX() {
    lockX = false;

    var composer = GetPositionComposer();
    if (!composer) {
      return;
    }

    if (originalTrackedOffset != null) {
      composer.TargetOffset = originalTrackedOffset.Value;
    }

    originalTrackedOffset = null;

    if (originalDamping != null) {
      composer.Damping = originalDamping.Value;
    }

    originalDamping = null;
  }

  private CinemachinePositionComposer GetPositionComposer() {
    if (!cinemachinePositionComposer) {
      cinemachinePositionComposer = GetComponent<CinemachinePositionComposer>();
    }

    return cinemachinePositionComposer;
  }
}