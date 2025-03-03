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

    // Always return the current time in UTC
    public static DateTime GetCurrentTime() {
      return DateTime.UtcNow;
    }
    
    public static string FormatDate(DateTime date) {
      return date.ToString("O");
    }

    // Always parse expecting UTC
    public static DateTime ParseDate(string date) {
      return DateTime.Parse(date, null, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    }
  }
}