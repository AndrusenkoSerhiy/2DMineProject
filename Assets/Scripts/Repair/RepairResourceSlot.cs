using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Repair {
  public class RepairResourceSlot : MonoBehaviour {
    [SerializeField] private Image blockFade;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI requiredCountText;

    private Color defaultBgColor;
    private GameObject blockFadeGameObject;
    private EventTrigger trigger;
    private InventorySlot slot;
    private int requiredCount;

    public bool IsEnough() {
      return slot.amount >= requiredCount;
    }

    public void Init(int count, InventorySlot inventorySlot) {
      slot = inventorySlot;
      defaultBgColor = background.color;
      requiredCount = count;
      requiredCountText.text = count.ToString();
    }

    public void ResetBackgroundColor() {
      background.color = defaultBgColor;
    }

    public void Blink(Color blinkBgColor) {
      background.color = blinkBgColor;
    }

    public void Block() {
      GetTrigger().enabled = false;
      GetFadeGameObject().SetActive(true);
    }

    public void UnBlock() {
      GetTrigger().enabled = true;
      GetFadeGameObject().SetActive(false);
    }

    private GameObject GetFadeGameObject() {
      if (blockFadeGameObject == null) {
        blockFadeGameObject = blockFade.gameObject;
      }

      return blockFadeGameObject;
    }

    private EventTrigger GetTrigger() {
      if (trigger == null) {
        trigger = gameObject.GetComponent<EventTrigger>();
      }

      return trigger;
    }
  }
}