using System;
using System.Collections.Generic;
using System.Linq;
using Settings;
using UnityEngine;

namespace Windows {
  public class WindowsController : MonoBehaviour {
    [SerializeField] private List<WindowBase>_windowsList = new List<WindowBase>();

    public List<WindowBase> WindowsList => _windowsList;
    
    public T GetWindow<T>() where T : WindowBase {
      return _windowsList.OfType<T>().FirstOrDefault();
    }

    private void Update() {
      HandleEsc();
    }

    //try to hide active window
    private void HandleEsc() {
      if (UserInput.instance.controls.UI.Cancel.triggered) {
        var window = _windowsList.Find(e => e.IsShow);
        if (window) {
          window.Hide();
        }
      }
    }
  }
}