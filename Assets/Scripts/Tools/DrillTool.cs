using System;
using Player;
using Settings;
using UnityEngine;

namespace Tools {
  public class DrillTool : ToolBase {
    private IDamageable currentTarget;
    [SerializeField] private Animator _animator;
    public override void Activate() {
      base.Activate();
      
      UserInput.instance.OnAttackPerformed += StartDrilling;
      UserInput.instance.OnAttackCanceled += StopDrilling;
    }

    private void StartDrilling(object sender, EventArgs e) {
      //InvokeRepeating("Attack", 1, 1);
      StartAnimation();
    }

    private void StopDrilling(object sender, EventArgs e) {
      //CancelInvoke("Attack");
      StopAnimation();
    }

    private void StartAnimation() {
      _animator.SetBool("isActive", true);
    }

    private void StopAnimation() {
      _animator.SetBool("isActive", false);
    }
    
    private void OnDestroy() {
      UserInput.instance.OnAttackPerformed -= StartDrilling;
      UserInput.instance.OnAttackCanceled -= StopDrilling;
    }
  }
}