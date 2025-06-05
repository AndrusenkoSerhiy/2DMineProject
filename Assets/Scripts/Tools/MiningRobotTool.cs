using System;
using System.Collections.Generic;
using Actors;
using Analytics;
using Animation;
using Audio;
using Interaction;
using Inventory;
using Menu;
using Player;
using SaveSystem;
using Scriptables;
using Scriptables.Repair;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;
using World;

namespace Tools {
  public class MiningRobotTool : MonoBehaviour, IInteractable, ISaveLoad {
    [SerializeField] private string interactEnterName;
    [SerializeField] private string interactExitName;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 positionForPlayer;
    [SerializeField] private List<Transform> exitTransforms;
    [SerializeField] private string holdInteractText;

    [SerializeField] private RobotObject robotObject;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerStats stats;
    [SerializeField] private PlaceCellRobot placeCellRobot;

    [SerializeField] private InteractionPrompt changeModePrompt;
    [SerializeField] private string actionName;
    [SerializeField] private string attackModeName;
    [SerializeField] private string buildModeName;

    [SerializeField] private InteractionPrompt changeBlockTypePrompt;
    [SerializeField] private string changeBlockActionName;
    [SerializeField] private Coords robotCoordsOutOfBounds;
    [SerializeField] private AudioData brokenInteract;
    [SerializeField] private AudioData robotBreak;
    [SerializeField] private AudioData robotRepair;
    private AudioController audioController;
    
    private bool isAttackMode = true;
    private string buttonName;
    private string changeBlockButtonName;
    private RobotData robotLoadData;
    private GameManager gameManager;
    private PlayerController playerController;
    private MiningRobotController miningRobotController;
    private PlayerInventory playerInventory;

    private string id;

    private bool broken;

    //use after save load and we in the robot
    private bool needActivateItem = true;
    private int repairValue;
    
    [SerializeField] private bool playerInRobot;
    private ChunkController chunkController;

    public static event Action OnPlayerEnteredRobot;
    public static event Action OnPlayerSitOnRobot;
    public static event Action OnPlayerExitFromRobot;

    public string InteractionText => playerInRobot
      ? $"{interactExitName}"
      : $"{interactEnterName}";

    public bool HasHoldInteraction => !playerInRobot && CanRepair();
    public string HoldInteractionText => holdInteractText;

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
      gameManager = GameManager.Instance;
      id = robotObject.Id;
      audioController = GameManager.Instance.AudioController;
    }

    private void Start() {
      if (!GameManager.Instance.InitScriptsOnStart()) {
        return;
      }

      Init();
    }

    private void Init() {
      broken = IsBroken();

      playerController = gameManager.PlayerController;
      miningRobotController = gameManager.MiningRobotController;
      playerInventory = gameManager.PlayerInventory;
      animator.SetBool("IsBroken", broken);
      if (broken) {
        animator.SetTrigger("Die");
      }
      stats.OnAddHealth += OnAddHealthHandler;

      UpdateRobotPosition();
      if (robotLoadData is { IsPlayerInside: true }) {
        SitOnRobot();
        ResetPlayerAnim();
      }

      chunkController = gameManager.ChunkController;
      robotCoordsOutOfBounds = miningRobotController.PlayerCoords.GetCoordsOutOfBounds();
      if (chunkController.ChunkData == null) {
        chunkController.OnCreateChunk += LockOnStart;
      }
      else {
        LockOnStart();
      }

      MenuController.OnExitToMainMenu += ExitToMainMenu;

      if (SaveLoadSystem.Instance.IsNewGame()) {
        gameManager.Locator.SetTarget(transform.position, id);
      }
    }

    private void ExitToMainMenu() {
      MenuController.OnExitToMainMenu -= ExitToMainMenu;
      EnablePhysics(false);
      needActivateItem = false;
    }

    private void LockOnStart() {
      chunkController.OnCreateChunk -= LockOnStart;
      LockCells(true);
    }

    #region Save/Load

    public int Priority => LoadPriority.PLAYER_CONTROLLER;

    public void Save() {
      var tr = miningRobotController.transform;
      SaveLoadSystem.Instance.gameData.Robots[id] = new RobotData {
        Id = id,
        IsSet = true,
        IsPlayerInside = playerInRobot,
        Position = tr.position,
        Rotation = tr.rotation,
        Scale = tr.localScale,
        PlayerStatsData = new PlayerStatsData {
          Health = stats.Health,
          Stamina = stats.Stamina
        }
      };
    }

    public void Load() {
      if (!SaveLoadSystem.Instance.IsNewGame() &&
          SaveLoadSystem.Instance.gameData.Robots.TryGetValue(id, out var data) &&
          data.IsSet) {
        robotLoadData = data;
        stats.Init(data.PlayerStatsData);
      }
      else {
        stats.Init();
      }

      Init();
    }

    public void Clear() {
      SetPlayerInRobot(false);
      robotLoadData = null;
    }

    private void SetPlayerInRobot(bool state) {
      playerInRobot = state;
    }

    #endregion

    private void OnDisable() {
      stats.OnAddHealth -= OnAddHealthHandler;
    }

    private void OnAddHealthHandler(float before, float after) {
      var brokenBefore = broken;
      broken = after <= 0;

      if (brokenBefore == broken) {
        return;
      }

      animator.SetBool("IsBroken", broken);
      if(broken) audioController.PlayAudio(robotBreak, transform.position);
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      if (broken) {
        audioController.PlayAudio(brokenInteract, transform.position);
        GameManager.Instance.MessagesManager.ShowSimpleMessage("Robot is broken. Get repair kit!");
        return true;
      }
      
      if (!playerInRobot) {
        SitOnRobot();
        ResetPlayerAnim();
      }
      else {
        ExitFromRobot();
      }

      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      Repair();
      return true;
    }

    private void ResetPlayerAnim() {
      playerController.ResetAnimatorMovement();
    }

    private void SitOnRobot() {
      gameManager.Locator.RemoveTarget(id);
      LockCells(false);
      ActorRobot.OnRobotBroked += ExitFromRobot;
      OnPlayerEnteredRobot?.Invoke();
      GameManager.Instance.QuickSlotListener.Deactivate("MiningRobot");
      playerController.EnableController(false);
      playerController.EnableCollider(false);
      playerController.SetLockHighlight(true);
      playerController.Stamina.EnableSprintScript(false);
      
      miningRobotController.Stamina.EnableSprintScript(true);
      miningRobotController.EnableController(true);
      miningRobotController.SetLockHighlight(false);
      EnablePhysics(true);

      SetPlayerPosition(playerTransform, positionForPlayer, new Vector3(0, 0, -92));
      GameManager.Instance.CurrPlayerController = miningRobotController;

      playerController.ResetLocalScale();

      AddRobotInventoryToMainInventory();
      SetPlayerInRobot(true);
      OnPlayerSitOnRobot?.Invoke();

      SubscribeToChangeMode();
      buttonName = ButtonPromptSprite.GetSpriteName(gameManager.UserInput.controls.GamePlay.Build);
      changeModePrompt.UpdateSpriteAsset();
      UpdateModePrompt();
      ApplyMode();
    }

    private void SubscribeToChangeMode() {
      gameManager.UserInput.controls.GamePlay.Build.performed += ChangeMode;
    }

    private void UnsubscribeToChangeMode() {
      gameManager.UserInput.controls.GamePlay.Build.performed -= ChangeMode;
    }

    private void SubscribeToChangeBlockType() {
      gameManager.UserInput.controls.GamePlay.BlockChange.performed += ChangeBlockType;
    }

    private void UnsubscribeToChangeBlockType() {
      gameManager.UserInput.controls.GamePlay.BlockChange.performed -= ChangeBlockType;
    }


    private void ChangeMode(InputAction.CallbackContext obj) {
      isAttackMode = !isAttackMode;
      miningRobotController.EnableAttack(isAttackMode);
      miningRobotController.SetMaxTargets(isAttackMode ? 6 : 0);
      if (isAttackMode) placeCellRobot.Deactivate();
      else placeCellRobot.Activate();

      UpdateModePrompt();
    }

    //if we exit from robot and build mode
    private void ApplyMode() {
      miningRobotController.EnableAttack(isAttackMode);
      miningRobotController.SetMaxTargets(isAttackMode ? 6 : 0);
      if (isAttackMode) placeCellRobot.Deactivate();
      else placeCellRobot.Activate();

      UpdateModePrompt();
    }

    private void ChangeBlockType(InputAction.CallbackContext obj) {
      placeCellRobot.UpdateBlockType();
    }

    private void UpdateModePrompt() {
      var nextMode = isAttackMode ? buildModeName : attackModeName;
      changeModePrompt.ShowPrompt(true, ButtonPromptSprite.GetFullPrompt(actionName + " " + nextMode, buttonName));

      //show hide prompt for change block type
      changeBlockButtonName = ButtonPromptSprite.GetSpriteName(gameManager.UserInput.controls.GamePlay.BlockChange);
      changeBlockTypePrompt.ShowPrompt(!isAttackMode,
        ButtonPromptSprite.GetFullPrompt(changeBlockActionName, changeBlockButtonName));
      if (isAttackMode) UnsubscribeToChangeBlockType();
      else SubscribeToChangeBlockType();
    }

    private void ExitFromRobot() {
      gameManager.Locator.SetTarget(transform.position, id);
      ActorRobot.OnRobotBroked -= ExitFromRobot;
      SetPlayerPosition(null, exitTransforms[0].position, Vector3.zero);

      placeCellRobot.Deactivate();
      playerController.EnableCollider(true);
      playerController.EnableController(true);
      playerController.SetLockHighlight(false);
      miningRobotController.ClearLockList();
      miningRobotController.SetLockHighlight(true);
      playerController.Stamina.EnableSprintScript(true);
      miningRobotController.Stamina.EnableSprintScript(false);
      miningRobotController.ResetAnimatorMovement();
      miningRobotController.ResetAttackParam();
      GameManager.Instance.CurrPlayerController = playerController;
      GameManager.Instance.QuickSlotListener.Activate("MiningRobot", needActivateItem);
      needActivateItem = true;
      UnsubscribeToChangeMode();
      changeModePrompt.ShowPrompt(false);

      //hide prompt for change block type
      changeBlockTypePrompt.ShowPrompt(false);
      UnsubscribeToChangeBlockType();

      RemoveRobotInventoryFromMainInventory();
      SetPlayerInRobot(false);

      if (miningRobotController.Grounded) {
        EnablePhysics(false);
        LockCells(true);
      }
      else {
        miningRobotController.GroundedChanged += GroundChanged;
      }

      OnPlayerExitFromRobot?.Invoke();
    }

    private void GroundChanged(bool state, float velocity) {
      EnablePhysics(!state);
      robotCoordsOutOfBounds = miningRobotController.PlayerCoords.GetCoordsOutOfBounds();
      miningRobotController.GroundedChanged -= GroundChanged;
      LockCells(true);
    }

    [SerializeField] private List<int> lockedCells = new() { 0 };

    private void LockCells(bool state) {
      if (playerInRobot) {
        return;
      }

      robotCoordsOutOfBounds = miningRobotController.PlayerCoords.GetCoordsOutOfBounds();
      var firstX = robotCoordsOutOfBounds.X;
      var firstY = robotCoordsOutOfBounds.Y + 1;

      if (chunkController.ChunkData.GetCellFill(firstX, firstY).Equals(1)) {
        if (state && chunkController.ChunkData.GetCellData(firstX, firstY).canTakeDamage) {
          chunkController.ChunkData.GetCellData(firstX, firstY).canTakeDamage = false;
          lockedCells[0] = 1;
        }
        else if (!state && lockedCells[0] == 1) {
          lockedCells[0] = 0;
          chunkController.ChunkData.GetCellData(firstX, firstY).canTakeDamage = true;
        }
      }
    }

    private void EnablePhysics(bool state) {
      //Debug.LogError($"EnablePhysics {state}");
      miningRobotController.EnableCollider(state);
      miningRobotController.SetRBType(state ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic);
      miningRobotController.EnableController(state);
    }

    private void SetPlayerPosition(Transform tr, Vector3 pos, Vector3 rot) {
      playerController.SetParent(tr);
      playerController.SetPosition(pos);
      playerController.SetRotation(rot);
      playerController.SetOrderInLayer(tr != null ? 0 : 2);
    }

    private void AddRobotInventoryToMainInventory() {
      var robotInventory =
        GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(InventoryType.RobotInventory, id);
      GameManager.Instance.PlayerInventory.GetInventory().AddInventoryObject(robotInventory.MainInventoryObject);
    }

    private void RemoveRobotInventoryFromMainInventory() {
      var robotInventory =
        GameManager.Instance.PlayerInventory.GetInventoryByTypeAndId(InventoryType.RobotInventory, id);
      GameManager.Instance.PlayerInventory.GetInventory().RemoveInventoryObject(robotInventory.MainInventoryObject);
    }

    private void Repair() {
      if (!CanRepair()) {
        return;
      }

      repairValue = playerInventory.Repair(stats.MaxHealth, stats.Health, robotObject.RepairCost);

      if (repairValue == 0) {
        return;
      }

      gameManager.QuestManager.StartQuest(2);
      gameManager.SiegeManager.StartSieges();

      if (broken) {
        AnimationEventManager.onRobotRepaired += RobotRepaired;
        animator.SetBool("IsBroken", false);
        animator.SetTrigger("Repair");
        audioController.PlayAudio(robotRepair, transform.position);
      }
      else {
        stats.AddHealth(repairValue);
      }
    }

    private void RobotRepaired() {
      stats.AddHealth(repairValue);
      //ShowNormalTexture();
      AnimationEventManager.onRobotRepaired -= RobotRepaired;
      miningRobotController.Actor.Respawn();
      AnalyticsManager.Instance.LogRobotRepaired(robotObject.name, repairValue);
    }

    private bool CanRepair() {
      if (stats == null) {
        return false;
      }

      return stats.Health < stats.MaxHealth;
    }

    private bool IsBroken() {
      if (stats == null) {
        return false;
      }

      return stats.Health == 0;
    }

    private void UpdateRobotPosition() {
      if (robotLoadData == null) {
        return;
      }

      var tr = miningRobotController.transform;
      tr.position = robotLoadData.Position;
      tr.rotation = robotLoadData.Rotation;
      tr.localScale = robotLoadData.Scale;
    }
  }
}