namespace Craft {
  public static class Helper {
    public static string SecondsToTimeString(int totalSeconds) {
      int hours = totalSeconds / 3600;
      int minutes = (totalSeconds % 3600) / 60;
      int seconds = totalSeconds % 60;
      return hours > 0 ? $"{hours:D2}:{minutes:D2}:{seconds:D2}" : $"{minutes:D2}:{seconds:D2}";
    }
  }
}