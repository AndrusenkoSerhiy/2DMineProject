using Animation;
using SaveSystem;
using Spine.Unity;
using UnityEngine;

namespace Player {
  [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
  public class PlayerController : PlayerControllerBase, IPlayerController, ISaveLoad {
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private SkeletonMecanim skeletonMecanim;
    protected override void Awake() {
      SaveLoadSystem.Instance.Register(this);
      base.Awake();
      GameManager.Instance.PlayerController = this;
      GameManager.Instance.CurrPlayerController = this;
      
      AnimationEventManager.onLeftStep += GameManager.Instance.AudioController.PlayPlayerLeftStep;
      AnimationEventManager.onRightStep += GameManager.Instance.AudioController.PlayPlayerRightStep;
    }

    protected override void Start() {
      SetEmptyHand();
    }
    private void SetEmptyHand() {
      if (skeletonMecanim == null)
        return;

      skeletonMecanim.Skeleton.SetAttachment("Weapon", null);
    }

    #region save/load

    public int Priority => LoadPriority.PLAYER_CONTROLLER;

    public void Save() {
      var data = SaveLoadSystem.Instance.gameData.PlayerData;

      data.Position = gameObject.transform.position;
      data.Rotation = gameObject.transform.rotation;
      data.Scale = gameObject.transform.localScale;
      data.IsSet = true;

      data.PlayerStatsData = PlayerStats.PrepareSaveData();
    }

    public void Load() {
      var data = SaveLoadSystem.Instance.gameData.PlayerData;
      if (SaveLoadSystem.Instance.IsNewGame() || !data.IsSet) {
        PlayerStats.Init();
        return;
      }

      gameObject.transform.position = data.Position;
      gameObject.transform.rotation = data.Rotation;
      gameObject.transform.localScale = data.Scale;

      PlayerStats.Init(data.PlayerStatsData);
    }

    public void Clear() {
    }

    #endregion

    public override void SetLockHighlight(bool state, string reason = "") {
      playerAttack.LockHighlight(state, reason);
    }

    protected override void FlipX() {
      if (GameManager.Instance.WindowsController.IsAnyWindowOpen)
        return;

      Vector2 mousePosition = _camera.ScreenToWorldPoint(GameManager.Instance.UserInput.GetMousePosition());
      var direction = (mousePosition - (Vector2)transform.position).normalized;

      if (Mathf.Abs(mousePosition.x - transform.position.x) > _flipDeadZone) {
        // Flip player
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Sign(direction.x);
        transform.localScale = localScale;

        rotationCoef = isFlipped ? -1f : 1f;
        direction.x *= rotationCoef;
      }
    }
  }
}