using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines different types of inventory items
/// </summary>
public enum InventoryItemType
{
    Gold,
    Diamond,
    Tank,
    Wrap,
    Decal,
    Sticker,
    AISticker,
    Achievement,
    EXP
}

/// <summary>
/// Base class for all inventory items
/// </summary>
[Serializable]
public abstract class InventoryItem
{
    public string id;
    public string name;
    public string description;
    public InventoryItemType itemType;
    public Sprite icon;
    public bool isStackable;
    public int maxStackSize = 1;
    
    public InventoryItem(string id, string name, InventoryItemType itemType)
    {
        this.id = id;
        this.name = name;
        this.itemType = itemType;
        this.isStackable = false;
    }
    
    public virtual bool CanStackWith(InventoryItem other)
    {
        return isStackable && other != null && other.id == this.id;
    }
}

/// <summary>
/// Tank items
/// </summary>
[Serializable]
public class TankItem : InventoryItem
{
    public string tankId;
    public int rarity;
    public Dictionary<string, object> stats;
    
    public TankItem(string tankId, string name) 
        : base(tankId, name, InventoryItemType.Tank)
    {
        this.tankId = tankId;
        this.stats = new Dictionary<string, object>();
        this.isStackable = false;
    }
}

/// <summary>
/// Cosmetic items (Wraps, Decals, Stickers)
/// </summary>
[Serializable]
public class CosmeticItem : InventoryItem
{
    public string cosmeticId;
    public int rarity;
    public bool isAI;
    
    public CosmeticItem(string cosmeticId, string name, InventoryItemType itemType, bool isAI = false) 
        : base(cosmeticId, name, itemType)
    {
        this.cosmeticId = cosmeticId;
        this.isAI = isAI;
        this.isStackable = false;
    }
}

/// <summary>
/// Achievement items
/// </summary>
[Serializable]
public class AchievementItem : InventoryItem
{
    public string achievementId;
    public int progress;
    public int maxProgress;
    public bool isCompleted;
    public DateTime unlockedDate;

    public AchievementItem(string achievementId, string name, int progress = 0, int maxProgress = 1) 
        : base(achievementId, name, InventoryItemType.Achievement)
    {
        this.achievementId = achievementId;
        this.progress = progress;
        this.maxProgress = maxProgress;
        this.isCompleted = progress >= maxProgress;
        this.isStackable = false;
    }
    
    public void UpdateProgress(int newProgress)
    {
        bool wasCompleted = isCompleted;
        progress = newProgress;
        isCompleted = progress >= maxProgress;
        
        if (!wasCompleted && isCompleted)
        {
            unlockedDate = DateTime.Now;
        }
    }
}

/// <summary>
/// Inventory slot containing an item and quantity
/// </summary>
[Serializable]
public class InventorySlot
{
    public InventoryItem item;
    public int quantity;
    public DateTime addedDate;
    public DateTime lastModified;

    public InventorySlot(InventoryItem item, int quantity = 1)
    {
        this.item = item;
        this.quantity = quantity;
        this.addedDate = DateTime.Now;
        this.lastModified = DateTime.Now;
    }

    public bool CanAddItem(InventoryItem newItem, long amount)
    {
        if (item == null) return true;

        return item.CanStackWith(newItem) &&
               (quantity + amount <= item.maxStackSize);
    }

    public void AddItem(InventoryItem newItem, int amount)
    {
        if (item == null)
        {
            item = newItem;
            quantity = amount;
            addedDate = DateTime.Now;
        }
        else if (CanAddItem(newItem, amount))
        {
            quantity += amount;
        }

        lastModified = DateTime.Now;
    }

    public bool RemoveItem(int amount)
    {
        if (quantity >= amount)
        {
            quantity -= amount;
            lastModified = DateTime.Now;

            if (quantity <= 0)
            {
                item = null;
                quantity = 0;
            }

            return true;
        }

        return false;
    }

    public bool IsEmpty => item == null || quantity <= 0;
}

public class InventoryItemEvent
{
    public InventoryItemType itemType;
    public int amount;

    public InventoryItemEvent(InventoryItemType itemType, int amount)
    {
        this.itemType = itemType;
        this.amount = amount;
    }
}