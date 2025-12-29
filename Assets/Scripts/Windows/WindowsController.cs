using System;
using System.Collections.Generic;
using System.Linq;
using Settings;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Windows {
  public class WindowsController : MonoBehaviour {
    [SerializeField] private List<WindowBase> windowsList = new List<WindowBase>();
    private bool isAnyWindowOpen;
    public List<WindowBase> WindowsList => windowsList;
    public bool IsAnyWindowOpen => isAnyWindowOpen;

    [SerializeField] private JournalWindow journalWindow;
    
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

    private void Awake() {
      WindowsEvents();
      //init windows (use for subscribe to player death in respawnWindow)
      foreach (var window in windowsList) {
        window.Init();
      }
    }

    private void Start() {
      GameManager.Instance.UserInput.controls.UI.Cancel.performed += HandleEsc;
      GameManager.Instance.UserInput.controls.UI.Journal.performed += ctx => ShowJournal();
    }

    private void ShowJournal() {
      if (journalWindow.IsShow) {
        journalWindow.Hide();
      }
      else {
        journalWindow.Show();
      }
    }

    /*private void Start() {
      WindowsEvents();
      //init windows (use for subscribe to player death in respawnWindow)
      foreach (var window in windowsList) {
        window.Init();
      }
    }*/

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

    //try to hide active window
    private void HandleEsc(InputAction.CallbackContext ctx) {
      var window = windowsList.Find(e => e.IsShow);
      //TODO 
      //need to replace this condition
      if (window && window.name.Equals("RespawnWindow"))
        return;

      if (window) {
        window.Hide();
        //GameManager.Instance.UserInput.ShowCursor(false);
      }
      else {
        GameManager.Instance.MenuController.ShowInGameMenu();
      }
    }

    public void CloseActiveWindow() {
      var window = windowsList.Find(e => e.IsShow);
      if (window) {
        window.Hide();
      }
    }
  }
}