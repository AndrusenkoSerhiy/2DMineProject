using System.Collections.Generic;
using UnityEngine;

namespace Player {
  [RequireComponent(typeof(Collider2D))]
  public class ObjectHighlighter : MonoBehaviour {
    [SerializeField] private int maxHighlights = 1;
    [SerializeField] private List<ObjectHighlight> highlights = new();

    [SerializeField] private List<ObjectHighlight> possibleHighlights = new();

    // [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject defaultCrosshair;
    [SerializeField] private GameObject rangeCrosshair;

    public List<ObjectHighlight> Highlights => highlights;
    private GameObject currentCrosshair;

    public void SetMaxHighlights(int highlightCount) {
      maxHighlights = highlightCount;
    }

    public void EnableCrosshair(bool state) {
      GetCurrentCrosshair().SetActive(state);
      // spriteRenderer.enabled = state;
    }

    public void ChangeCrosshair(bool isRange = false) {
      if (!rangeCrosshair || !defaultCrosshair) {
        return;
      }

      var newCrosshair = isRange ? rangeCrosshair : defaultCrosshair;
      var oldCrosshair = isRange ? defaultCrosshair : rangeCrosshair;

      var curr = GetCurrentCrosshair();
      var shouldBeActive = curr && curr.activeSelf;

      newCrosshair.SetActive(shouldBeActive);
      oldCrosshair.SetActive(false);

      currentCrosshair = newCrosshair;
    }

    public void OnTriggerEnter2D(Collider2D other) {
      var highlight = other.GetComponent<ObjectHighlight>();

      if (highlights.Count >= maxHighlights) {
        if (highlight && !highlights.Contains(highlight)) {
          possibleHighlights.Add(highlight);
        }

        return;
      }

      if (highlight && !highlights.Contains(highlight)) {
        EnableHighlight(highlight);
        highlights.Add(highlight);
      }
    }

    public void OnTriggerExit2D(Collider2D other) {
      var highlight = other.GetComponent<ObjectHighlight>();
      if (highlight && highlights.Contains(highlight)) {
        highlights.Remove(highlight);
        AddFromPossible();
        EnableHighlight(highlight, false);
      }

      //need remove from possible when we just entered but return to previous
      if (highlight && possibleHighlights.Contains(highlight)) {
        possibleHighlights.Remove(highlight);
      }
    }

    private GameObject GetCurrentCrosshair() {
      if (!currentCrosshair) {
        currentCrosshair = defaultCrosshair;
      }

      return currentCrosshair;
    }

    //when we enter in collider but not leave last
    private void AddFromPossible() {
      if (possibleHighlights.Count <= 0)
        return;

      EnableHighlight(possibleHighlights[0]);
      highlights.Add(possibleHighlights[0]);
      possibleHighlights.RemoveAt(0);
    }

    private void EnableHighlight(ObjectHighlight highlight, bool state = true) {
      highlight.SetHighlight(state);
      //highlight.spriteRendererRef.material.SetFloat(Thickness, thickness);
    }
  }
}