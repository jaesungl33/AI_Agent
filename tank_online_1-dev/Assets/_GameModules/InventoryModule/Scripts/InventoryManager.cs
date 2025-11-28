using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages the player's inventory and handles item updates
/// </summary>
public class InventoryManager : Singleton<InventoryManager>, IInitializableManager
{
    public UnityAction<bool> OnInitialized { get; set; }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        EventManager.Register<InventoryItemEvent>(UpdateInventory);
        OnInitialized?.Invoke(true);
    }

    private void UnregisterEvents()
    {
        EventManager.Unregister<InventoryItemEvent>(UpdateInventory);
    }

    protected override void OnDestroy()
    {
        UnregisterEvents();
        base.OnDestroy();
    }

    private void UpdateInventory(InventoryItemEvent @event)
    {
        PlayerCollection playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        PlayerDocument playerDocument = playerCollection?.GetMine();

        if (playerDocument == null) return;
        if (!IsCanConsume(@event.itemType, @event.amount)) return;

        switch (@event.itemType)
        {
            case InventoryItemType.Gold:
                playerDocument.gold += @event.amount;
                break;

            case InventoryItemType.Diamond:
                playerDocument.diamond += @event.amount;
                break;

            case InventoryItemType.EXP:
                playerDocument.exp += @event.amount;
                if (playerDocument.exp < 0)
                    playerDocument.exp = 0;
                break;

            case InventoryItemType.Wrap:
                playerDocument.ownedWraps.Add(@event.itemType.ToString());
                break;

            case InventoryItemType.Decal:
                playerDocument.ownedDecals.Add(@event.itemType.ToString());
                break;

            case InventoryItemType.Sticker:
                playerDocument.ownedStickers.Add(@event.itemType.ToString());
                break;

            case InventoryItemType.AISticker:
                playerDocument.ownedAIStickers.Add(@event.itemType.ToString());
                break;
        }
        Debug.Log($"InventoryManager: Updated inventory for player {JsonUtility.ToJson(playerDocument)}");
        _ = playerCollection.UpdateDocumentAsync(playerDocument);
    }

    private bool IsCanConsume(InventoryItemType itemType, int amount)
    {
        PlayerCollection playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        PlayerDocument playerDocument = playerCollection?.GetMine();
        return itemType switch
        {
            InventoryItemType.Gold => playerDocument.gold + amount >= 0, // sample 1000 gold can consume -500 => true
            InventoryItemType.Diamond => playerDocument.diamond + amount >= 0,
            InventoryItemType.EXP => true, // EXP can be negative then reset to 0 later
            InventoryItemType.Wrap => !playerDocument.ownedWraps.Contains(itemType.ToString()) && amount > 0,
            InventoryItemType.Decal => !playerDocument.ownedDecals.Contains(itemType.ToString()) && amount > 0,
            InventoryItemType.Sticker => !playerDocument.ownedStickers.Contains(itemType.ToString()) && amount > 0,
            InventoryItemType.AISticker => !playerDocument.ownedAIStickers.Contains(itemType.ToString()) && amount > 0,
            _ => false
        };
    }
}

/// <summary>
/// Helper class for inventory operations
/// </summary>
public class InventoryHelper
{
    public static void ConsumeItem(InventoryItemType itemType, int amount)
    {
        // Tiêu thụ item từ inventory
        var consumeEvent = new InventoryItemEvent(itemType, -amount);
        EventManager.TriggerEvent(consumeEvent);
    }

    public static void RewardItem(InventoryItemType itemType, int amount)
    {
        // Nhận thưởng item vào inventory
        var rewardEvent = new InventoryItemEvent(itemType, amount);
        EventManager.TriggerEvent(rewardEvent);
    }

    public static List<string> GetAllTanksInGame()
    {
        var tankCollection = DatabaseManager.GetDB<TankCollection>();
        List<string> allTanks = new List<string>();

        foreach (var tank in tankCollection.GetAllDocuments())
        {
            if (tank.tankType != TankType.Outpost && tank.tankType != TankType.None)
                allTanks.Add(tank.tankId);
        }
        return allTanks;
    }

    public static List<string> GetOwnedTanks()
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null) return new List<string>();

        return playerDocument.tanks;
    }

    public static bool IsTankOwned(string tankID)
    {
        var ownedTanks = GetOwnedTanks();
        return ownedTanks.Contains(tankID);
    }

    public static int GetGold()
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null) return 0;

        return playerDocument.gold;
    }

    public static int GetDiamond()
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null) return 0;

        return playerDocument.diamond;
    }

    public static void BuyTank(InventoryItemType inventoryItemType, int amount, string tankID)
    {
        if (IsEnoughConsume(inventoryItemType, amount))
        {
            ConsumeItem(inventoryItemType, amount);
            OrderTank(tankID);
        }
        else
        {
            string message = LocalizationHelper.GetString(nameof(LocKeys.UI_Popup), LocKeys.UI_Popup.UI_Popup_NotEnoughCurrency);
            EventManager.TriggerEvent(new ShowPopupEvent(PopupIDs.Inform, new InformPopupParam() { message = message }));
            return;
        }
    }

    public static void OrderTank(string tankID)
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null) return;

        if (!playerDocument.tanks.Contains(tankID))
        {
            playerDocument.tanks.Add(tankID);
            _ = playerCollection.UpdateDocumentAsync(playerDocument);
        }
        else
        {
            Debug.LogWarning($"Tank {tankID} is already owned.");
        }
    }

    public static bool IsCanConsumeTank(string tankID)
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null) return false;

        return playerDocument.tanks.Contains(tankID);
    }

    public static bool IsEnoughConsume(InventoryItemType itemType, int amount)
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null) return false;

        return itemType switch
        {
            InventoryItemType.Gold => playerDocument.gold >= amount,
            InventoryItemType.Diamond => playerDocument.diamond >= amount,
            _ => false
        };
    }

    public static void SetSelectedTank(string tankID)
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null) return;

        playerDocument.selectedTank = tankID;
        _ = playerCollection.UpdateDocumentAsync(playerDocument);
    }

    public static string GetSelectedTank()
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null) return string.Empty;

        return playerDocument.selectedTank;
    }

    public static List<string> GetDeco(InventoryItemType inventoryItemType)
    {
        var playerCollection = DatabaseManager.GetDB<PlayerCollection>();
        var playerDocument = playerCollection?.GetMine();
        if (playerDocument == null)
        {
            Debug.LogWarning("InventoryHelper.GetDeco: playerDocument is null");
            return new List<string>();
        }

        switch (inventoryItemType)
        {
            case InventoryItemType.Wrap:
                // Get wrap logic
                return playerDocument.ownedWraps;
            case InventoryItemType.Decal:
                // Get decal logic
                return playerDocument.ownedDecals;
            case InventoryItemType.Sticker:
                // Get sticker logic
                return playerDocument.ownedStickers;
            case InventoryItemType.AISticker:
                // Get AI sticker logic
                return playerDocument.ownedAIStickers;
        }

        return new List<string>();
    }
}