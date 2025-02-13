using System.Collections.Generic;
using System.Linq;
using Settings;
using UnityEngine;

namespace Windows {
  public class WindowsController : MonoBehaviour {
    [SerializeField] private List<WindowBase> windowsList = new List<WindowBase>();

    public List<WindowBase> WindowsList => windowsList;

    public T GetWindow<T>() where T : WindowBase {
      return windowsList.OfType<T>().FirstOrDefault();
    }

    public void AddWindow(WindowBase window) {
      windowsList.Add(window);
      window.OnShow += OnWindowShow;
    }

    public void RemoveWindow(WindowBase window) {
      window.OnShow -= OnWindowShow;
      windowsList.Remove(window);
    }

    private void Start() {
      WindowsEvents();
    }

    private void WindowsEvents() {
      foreach (var window in windowsList) {
        window.OnShow += OnWindowShow;
      }
    }

    private void OnWindowShow(WindowBase currentWindow) {
      foreach (var window in windowsList) {
        if (currentWindow != window && window.IsShow) {
          window.Hide();
        }
      }
    }

    private void Update() {
      HandleEsc();
    }

    //try to hide active window
    private void HandleEsc() {
      if (UserInput.instance.controls.UI.Cancel.triggered) {
        var window = windowsList.Find(e => e.IsShow);
        if (window) {
          window.Hide();
        }
      }
    }
  }
}