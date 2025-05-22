using System;
using UnityEngine;

namespace Tools {
  public class DrillTool : HandItem {
    private static readonly int IsActive = Animator.StringToHash("isActive");

    [SerializeField] private Animator animator;
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
    }

    private void StopDrilling(object sender, EventArgs e) {
      StopAnimation();

      transform.localEulerAngles = defaultRotation;
    }

    private void StartAnimation() {
      animator.SetBool(IsActive, true);
    }

    private void StopAnimation() {
      animator.SetBool(IsActive, false);
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