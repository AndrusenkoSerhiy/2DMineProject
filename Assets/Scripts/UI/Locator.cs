using System.Collections.Generic;
using Scriptables.Items;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI {
  public class Locator : MonoBehaviour {
    [SerializeField] private Image arrowImage;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private GameObject pointerUI;
    [SerializeField] private Camera cam;
    [SerializeField] private List<ItemObject> allowedBuildings;

    private Coords? targetGridCoords;
    private Vector3? target;
    private GameManager gameManager;

    private void Awake() {
      gameManager = GameManager.Instance;
    }

    private void Update() => CheckArea();

    public void SetTarget(Vector3 position, Coords coords, ItemObject itemObject) {
      if (!itemObject || !allowedBuildings.Contains(itemObject)) {
        return;
      }

      iconImage.sprite = itemObject.UiDisplay;
      target = position;
      targetGridCoords = coords;
    }

    public void RemoveTarget(Coords coords) {
      if (targetGridCoords == null) {
        return;
      }

      if (!coords.Equals(targetGridCoords)) {
        return;
      }

      targetGridCoords = null;
      target = null;
    }

    private void CheckArea() {
      if (target == null) {
        return;
      }

      var screenPos = cam.WorldToScreenPoint((Vector3)target);

      if (screenPos.x > 0 && screenPos.x < Screen.width &&
          screenPos.y > 0 && screenPos.y < Screen.height) {
        pointerUI.gameObject.SetActive(false);
      }
      else {
        pointerUI.gameObject.SetActive(true);

        var playerPos = gameManager.PlayerController.PlayerCoords.GetPosition();
        var directionToTarget = ((Vector3)target - playerPos).normalized;
        arrowImage.transform.up = directionToTarget;

        iconImage.rectTransform.rotation = Quaternion.identity;

        var distance = Vector3.Distance(playerPos, (Vector3)target) / gameManager.GameConfig.CellSizeX;
        distanceText.text = $"{distance:F1} m";
      }
    }
  }
}