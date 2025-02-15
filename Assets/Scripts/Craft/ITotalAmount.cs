using System;
using System.Collections.Generic;

namespace Craft {
  public interface ITotalAmount : ICraftComponent {
    public event Action<string> onResourcesTotalUpdate;
    public int GetResourceTotalAmount(string resourceId);
  }
}