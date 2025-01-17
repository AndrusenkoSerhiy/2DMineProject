using UnityEngine;

namespace Windows {
  public class WindowBase : MonoBehaviour {
    [SerializeField] private bool _isShow;

    public bool IsShow => _isShow;
    public virtual void Show() {
      _isShow = true;
      gameObject.SetActive(true);
    }

    public virtual void Hide() {
      _isShow = false;
      gameObject.SetActive(false);
    }
  }
}