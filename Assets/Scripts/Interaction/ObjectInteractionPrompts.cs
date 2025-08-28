using TMPro;
using UnityEngine;

namespace Interaction {
  public class ObjectInteractionPrompts : MonoBehaviour {
    [Header("Refs")] [SerializeField] private TextMeshProUGUI interaction;
    [SerializeField] private TextMeshProUGUI holdInteraction;

    [Header("Positioning")] [SerializeField]
    private float yOffset = 0.5f;

    [SerializeField] private float yOffsetWithHold = 1f;

    private RectTransform selfRect;
    private Camera cam;
    private Canvas canvas;

    private bool hasTrackedWorldPos;

    private bool interactionVisible;
    private bool holdVisible;

    private void Awake() {
      selfRect = transform as RectTransform;
      canvas = GameManager.Instance.Canvas;
      cam = GameManager.Instance.MainCamera;

      if (selfRect == null) {
        return;
      }

      selfRect.pivot = new Vector2(0f, 0f);
      selfRect.anchorMin = new Vector2(0.5f, 0.5f);
      selfRect.anchorMax = new Vector2(0.5f, 0.5f);
    }

    /*private void Update() {
      if (!interactionVisible && !holdVisible) return;

      Vector3 baseWorldPos;
      if (trackedInteractable != null) {
        var bounds = trackedInteractable.GetBounds();
        baseWorldPos = new Vector3(bounds.center.x, bounds.max.y, bounds.center.z);
      }
      else if (hasTrackedWorldPos) {
        baseWorldPos = trackedWorldPos;
      }
      else {
        return;
      }

      SetUiToWorld(baseWorldPos);
    }*/

    public void ShowInteractionPrompt(IInteractable interactable, string str = "") {
      if (interactable == null) {
        return;
      }

      TrackRenderer(interactable);
      ShowPrompt(interaction, str);
      interactionVisible = true;

      var bounds = interactable.GetBounds();

      SetUiToWorld(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
    }

    public void ShowHoldInteractionPrompt(IInteractable interactable, string str = "") {
      if (interactable == null) {
        return;
      }

      TrackRenderer(interactable);
      ShowPrompt(holdInteraction, str);
      holdVisible = true;

      var bounds = interactable.GetBounds();
      SetUiToWorld(new Vector3(bounds.center.x, bounds.max.y, bounds.center.z));
    }

    public void HideInteractionPrompt() {
      interactionVisible = false;
      interaction.gameObject.SetActive(false);
      StopTrackingIfNothingVisible();
    }

    public void HideHoldInteractionPrompt() {
      holdVisible = false;
      holdInteraction.gameObject.SetActive(false);
      StopTrackingIfNothingVisible();
    }

    public void UpdateSpriteAsset() {
      var index = (int)GameManager.Instance.UserInput.GetActiveGameDevice();
      interaction.spriteAsset = GameManager.Instance.ListOfTmpSpriteAssets.SpriteAssets[index];
      holdInteraction.spriteAsset = GameManager.Instance.ListOfTmpSpriteAssets.SpriteAssets[index];
    }

    private void ShowPrompt(TextMeshProUGUI holder, string str) {
      if (holder.gameObject.activeSelf && holder.text == str) return;
      holder.text = str;
      holder.gameObject.SetActive(true);
    }

    private void TrackRenderer(IInteractable interactable) {
      hasTrackedWorldPos = false;
    }

    private void StopTrackingIfNothingVisible() {
      if (!interactionVisible && !holdVisible) {
        hasTrackedWorldPos = false;
      }
    }

    private void SetUiToWorld(Vector3 basePos) {
      var offset = (interactionVisible && holdVisible) ? yOffsetWithHold : yOffset;
      var worldPosWithOffset = new Vector3(basePos.x, basePos.y + offset, basePos.z);

      if (canvas && canvas.renderMode == RenderMode.WorldSpace) {
        transform.position = worldPosWithOffset;
        return;
      }

      var screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPosWithOffset);
      var canvasRect = canvas.transform as RectTransform;
      var forUtil = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

      if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, forUtil,
            out var localPoint)) {
        selfRect.anchoredPosition = new Vector2(localPoint.x - 14, localPoint.y);
      }
    }
  }
}