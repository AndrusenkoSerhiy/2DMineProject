using UnityEngine;

namespace Windows {
  public class WindowBase : MonoBehaviour {
    [SerializeField] private bool _isShow;

    protected virtual void Start() => Hide();
    public delegate void ShowWindow(WindowBase window);
    public event ShowWindow OnShow;

    public bool IsShow => _isShow;
    public virtual void Show() {
      _isShow = true;
      gameObject.SetActive(true);
      OnShow.Invoke(this);
    }

    public virtual void Hide() {
      _isShow = false;
      gameObject.SetActive(false);
    }
  }
}