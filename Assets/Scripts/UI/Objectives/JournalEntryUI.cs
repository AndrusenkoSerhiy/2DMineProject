using Scriptables.Objectives;
using UnityEngine;

namespace UI.Objectives {
  public class JournalEntryUI : MonoBehaviour {
    public JournalManager ManagerRef;
    public JournalEntrySO data;
    public GameObject isNewRef;
    public GameObject BackgroundSelectedRef;
    
    public void Select() {
      BackgroundSelectedRef.SetActive(true);
      ManagerRef.SetSelected(data.uniqueID);
      SetNew(false);
    }
    
    public void DeSelect() {
      BackgroundSelectedRef.SetActive(false);
    }
    
    public void SetNew(bool status) {
      isNewRef.SetActive(status);
    }
  }
}