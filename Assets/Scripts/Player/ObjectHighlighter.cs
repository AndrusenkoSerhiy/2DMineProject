using System.Collections.Generic;
using UnityEngine;

namespace Player {
  [RequireComponent(typeof(Collider2D))]
  public class ObjectHighlighter : MonoBehaviour {
    private List<ObjectHighlight> _highlights = new();

    public List<ObjectHighlight> Highlights => _highlights;

    public void OnTriggerEnter2D(Collider2D other) {
      var highlight = other.GetComponent<ObjectHighlight>();
      if (highlight && !_highlights.Contains(highlight)) {
        highlight.spriteRendererRef.material.SetFloat("_Thickness", 0.5f);
        _highlights.Add(highlight);
      }
    }

    public void OnTriggerExit2D(Collider2D other) {
      var highlight = other.GetComponent<ObjectHighlight>();
      if (highlight && _highlights.Contains(highlight)) {
        _highlights.Remove(highlight);
      }
    }

    public void ClearHighlights() {
      foreach (var highlight in _highlights) {
        highlight.spriteRendererRef.material.SetFloat("_Thickness", 0);
      }
      
      _highlights.Clear();
    }
  }
}