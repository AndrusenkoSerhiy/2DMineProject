using TMPro;
using UnityEngine;

namespace Interaction
{
  public class InteractUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _promt;
    private bool _isDisplayed;
    public bool IsDisplayed => _isDisplayed;

    private void Start() {
      HidePromt();
    }
    public void ShowPromt(string str){
      _promt.text = str;
      _promt.gameObject.SetActive(true);
      _isDisplayed = true;
    }

    public void HidePromt(){
      _isDisplayed = false;
      _promt.gameObject.SetActive(false);
    }

    public void UpdatePromt(string str) {
      if (str.Equals(_promt.text))
        return;
      
      _promt.text = str;
    }
  }
}
