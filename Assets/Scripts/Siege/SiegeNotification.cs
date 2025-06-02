using System;

namespace Siege {
  [Serializable]
  public class SiegeNotification {
    public float SecondsBeforeStart = 60f;
    public string Message = "Siege will start in 1 minute.";
  }
}