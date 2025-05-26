using Audio;
using Inventory;
using Scriptables.Items;
using UI;
using UnityEngine;

namespace Tools {
  public class RangedTool : HandItem {
    [SerializeField] private Transform firePoint;

    private PlayerEquipment playerEquipment;
    private AudioController audioController;
    private DynamicCrosshair crosshair;
    private Tool tool;
    private BulletsPool bulletsPool;

    private void Awake() {
      playerEquipment = GameManager.Instance.PlayerEquipment;
      audioController = GameManager.Instance.AudioController;
      crosshair = GameManager.Instance.DynamicCrosshair;
      tool = (Tool)Item;
      bulletsPool = GameManager.Instance.BulletsPool;
    }

    public override void StartUse() {
      if (!playerEquipment.EquippedItem.CanShoot()) {
        return;
      }

      var firePos = firePoint.position;
      var bullet = bulletsPool.GetBullet(tool.ammo.Id);
      bullet.transform.position = firePos;

      var shootDirection = (crosshair.GetCenter() - firePos).normalized;

      bullet.Launch(shootDirection, tool, bulletsPool);
      audioController.PlayAudio(tool.useSound);
      playerEquipment.ConsumeAmmo();
    }
  }
}