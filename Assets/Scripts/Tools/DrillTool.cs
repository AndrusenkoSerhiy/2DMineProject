using System;
using Audio;
using Scriptables;
using UnityEngine;

namespace Tools {
  public class DrillTool : HandItem {
    private static readonly int IsActive = Animator.StringToHash("isActive");

    [SerializeField] private Animator animator;
    [SerializeField] private AudioData drillSound;
    
    private Vector3 defaultRotation;
    private AudioController audioController;
    
    private void Awake() {
      animator.enabled = false;
      audioController = GameManager.Instance.AudioController;
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
      audioController.PlayAudio(drillSound, transform.position);
      StartAnimation();
    }

    private void StopDrilling(object sender, EventArgs e) {
      audioController.StopAudio(drillSound);
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