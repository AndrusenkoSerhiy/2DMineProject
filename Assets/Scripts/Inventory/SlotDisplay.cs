using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory {
  public class SlotDisplay : MonoBehaviour {
    [SerializeField] private GameObject outline;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI text;

    public TextMeshProUGUI Text => text;
    public Image Background => background;

    public void ActivateOutline() {
      outline.SetActive(true);
    }

    public void DeactivateOutline() {
      outline.SetActive(false);
    }
  }
}