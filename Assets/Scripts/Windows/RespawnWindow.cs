using Actors;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

namespace Windows {
  public class RespawnWindow : WindowBase {
    [SerializeField] private string labelText;
    public override void Init() {
      base.Init();
      SubscribePlayerDeath();
    }

    public override void Show() {
      DOVirtual.DelayedCall(2.0f, () => {
        base.Show();
        GameManager.Instance.UserInput.controls.UI.Respawn.performed += Respawn;
        GetInteractionText();
        SetInteractionText();
        GameManager.Instance.AudioController.PlayPlayerDeath();
      });
    }
    
    private void GetInteractionText() {
      interactionPromtUI.UpdateSpriteAsset();
      buttonName = ButtonPromptSprite.GetSpriteName(GameManager.Instance.UserInput.controls.UI.Respawn);
    }

    protected override void OnChangeDevice() {
      GetInteractionText();
      SetInteractionText();
    }

    protected override void SetInteractionText() {
      var test = ButtonPromptSprite.GetSpriteTag(buttonName);
      var str = string.Format(labelText, test);
      interactionPromtUI.ShowPrompt(true, str);
    }

    private void Respawn(InputAction.CallbackContext obj) {
      // Debug.LogError("respawn");
      GameManager.Instance.PlayerController.transform.position = GameManager.Instance.RespawnManager.GetRespawnPoint();
      GameManager.Instance.PlayerController.SetAnimatorRespawn();
      GameManager.Instance.PlayerController.RestoreHealth();
      Hide();
    }

    public override void Hide() {
      base.Hide();
      GameManager.Instance.AudioController.StopPlayerDeath();
      GameManager.Instance.UserInput.controls.UI.Respawn.performed -= Respawn;
    }

    private void SubscribePlayerDeath() {
      ActorPlayer.OnPlayerDeath += Show;
    }
    
    private void UnsubscribePlayerDeath() {
      ActorPlayer.OnPlayerDeath -= Show;
    }

    private void OnDestroy() {
      UnsubscribePlayerDeath();
    }
  }
}