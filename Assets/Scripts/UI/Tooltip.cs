using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
  public class Tooltip : MonoBehaviour {
    public TextMeshProUGUI headerField;
    public TextMeshProUGUI contentField;
    public LayoutElement layoutElement;
    public int characterWrapLimit;

    private RectTransform rectTransform;

    private void Awake() {
      rectTransform = GetComponent<RectTransform>();
      
      GameManager.Instance.OnGamePaused += () => gameObject.SetActive(false);
    }

    private void Update() {
      Vector2 position = Input.mousePosition;

      var pivotX = position.x / Screen.width;
      var pivotY = position.y / Screen.height;

      rectTransform.pivot = new Vector2(pivotX, pivotY > 0.5f ? 1 : 0);

      transform.position = position;
    }


    public void SetText(string content, string header) {
      if (string.IsNullOrEmpty(header)) {
        headerField.gameObject.SetActive(false);
      }
      else {
        headerField.text = header;
        headerField.gameObject.SetActive(true);
      }

      contentField.text = content;
      UpdateWidth();
    }

    private void UpdateWidth() {
      var headerLength = headerField.text.Length;
      var contentLength = contentField.text.Length;

      layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit);
    }
  }
}