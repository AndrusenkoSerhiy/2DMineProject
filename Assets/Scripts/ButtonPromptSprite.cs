using UnityEngine.InputSystem;

public static class ButtonPromptSprite {
  public static string GetSpriteName(InputAction action) {
    var index = (int)GameManager.Instance.UserInput.GetActiveGameDevice();
    var inputBinding = action.GetBindingForControl(action.controls[index]);
    if (inputBinding == null)
      return string.Empty;

    var buttonName = inputBinding.Value.path;
    buttonName = buttonName.Replace("<Keyboard>/", "Keyboard_");
    buttonName = buttonName.Replace("<Gamepad>/", "Gamepad_");
    return buttonName;
  }

  public static string GetFullPrompt(string actionName, string buttonName, bool isLeft = false, bool hold = false ) {
    if (isLeft) {
      var holdStr = hold ? "_hold" : "";
      return "<sprite name=" + buttonName+holdStr + ">" + " " + actionName;
    }

    return actionName + " " + "<sprite name=" + buttonName + ">";
  }

  //use for respawn window
  public static string GetSpriteTag(string buttonName) {
    return "<sprite name=" + buttonName + ">";
  }
}