using System;
using System.Globalization;

namespace Craft {
  public static class Helper {
    public static string SecondsToTimeString(float totalSeconds) {
      var hours = (int)(totalSeconds / 3600);
      var minutes = (int)((totalSeconds % 3600) / 60);
      var seconds = (int)(totalSeconds % 60);
      return hours > 0 ? $"{hours:D2}:{minutes:D2}:{seconds:D2}" : $"{minutes:D2}:{seconds:D2}";
    }
    
    public static long GetCurrentTimestampMillis() {
      return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
    
    public static string FormatTimestamp(long timestampMillis) {
      var date = DateTimeOffset.FromUnixTimeMilliseconds(timestampMillis).UtcDateTime;
      return date.ToString("O"); // or any other format
    }
  }
}