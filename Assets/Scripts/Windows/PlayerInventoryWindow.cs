using System;
using Player;
using Settings;
using UnityEngine;

namespace Windows {
  public class PlayerInventoryWindow : WindowBase {
    private PlayerController _playerController;

    private void Start() {
      _playerController = GameManager.instance.PlayerController;
    }

    public override void Show() {
      base.Show();
      LockPlayer(true);
    }

    public override void Hide() {
      base.Hide();
      LockPlayer(false);
    }
    
    private void LockPlayer(bool state) {
      _playerController.SetLockPlayer(state);
      UserInput.instance.EnableGamePlayControls(!state);
    }
  }
}