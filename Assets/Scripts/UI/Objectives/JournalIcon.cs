using System;
using UnityEngine;
using Utility;

namespace UI.Objectives {
  public class JournalIcon : MonoBehaviour {
    public static JournalIcon Instance { get; private set; }
    public GameObject IsNewImageRef;
    private bool isInited = false;

    public void Awake() {
      if (Instance != null && Instance != this) {
        Destroy(gameObject);
        return;
      }

      Instance = this;
    }

    public void Init() {
      if (isInited) return;
      JournalManager.Instance.OnEntrySeen += CheckSeen;
      isInited = true;
    }

    public void CheckSeen() {
      var result = JournalManager.Instance.HasUnseen();
      IsNewImageRef.SetActive(result);
    }

    private void OnDisable() {
      if (!isInited) return;
      if (JournalManager.Instance != null) {
        JournalManager.Instance.OnEntrySeen -= CheckSeen;
      }
    }
  }
}