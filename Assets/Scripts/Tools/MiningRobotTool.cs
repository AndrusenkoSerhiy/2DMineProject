using System;
using System.Collections.Generic;
using Interaction;
using Player;
using UnityEngine;

namespace Tools {
  public class MiningRobotTool : MonoBehaviour, IInteractable {
    [SerializeField] private string interactEnterName;
    [SerializeField] private string interactExitName;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private List<Transform> exitTransforms;
    [SerializeField] private string interactHeader;
    private bool isPlayerInside;
    
    private PlayerController playerController;
    private MiningRobotController miningRobotController;
    
    public static event Action OnPlayerEnteredRobot;

    private void Start() {
      playerController = GameManager.Instance.PlayerController;
      miningRobotController = GameManager.Instance.MiningRobotController;
    }

    public string InteractionText => isPlayerInside ? $"{interactExitName}" : $"{interactEnterName}";
    public string InteractionHeader => interactHeader;

    public bool Interact(PlayerInteractor playerInteractor) {
      isPlayerInside = !isPlayerInside;
      if (isPlayerInside) {
        SitOnRobot();
        ResetPlayerAnim();
      }
      else {
        ExitFromRobot();        
      }
      return true;
    }

    private void ResetPlayerAnim() {
      playerController.ResetAnimatorMovement();
    }

    private void SitOnRobot() {
      OnPlayerEnteredRobot?.Invoke();
      GameManager.Instance.QuickSlotListener.Deactivate();
      
      playerController.EnableController(false);
      playerController.EnableCollider(false);
      playerController.SetLockHighlight(true);
      playerController.ResetHeadPos();
      miningRobotController.EnableController(true);
      miningRobotController.SetLockHighlight(false);
      
      SetPlayerPosition(playerTransform, Vector3.zero);
      GameManager.Instance.CurrPlayerController = miningRobotController;
      
      playerController.ResetLocalScale();
    }

    private void ExitFromRobot() {
      SetPlayerPosition(null, exitTransforms[0].position);
      miningRobotController.EnableController(false);
      playerController.EnableCollider(true);
      playerController.EnableController(true);
      playerController.SetLockHighlight(false);
      miningRobotController.SetLockHighlight(true);
      GameManager.Instance.CurrPlayerController = playerController;
      GameManager.Instance.QuickSlotListener.Activate();
    }

    private void SetPlayerPosition(Transform tr, Vector3 pos) {
      playerController.SetParent(tr);
      playerController.SetPosition(pos);
    }
  }
}