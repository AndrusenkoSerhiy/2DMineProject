using System.Collections.Generic;
using Interaction;
using Player;
using UnityEngine;

namespace Tools {
  public class MiningRobotTool : MonoBehaviour, IInteractable {
    [SerializeField] private string _interactEnterName;
    [SerializeField] private string _interactExitName;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private List<Transform> _exitTransforms;
    private bool _isPlayerInside;
    
    private PlayerController _playerController;
    private PlayerAttack _playerAttack;
    private MiningRobotController _miningRobotController;

    private void Start() {
      _playerController = GameManager.instance.PlayerController;
      _miningRobotController = GameManager.instance.MiningRobotController;
      _playerAttack = GameManager.instance.PlayerAttack;
    }
    public string InteractionPrompt => _isPlayerInside ? $"{_interactExitName}" : $"{_interactEnterName}";

    public bool Interact(Interactor interactor) {
      _isPlayerInside = !_isPlayerInside;
      if (_isPlayerInside) {
        SitOnRobot();
      }
      else {
        ExitFromRobot();        
      }
      return true;
    }

    private void SitOnRobot() {
      Debug.LogError("SitOnRobot");
      _playerController.EnableController(false);
      _playerController.EnableCollider(false);
      _playerAttack.enabled = false;
      _miningRobotController.EnableController(true);
      
      SetPlayerPosition(_playerTransform, Vector3.zero);
    }

    private void ExitFromRobot() {
      Debug.LogError("ExitFromRobot");
      SetPlayerPosition(null, _exitTransforms[0].position);
      _miningRobotController.EnableController(false);
      _playerController.EnableCollider(true);
      _playerController.EnableController(true);
      _playerAttack.enabled = true;
    }

    private void SetPlayerPosition(Transform tr, Vector3 pos) {
      _playerController.SetParent(tr);
      _playerController.SetPosition(pos);
    }
  }
}