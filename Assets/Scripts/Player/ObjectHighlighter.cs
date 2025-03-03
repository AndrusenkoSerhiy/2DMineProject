using System.Collections.Generic;
using UnityEngine;

namespace Player {
  [RequireComponent(typeof(Collider2D))]
  public class ObjectHighlighter : MonoBehaviour {
    private static readonly int Thickness = Shader.PropertyToID("_Thickness");
    [SerializeField] private int maxHighlights = 1;
    [SerializeField] private List<ObjectHighlight> highlights = new();
    [SerializeField] private List<ObjectHighlight> possibleHighlights = new();
    [SerializeField] private SpriteRenderer spriteRenderer;

    public List<ObjectHighlight> Highlights => highlights;

    public void SetMaxHighlights(int highlightCount) {
      maxHighlights = highlightCount;
    }

    public void EnableCrosshair(bool state) {
      spriteRenderer.enabled = state;
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
        EnableHighlight(highlight, 0);
      }
      //need remove from possible when we just entered but return to previous
      if (highlight && possibleHighlights.Contains(highlight)) {
        possibleHighlights.Remove(highlight);
      }
    }

    //when we enter in collider but not leave last
    private void AddFromPossible() {
      if (possibleHighlights.Count <= 0)
        return;
      
      EnableHighlight(possibleHighlights[0]);
      highlights.Add(possibleHighlights[0]);
      possibleHighlights.RemoveAt(0);
    }

    private void EnableHighlight(ObjectHighlight highlight, float thickness = .5f) {
      highlight.spriteRendererRef.material.SetFloat(Thickness, thickness);
    }
  }
}