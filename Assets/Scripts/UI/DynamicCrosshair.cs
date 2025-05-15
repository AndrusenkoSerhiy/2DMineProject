using Player;
using UnityEngine;

namespace UI {
  public class DynamicCrosshair : MonoBehaviour {
    [SerializeField] private Transform center;
    [SerializeField] private Transform left;
    [SerializeField] private Transform right;
    [SerializeField] private Transform top;
    [SerializeField] private Transform bottom;
    [SerializeField] private float maxDynamicRadius = 0.5f;

    private PlayerController playerController;
    private PlayerStats playerStats;

    private Vector3 baseLeftPos;
    private Vector3 baseRightPos;
    private Vector3 baseTopPos;
    private Vector3 baseBottomPos;

    private void Start() {
      playerController = GameManager.Instance.PlayerController;
      playerStats = playerController.PlayerStats;

      baseLeftPos = left.localPosition;
      baseRightPos = right.localPosition;
      baseTopPos = top.localPosition;
      baseBottomPos = bottom.localPosition;
    }

    private void Update() {
      var speed = Mathf.Min(playerController.GetVelocity().magnitude, playerStats.MaxSpeed);
      var t = Mathf.Clamp01(speed / playerStats.MaxSpeed);
      var dynamicRadius = t * maxDynamicRadius;

      left.localPosition = baseLeftPos + Vector3.left * dynamicRadius;
      right.localPosition = baseRightPos + Vector3.right * dynamicRadius;
      top.localPosition = baseTopPos + Vector3.up * dynamicRadius;
      bottom.localPosition = baseBottomPos + Vector3.down * dynamicRadius;
    }

    public Vector3 GetCenter() => center.position;
  }
}