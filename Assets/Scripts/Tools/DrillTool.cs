using System;
using Settings;
using UnityEngine;

namespace Tools {
  public class DrillTool : ToolBase {
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
      UserInput.instance.OnAttackPerformed += StartDrilling;
      UserInput.instance.OnAttackCanceled += StopDrilling;
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
      var mousePosition = GameManager.instance.MainCamera.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
      
      var direction = GameManager.instance.PlayerController.transform.localScale.x * (mousePosition - transform.position);
      
      float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

      // Apply the rotation
      transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
    }
    
    private void OnDestroy() {
      UserInput.instance.OnAttackPerformed -= StartDrilling;
      UserInput.instance.OnAttackCanceled -= StopDrilling;
    }
  }
}