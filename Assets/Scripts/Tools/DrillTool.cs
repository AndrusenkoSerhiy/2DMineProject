using System;
using Items;
using Player;
using Settings;
using UnityEngine;

namespace Tools {
  public class DrillTool : ToolBase {
    private IDamageable currentTarget;
    [SerializeField] private Animator _animator;
    [SerializeField] private CapsuleCollider2D _collider;
    private bool _isActive;
    private Vector3 _defaultRotation;

    private void Awake() {
      _animator.enabled = false;
      _collider.enabled = false;
    }

    private void Start() {
      _defaultRotation = transform.localEulerAngles;
    }

    public override void Activate() {
      base.Activate();
      _animator.enabled = true;
      UserInput.instance.OnAttackPerformed += StartDrilling;
      UserInput.instance.OnAttackCanceled += StopDrilling;
    }

    private void StartDrilling(object sender, EventArgs e) {
      //InvokeRepeating("Attack", 1, 1);
      StartAnimation();
      _isActive = true;
    }

    private void StopDrilling(object sender, EventArgs e) {
      //CancelInvoke("Attack");
      StopAnimation();
      _isActive = false;
      
      transform.localEulerAngles = _defaultRotation;
    }

    private void StartAnimation() {
      _animator.SetBool("isActive", true);
    }

    private void StopAnimation() {
      _animator.SetBool("isActive", false);
    }

    private void Update() {
      if (!_isActive)
        return;
      
      LookAt();
    }

    private void LookAt() {
      Vector3 mousePosition = Camera.main.ScreenToWorldPoint(UserInput.instance.GetMousePosition());
      
      Vector3 direction = GameManager.instance.PlayerController.transform.localScale.x * (mousePosition - transform.position);
      
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