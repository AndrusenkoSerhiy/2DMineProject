using System;
using System.Collections.Generic;
using Animation;
using Interaction;
using Inventory;
using Player;
using SaveSystem;
using Scriptables.Repair;
using UnityEngine;

namespace Tools {
  public class MiningRobotTool : MonoBehaviour, IInteractable, ISaveLoad {
    [SerializeField] private string interactEnterName;
    [SerializeField] private string interactExitName;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private List<Transform> exitTransforms;
    [SerializeField] private string holdInteractText;

    [SerializeField] private RobotObject robotObject;
    [SerializeField] private SpriteRenderer robotImage;
    [SerializeField] private SpriteRenderer brokenRobotImage;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerStats stats;

    private bool isPlayerInside;
    private RobotData robotLoadData;

    private GameManager gameManager;
    private PlayerController playerController;
    private MiningRobotController miningRobotController;
    private PlayerInventory playerInventory;

    private string id;
    private bool broken;

    private int repairValue;
    private bool playerInRobot;

    public static event Action OnPlayerEnteredRobot;
    public static event Action OnPlayerSitOnRobot;
    public static event Action OnPlayerExitFromRobot;

    public string InteractionText => isPlayerInside
      ? $"{interactExitName}"
      : $"{interactEnterName}";

    public bool HasHoldInteraction => !playerInRobot && CanRepair();
    public string HoldInteractionText => holdInteractText;

    private void Awake() {
      SaveLoadSystem.Instance.Register(this);
      id = robotObject.Id;
      Load();
    }

    private void Start() {
      broken = IsBroken();
      CheckRobotRepaired();

      gameManager = GameManager.Instance;
      playerController = gameManager.PlayerController;
      miningRobotController = gameManager.MiningRobotController;
      playerInventory = gameManager.PlayerInventory;
      animator.SetBool("IsBroken", broken);
      stats.OnAddHealth += OnAddHealthHandler;

      UpdateRobotPosition();
      if (robotLoadData is { IsPlayerInside: true }) {
        isPlayerInside = !isPlayerInside;
        SitOnRobot();
        ResetPlayerAnim();
      }
    }

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
      CheckRobotRepaired();
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      if (broken) {
        GameManager.Instance.MessagesManager.ShowSimpleMessage("Robot is broken.");
        return true;
      }

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

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      Repair();
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
      //playerController.ResetHeadPos();
      playerController.Stamina.EnableSprintScript(false);
      miningRobotController.Stamina.EnableSprintScript(true);
      miningRobotController.EnableController(true);
      miningRobotController.SetLockHighlight(false);
      miningRobotController.EnableCollider(true);
      miningRobotController.SetRBType(RigidbodyType2D.Dynamic);

      SetPlayerPosition(playerTransform, Vector3.zero);
      GameManager.Instance.CurrPlayerController = miningRobotController;

      playerController.ResetLocalScale();

      AddRobotInventoryToMainInventory();
      playerInRobot = true;
      OnPlayerSitOnRobot?.Invoke();
    }

    private void ExitFromRobot() {
      SetPlayerPosition(null, exitTransforms[0].position);
      miningRobotController.EnableCollider(false);
      miningRobotController.SetRBType(RigidbodyType2D.Kinematic);
      miningRobotController.EnableController(false);
      playerController.EnableCollider(true);
      playerController.EnableController(true);
      playerController.SetLockHighlight(false);
      miningRobotController.SetLockHighlight(true);
      playerController.Stamina.EnableSprintScript(true);
      miningRobotController.Stamina.EnableSprintScript(false);
      GameManager.Instance.CurrPlayerController = playerController;
      GameManager.Instance.QuickSlotListener.Activate();

      RemoveRobotInventoryFromMainInventory();
      playerInRobot = false;
      OnPlayerExitFromRobot?.Invoke();
    }

    private void SetPlayerPosition(Transform tr, Vector3 pos) {
      playerController.SetParent(tr);
      playerController.SetPosition(pos);
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

      gameManager.SiegeManager.StartSieges();

      if (broken) {
        AnimationEventManager.onRobotRepaired += RobotRepaired;
        animator.SetBool("IsBroken", false);
        animator.SetTrigger("Repair");
      }
      else {
        stats.AddHealth(repairValue);
      }
    }

    private void RobotRepaired() {
      stats.AddHealth(repairValue);
      ShowNormalTexture();
      AnimationEventManager.onRobotRepaired -= RobotRepaired;
    }

    private void CheckRobotRepaired() {
      if (!broken) {
        ShowNormalTexture();
      }
      else {
        ShowBrokenTexture();
      }
    }

    private void ShowNormalTexture() {
      robotImage.enabled = true;
      brokenRobotImage.enabled = false;
    }

    private void ShowBrokenTexture() {
      brokenRobotImage.enabled = true;
      robotImage.enabled = false;
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

    public void Save() {
      var tr = miningRobotController.transform;
      SaveLoadSystem.Instance.gameData.Robots[id] = new RobotData {
        Id = id,
        IsSet = true,
        IsPlayerInside = playerInRobot,
        Position = transform.position,
        Rotation = transform.rotation,
        Scale = transform.localScale,
        PlayerStatsData = new PlayerStatsData {
          Health = stats.Health,
          Stamina = stats.Stamina
        }
      };
    }

    public void Load() {
      if (SaveLoadSystem.Instance.IsNewGame) {
        return;
      }

      if (!SaveLoadSystem.Instance.gameData.Robots.TryGetValue(id, out var data)) {
        return;
      }

      if (!data.IsSet) {
        return;
      }

      robotLoadData = data;
      stats.Init(data.PlayerStatsData);
    }
  }
}