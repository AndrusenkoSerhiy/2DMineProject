using System.Collections.Generic;
using System.Linq;
using Settings;
using UnityEngine;

namespace Windows {
  public class WindowsController : MonoBehaviour {
    [SerializeField] private List<WindowBase> windowsList = new List<WindowBase>();
    private bool isAnyWindowOpen;
    public List<WindowBase> WindowsList => windowsList;
    public bool IsAnyWindowOpen => isAnyWindowOpen;
    
    public T GetWindow<T>() where T : WindowBase {
      return windowsList.OfType<T>().FirstOrDefault();
    }

    public void AddWindow(WindowBase window) {
      windowsList.Add(window);
      window.OnShow += OnWindowShow;
      window.OnHide += OnWindowHide;
    }

    public void RemoveWindow(WindowBase window) {
      window.OnShow -= OnWindowShow;
      window.OnHide -= OnWindowHide;
      windowsList.Remove(window);
    }

    private void Start() {
      WindowsEvents();
      //init windows (use for subscribe to player death in respawnWindow)
      foreach (var window in windowsList) {
        window.Init();
      }
    }

    private void WindowsEvents() {
      foreach (var window in windowsList) {
        window.OnShow += OnWindowShow;
        window.OnHide += OnWindowHide;
      }
    }

    private void OnWindowShow(WindowBase currentWindow) {
      foreach (var window in windowsList) {
        if (currentWindow != window && window.IsShow) {
          window.Hide();
        }
      }
      isAnyWindowOpen = true;
    }
    
    private void OnWindowHide(WindowBase currentWindow) {
      isAnyWindowOpen = false;
    }

    private void Update() {
      HandleEsc();
    }

    //try to hide active window
    private void HandleEsc() {
      if (GameManager.Instance.UserInput.controls.UI.Cancel.triggered) {
        var window = windowsList.Find(e => e.IsShow);
        //TODO 
        //need to replace this condition
        if(window && window.name.Equals("RespawnWindow"))
          return;
        
        if (window) {
          window.Hide();
        }
        else {
          GameManager.Instance.ShowInGameMenu();
        }
      }
    }
  }
}