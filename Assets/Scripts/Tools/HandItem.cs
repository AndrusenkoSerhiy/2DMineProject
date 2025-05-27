using Scriptables.Items;
using UnityEngine;

namespace Tools {
  public class HandItem : MonoBehaviour {
    [SerializeField] private ItemObject item;
    public ItemObject Item => item;

    public virtual void StartUse() {
      if (item is Tool tool) {
        GameManager.Instance.AudioController.PlayAudio(tool.useSound);
      }
    }

    public virtual void EndUse() {
    }

    public virtual void Activate() {
    }
  }
}