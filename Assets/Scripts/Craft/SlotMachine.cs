using System.Collections.Generic;
using Interaction;
using Scriptables.Craft;
using Scriptables.Items;
using UnityEngine;
using World;

namespace Craft {
  public class SlotMachine : MonoBehaviour, IInteractable, IBaseCellHolder {
    [System.Serializable]
    public class SlotReward {
      public ItemObject itemData;
      [SerializeField] private Vector2Int amount;
      public float weight = 1;

      public int Amount() {
        var rand = Random.Range(amount.x, amount.y + 1);
        if (rand > 10) rand = Mathf.RoundToInt(Random.Range(amount.x, amount.y + 1) / 10f) * 10;
        return rand;
      }
    }

    [SerializeField] private string interactText;
    [SerializeField] private string holdInteractText;
    [SerializeField] private bool hasHoldInteraction = true;
    [SerializeField] private Recipe stationRecipe;
    [SerializeField] private Color destroyEffectColor = new(148, 198, 255, 255);
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] protected BuildingDataObject buildObject;
    [SerializeField] protected WorkstationObject stationObject;
    [SerializeField] protected ItemObject itemObject;

    private CellHolderHandler cellHandler;

    public string InteractionText => interactText;
    public bool HasHoldInteraction => hasHoldInteraction;
    public string HoldInteractionText => holdInteractText;
    public string HoldProcessText => holdInteractText;
    protected GameManager gameManager;

    #region Rewards

    [Header("Rewards")] public List<SlotReward> possibleRewards;
    [Header("Settings")] [Range(0f, 1f)] public float baseWinChance = 0.1f;
    [Range(0f, 1f)] public float maxWinChance = 0.8f;
    public float chanceIncreasePerLose = 0.08f;

    [Header("Runtime State (Debug)")] [SerializeField]
    private float currentWinChance;

    [SerializeField] private SlotReward slot1Result;
    [SerializeField] private SlotReward slot2Result;
    [SerializeField] private SlotReward slot3Result;
    [SerializeField] private bool lastResultWasWin;

    void Play() {
      Debug.Log("Play");
      bool win = RollWin();
      lastResultWasWin = win;

      if (win) {
        CalculateWin();
        ResetChance();
      }
      else {
        CalculateLose();
        currentWinChance = Mathf.Min(currentWinChance + chanceIncreasePerLose, maxWinChance);
      }
    }

    private bool RollWin() {
      return Random.value < currentWinChance;
    }

    private void CalculateWin() {
      // Step 1: Pick one reward from the list
      SlotReward reward = GetRandomRewardWeighted();

      // Step 2–4: Set the same reward to all 3 slots
      slot1Result = reward;
      slot2Result = reward;
      slot3Result = reward;

      // Here you can trigger VFX, add reward to inventory, etc.
      GiveReward(reward);
    }

    private void CalculateLose() {
      if (possibleRewards.Count < 3) {
        Debug.LogWarning("Need at least 3 unique rewards for lose sequence!");
        return;
      }

      // Step 1: Create a temp list to pick unique randoms
      List<SlotReward> tempList = new List<SlotReward>(possibleRewards);

      // Step 2–4: Assign random unique rewards to each slot
      slot1Result = PopRandom(tempList);
      slot2Result = PopRandom(tempList);
      slot3Result = PopRandom(tempList);
    }

    private SlotReward GetRandomRewardWeighted() {
      if (possibleRewards.Count == 0) return null;

      float totalWeight = 0;
      foreach (var r in possibleRewards)
        totalWeight += r.weight;
      float roll = Random.Range(0, totalWeight);
      float cumulative = 0;
      foreach (var r in possibleRewards) {
        cumulative += r.weight;
        if (roll < cumulative)
          return r;
      }

      return possibleRewards[0];
    }

    private SlotReward PopRandom(List<SlotReward> list) {
      int index = Random.Range(0, list.Count);
      SlotReward result = list[index];
      list.RemoveAt(index);
      return result;
    }

    private void GiveReward(SlotReward reward) {
      // Example: add to inventory, play effects, etc.
      Debug.Log($"🎁 Player receives: {reward.itemData.name} x{reward.Amount()}");
    }

    public SlotReward GetSlotResult(int slotIndex) {
      return slotIndex switch {
        1 => slot1Result,
        2 => slot2Result,
        3 => slot3Result,
        _ => null
      };
    }

    void ResetChance() {
      currentWinChance = baseWinChance;
    }

    #endregion


    public Bounds GetBounds() {
      return spriteRenderer ? spriteRenderer.bounds : new Bounds(transform.position, Vector3.zero);
    }

    protected void Awake() {
      gameManager = GameManager.Instance;
      cellHandler = new CellHolderHandler(OnAllBaseCellsDestroyed, stationRecipe, transform.position);
      ResetChance();
    }

    public bool Interact(PlayerInteractor playerInteractor) {
      //todo main interact
      Play();
      return true;
    }

    public bool HoldInteract(PlayerInteractor playerInteractor) {
      GameManager.Instance.PlayerInventory.TakeBuildingToInventory(buildObject, itemObject);

      return true;
    }

    public void ClearBaseCells() {
      cellHandler.ClearBaseCells();
    }

    public void SetBaseCells(List<CellData> cells) {
      cellHandler.SetBaseCells(cells, transform.position);
    }

    private void OnAllBaseCellsDestroyed() {
      var psGo = GameManager.Instance.PoolEffects
        .SpawnFromPool("CellDestroyEffect", transform.position, Quaternion.identity)
        .gameObject;
      var ps = psGo.GetComponent<ParticleSystem>();

      if (ps != null) {
        var main = ps.main;
        main.startColor = new ParticleSystem.MinMaxGradient(destroyEffectColor);
      }

      ResetChance();
      gameManager.PlaceCell.RemoveBuilding(buildObject, stationObject.InventoryItem);
      gameManager.MessagesManager.ShowSimpleMessage(stationObject.Title + " destroyed");
      gameManager.AudioController.PlayWorkstationDestroyed();
    }
  }
}