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
        highlight.spriteRendererRef.sortingOrder++;
        highlight.spriteRendererRef.material.SetFloat("_Thickness", 1f);
        _highlights.Add(highlight);
      }
    }

    public void OnTriggerExit2D(Collider2D other) {
      var highlight = other.GetComponent<ObjectHighlight>();
      if (highlight && _highlights.Contains(highlight)) {
        highlight.spriteRendererRef.material.SetFloat("_Thickness", 0);
        highlight.spriteRendererRef.sortingOrder--;
        _highlights.Remove(highlight);
      }
    }
  }
}