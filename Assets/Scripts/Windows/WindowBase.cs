using Player;
using Settings;
using UnityEngine;

namespace Windows {
  public class WindowBase : MonoBehaviour {
    [SerializeField] private bool isShow;
    public bool IsShow => isShow;
    public delegate void ShowWindow(WindowBase window);
    public event ShowWindow OnShow;
    public event ShowWindow OnHide;
    
    // protected virtual void Start() {
    //   Hide();
    // }

    private PlayerControllerBase GetCurrPlayerController() {
      return GameManager.Instance.CurrPlayerController;
    }
    
    public virtual void Show() {
      isShow = true;
      gameObject.SetActive(true);
      OnShow?.Invoke(this);
      LockPlayer(true);
      LockHighlight(true);
    }

    public virtual void Hide() {
      isShow = false;
      gameObject.SetActive(false);
      OnHide?.Invoke(this);
      LockPlayer(false);
      LockHighlight(false);
    }
    
    private void LockPlayer(bool state) {
      GetCurrPlayerController().SetLockPlayer(state);
      UserInput.instance.EnableGamePlayControls(!state);
    }

    private void LockHighlight(bool state) {
      GetCurrPlayerController().SetLockHighlight(state);
    }
  }
}