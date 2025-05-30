using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using UnityEngine.Rendering;

namespace Messages {
  public static class MessageTemplateVariables {
    public static string name = "{name}";
    public static string amount = "{amount}";
  }

  public enum Position {
    Top,
    Bottom,
    Left,
    Right
  }

  public class MessagesManager : MonoBehaviour {
    public Transform topContainer;
    public Transform bottomContainer;
    public Transform leftContainer;
    public Transform rightContainer;

    public GameObject messagePrefab;
    public GameObject messageRecipePrefab;

    public int maxMessagesPerZone = 3;
    public float messageDuration = 5f;

    private SerializedDictionary<MessageType, string> GetDefaultMessageTemplates() {
      return new SerializedDictionary<MessageType, string> {
        { MessageType.SimpleText, $"{MessageTemplateVariables.name}" },
        { MessageType.NewRecipe, $"New recipe: {MessageTemplateVariables.name}" },
        { MessageType.ResourceAdded, $"Added x {MessageTemplateVariables.amount} {MessageTemplateVariables.name}" },
        { MessageType.CraftSuccess, $"Crafted x {MessageTemplateVariables.amount} {MessageTemplateVariables.name}" }, {
          MessageType.ResourcePickup, $"Picked up x {MessageTemplateVariables.amount} {MessageTemplateVariables.name}"
        },
        { MessageType.ResourceDropped, $"Dropped x {MessageTemplateVariables.amount} {MessageTemplateVariables.name}" }
      };
    }

    private SerializedDictionary<MessageType, Position> GetDefaultMessagePositions() {
      return new SerializedDictionary<MessageType, Position> {
        { MessageType.SimpleText, Position.Top },
        { MessageType.NewRecipe, Position.Top },
        { MessageType.ResourceAdded, Position.Left },
        { MessageType.CraftSuccess, Position.Right },
        { MessageType.ResourcePickup, Position.Left },
        { MessageType.ResourceDropped, Position.Right }
      };
    }

    [Tooltip("Available variables: {name}, {amount}")]
    public SerializedDictionary<MessageType, string> messageTemplates;

    public SerializedDictionary<MessageType, Position> messagePositions;

    public List<MessageUI> messagePool = new();
    public List<MessageUI> messageRecipePool = new();

    private Dictionary<Transform, List<MessageUI>> activeMessages = new();

    private void Awake() {
      activeMessages[topContainer] = new List<MessageUI>();
      activeMessages[bottomContainer] = new List<MessageUI>();
      activeMessages[leftContainer] = new List<MessageUI>();
      activeMessages[rightContainer] = new List<MessageUI>();
      messageTemplates = GetDefaultMessageTemplates();
      messagePositions = GetDefaultMessagePositions();

      GameManager.Instance.OnGamePaused += OnGamePausedHandler;
    }

    [ContextMenu("Set default messages settings")]
    private void SetDefaultMessagesSettings() {
      messageTemplates = GetDefaultMessageTemplates();
      messagePositions = GetDefaultMessagePositions();
    }

    public void ShowSimpleMessage(string text) {
      var position = messagePositions[MessageType.SimpleText];
      ShowMessage(MessageType.SimpleText, position, text);
    }

    public void ShowAddResourceMessage(ItemObject item, int amount) {
      if (amount <= 0) {
        return;
      }

      var position = messagePositions[MessageType.ResourceAdded];
      ShowMessage(MessageType.ResourceAdded, position, item.Name, item.Id, amount, item.UiDisplay);
    }

    public void ShowCraftMessage(ItemObject item, int amount) {
      if (amount <= 0) {
        return;
      }

      var position = messagePositions[MessageType.CraftSuccess];
      ShowMessage(MessageType.CraftSuccess, position, item.Name, item.Id, amount, item.UiDisplay);
    }

    public void ShowNewRecipeMessage(Recipe recipe) {
      var position = messagePositions[MessageType.NewRecipe];
      var additionalMessage = recipe.RecipeTypes.Count > 0
        ? $"{string.Join(", ", recipe.RecipeTypes)}"
        : null;
      ShowMessage(MessageType.NewRecipe, position, recipe.RecipeName, recipe.Id, null, null, additionalMessage);
    }

    public void ShowPickupResourceMessage(ItemObject item, int amount) {
      if (amount <= 0) {
        return;
      }

      var position = messagePositions[MessageType.ResourcePickup];
      ShowMessage(MessageType.ResourcePickup, position, item.Name, item.Id, amount, item.UiDisplay);
    }

    public void ShowDroppedResourceMessage(ItemObject item, int amount) {
      if (amount <= 0) {
        return;
      }

      var position = messagePositions[MessageType.ResourceDropped];
      ShowMessage(MessageType.ResourceDropped, position, item.Name, item.Id, amount, item.UiDisplay);
    }

    private void ShowMessage(
      MessageType type,
      Position position,
      string msgName,
      [CanBeNull] string entityId = null,
      int? amount = null,
      [CanBeNull] Sprite icon = null,
      [CanBeNull] string additionalMessage = null) {
      var container = GetContainer(position);
      var template = messageTemplates[type];

      if (UpdateExistingMessage(container, entityId, amount)) {
        return;
      }

      // var message = type == MessageType.NewRecipe ? GetRecipeMessageFromPool() : GetMessageFromPool();
      var message = GetMessageFromPool(position != Position.Top);
      message.transform.SetParent(container, false);
      message.gameObject.SetActive(true);

      message.SetTemplate(template)
        .SetDuration(messageDuration)
        .SetEntityId(entityId)
        .SetName(msgName)
        .SetAdditionalMessage(additionalMessage)
        .SetAmount(amount)
        .SetIcon(icon)
        .SetHideCallback(msg => ReturnMessage(container, msg))
        .Setup();

      activeMessages[container].Add(message);

      TrimMessages(container);
    }

    private bool UpdateExistingMessage(Transform container, [CanBeNull] string entityId, int? amount) {
      if (amount == null || entityId == null) {
        return false;
      }

      foreach (var msg in activeMessages[container]) {
        if (msg.MatchesResource(entityId)) {
          msg.TryUpdateAmount((int)amount);
          return true;
        }
      }

      return false;
    }

    private void TrimMessages(Transform container) {
      while (activeMessages[container].Count > maxMessagesPerZone) {
        var oldestMessage = activeMessages[container][0];
        ReturnMessage(container, oldestMessage);
      }
    }

    private void ReturnMessage(Transform container, MessageUI message) {
      message.gameObject.SetActive(false);
      activeMessages[container].Remove(message);

      var pool = container != topContainer ? messagePool : messageRecipePool;
      pool.Add(message);
      /*Debug.LogError("Return :" + container.name, container.gameObject);
      if (container == topContainer) messageRecipePool.Add(message);
      else messagePool.Add(message);*/
    }

    private void OnGamePausedHandler() {
      foreach (var (container, messages) in activeMessages) {
        foreach (var message in messages.ToList()) {
          ReturnMessage(container, message);
        }
      }
    }

    private MessageUI GetMessageFromPool(bool main = true) {
      var pool = main ? messagePool : messageRecipePool;
      if (pool.Count > 0) {
        var message = pool[0];
        pool.RemoveAt(0);
        return message;
      }

      var msgPrefab = main ? messagePrefab : messageRecipePrefab;
      var newMessage = Instantiate(msgPrefab).GetComponent<MessageUI>();

      return newMessage;
    }

    private Transform GetContainer(Position position) {
      return position switch {
        Position.Top => topContainer,
        Position.Bottom => bottomContainer,
        Position.Left => leftContainer,
        Position.Right => rightContainer,
        _ => topContainer
      };
    }
  }
}