using Scriptables.Items;
using UnityEngine;

namespace Items {
  public class GroundItem : MonoBehaviour {
    [SerializeField] private int _count = 1;
    public ItemObject item;
    public bool IsPicked;

    public int Count {
      get { return _count; }
      set { _count = value; }
    }
  }
}
