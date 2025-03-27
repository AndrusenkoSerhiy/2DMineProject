using System;
using UnityEngine;

namespace Tools {
  public class DrillTool : HandItem {
    private static readonly int IsActive = Animator.StringToHash("isActive");

    [SerializeField] private Animator animator;
    private bool isActive;
    private Vector3 defaultRotation;

    private void Awake() {
      animator.enabled = false;
    }

    private void Start() {
      defaultRotation = transform.localEulerAngles;
    }

    public override void Activate() {
      base.Activate();
      animator.enabled = true;
      GameManager.Instance.UserInput.OnAttackPerformed += StartDrilling;
      GameManager.Instance.UserInput.OnAttackCanceled += StopDrilling;
    }

    private void StartDrilling(object sender, EventArgs e) {
      StartAnimation();
      isActive = true;
    }

    private void StopDrilling(object sender, EventArgs e) {
      StopAnimation();
      isActive = false;

      transform.localEulerAngles = defaultRotation;
    }

    private void StartAnimation() {
      animator.SetBool(IsActive, true);
    }

    private void StopAnimation() {
      animator.SetBool(IsActive, false);
    }

    private void Update() {
      if (!isActive)
        return;

      LookAt();
    }

    private void LookAt() {
      return;
      var mousePosition =
        GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.UserInput.GetMousePosition());

      var direction = GameManager.Instance.PlayerController.transform.localScale.x *
                      (mousePosition - transform.position);

      float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

      // Apply the rotation
      transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }

    private void OnDestroy() {
      if (!GameManager.HasInstance) {
        return;
      }

      GameManager.Instance.UserInput.OnAttackPerformed -= StartDrilling;
      GameManager.Instance.UserInput.OnAttackCanceled -= StopDrilling;
    }
  }
}