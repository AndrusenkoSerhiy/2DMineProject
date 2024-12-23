using System;
using Settings;
using UnityEngine;

namespace Tools {
  public class DrillTool : ToolBase {
    
    private IDamageable currentTarget;
    public override void Activate() {
      base.Activate();
      
      UserInput.instance.OnAttackPerformed += StartDrilling;
      UserInput.instance.OnAttackCanceled += StopDrilling;
    }

    private void StartDrilling(object sender, EventArgs e) {
      InvokeRepeating("Attack", 1, 1);
    }

    private void StopDrilling(object sender, EventArgs e) {
      CancelInvoke("Attack");
    }

    private void Attack() {
      Debug.LogError("Attack");
    }
    
    private void OnDestroy() {
      UserInput.instance.OnAttackPerformed -= StartDrilling;
      UserInput.instance.OnAttackCanceled -= StopDrilling;
    }
  }
}