using System;
using System.Collections.Generic;
using Windows;
using Animation;
using Interaction;
using Inventory;
using Player;
using Repair;
using SaveSystem;
using Scriptables.Repair;
using UnityEngine;

namespace Tools {
  public class MiningRobotTool : MonoBehaviour, IInteractable, ISaveLoad {
    [SerializeField] private string interactEnterName;
    [SerializeField] private string interactExitName;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private List<Transform> exitTransforms;
    [SerializeField] private string interactHeader;

    [SerializeField] private RobotObject robotObject;
    [SerializeField] private string repairText;
    [SerializeField] private SpriteRenderer robotImage;
    [SerializeField] private SpriteRenderer brokenRobotImage;
    [SerializeField] private Animator animator;

    private bool isPlayerInside;

    private PlayerController playerController;
    private MiningRobotController miningRobotController;

    private string id;
    private bool broken = true;
    private WindowBase window;
    private RepairWindow repairWindow;

    public static event Action OnPlayerEnteredRobot;

    private void Start() {
      id = robotObject.Id;

      Load();
      CheckRobotRepaired();

      playerController = GameManager.Instance.PlayerController;
      miningRobotController = GameManager.Instance.MiningRobotController;
      animator.SetBool("IsBroken", broken);
    }

    public string InteractionText => broken
      ? $"{repairText}"
      : isPlayerInside
        ? $"{interactExitName}"
        : $"{interactEnterName}";

    public string InteractionHeader => interactHeader;

    public bool Interact(PlayerInteractor playerInteractor) {
      if (broken) {
        InitRepairWindow();
        window.Show();
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
      playerController.Stamina.EnableSprintScript(false);
      miningRobotController.Stamina.EnableSprintScript(true);
      miningRobotController.EnableController(true);
      miningRobotController.SetLockHighlight(false);

      SetPlayerPosition(playerTransform, Vector3.zero);
      GameManager.Instance.CurrPlayerController = miningRobotController;

      playerController.ResetLocalScale();

      AddRobotInventoryToMainInventory();
    }

    private void ExitFromRobot() {
      SetPlayerPosition(null, exitTransforms[0].position);
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

    private void InitRepairWindow() {
      if (window != null) {
        return;
      }

      var windowObj = Instantiate(robotObject.InterfacePrefab, GameManager.Instance.Canvas.transform);
      windowObj.transform.SetSiblingIndex(0);

      repairWindow = windowObj.GetComponent<RepairWindow>();
      repairWindow.Setup(robotObject);

      repairWindow.OnRepaired += OnRobotRepairedHandler;

      window = windowObj.GetComponent<WindowBase>();
      GameManager.Instance.WindowsController.AddWindow(window);
    }

    private void OnRobotRepairedHandler() {
      broken = false;
      AnimationEventManager.onRobotRepaired += RobotRepaired;
      animator.SetBool("IsBroken", broken);
      animator.SetTrigger("Repair");
    }

    private void RobotRepaired() {
      Repair();
      AnimationEventManager.onRobotRepaired -= RobotRepaired;
    }

    private void CheckRobotRepaired() {
      if (!broken) {
        Repair();
      }
    }

    private void Repair() {
      robotImage.enabled = true;
      brokenRobotImage.enabled = false;
    }

    public void Save() {
      SaveLoadSystem.Instance.gameData.Robots[id] = new RobotData {
        Id = id,
        Broken = broken
      };
    }

    public void Load() {
      if (!SaveLoadSystem.Instance.gameData.Robots.TryGetValue(id, out var data)) {
        return;
      }

      var isNew = string.IsNullOrEmpty(data.Id);
      if (isNew) {
        return;
      }

      broken = data.Broken;
    }
  }
}