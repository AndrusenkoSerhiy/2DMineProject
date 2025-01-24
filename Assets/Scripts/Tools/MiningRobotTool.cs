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
    private bool isPlayerInside;
    
    private PlayerController playerController;
    private MiningRobotController miningRobotController;

    private void Start() {
      playerController = GameManager.instance.PlayerController;
      miningRobotController = GameManager.instance.MiningRobotController;
    }
    public string InteractionPrompt => isPlayerInside ? $"{interactExitName}" : $"{interactEnterName}";

    public bool Interact(Interactor interactor) {
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
      playerController.EnableController(false);
      playerController.EnableCollider(false);
      playerController.SetLockHighlight(true);
      miningRobotController.EnableController(true);
      miningRobotController.EnableAttackCollider(true);
      
      SetPlayerPosition(playerTransform, Vector3.zero);
      GameManager.instance.CurrPlayerController = miningRobotController;
    }

    private void ExitFromRobot() {
      SetPlayerPosition(null, exitTransforms[0].position);
      miningRobotController.EnableController(false);
      playerController.EnableCollider(true);
      playerController.EnableController(true);
      playerController.SetLockHighlight(false);
      miningRobotController.EnableAttackCollider(true);
      miningRobotController.ClearHighlights();
      GameManager.instance.CurrPlayerController = playerController;
    }

    private void SetPlayerPosition(Transform tr, Vector3 pos) {
      playerController.SetParent(tr);
      playerController.SetPosition(pos);
    }
  }
}