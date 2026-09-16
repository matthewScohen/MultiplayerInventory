using UnityEngine;
using Unity.Netcode;
using System;

public class Inventory : NetworkBehaviour
{
    [SerializeField] public const int InventorySize = 20;
    [SerializeField] private ItemTemplateDataBaseSO ItemTemplateDatabase;

    public NetworkList<int> InventoryList = new();
    public NetworkVariable<int> ActiveInventorySlot = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public Action itemPickedUp;

    public int ItemCount => CountItemsInInventory();

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;
        InitializeInventoryList();
    }

    /// <summary>
    /// Execute one of the currently held item's actions.
    /// <param name="actionID">The id of the action to use</param>
    /// </summary>
    public void UseHeldItem(int actionID)
    {
        int heldItemInstanceID = InventoryList[ActiveInventorySlot.Value];
        ItemRegistryEntry heldItemEntry = ItemRegistry.Instance.GetEntry(heldItemInstanceID);
        if(!heldItemEntry.IsValid)
            return;

        ItemTemplateSO heldItemTemplate = ItemTemplateDatabase.GetItemTemplate(heldItemEntry.TemplateID);

        heldItemTemplate.ServerUseRpc(NetworkObject, heldItemInstanceID, actionID);
        heldItemTemplate.ClientUseRpc(NetworkObject, heldItemInstanceID, actionID);
    }

    /// <summary>
    /// Attempt to pick up an item. Will return false if the item is invalid or already owned.
    /// </summary>
    /// <param name="instanceID"></param>
    /// <returns>If pickup succeeded</returns>
    public bool TryPickUpItem(int instanceID)
    {
        if(!IsServer) return false;

        ItemRegistryEntry entry = ItemRegistry.Instance.GetEntry(instanceID);
        if(!entry.IsValid || entry.Owned)
            return false;

        for(int i = 0; i < InventoryList.Count; i++)
        {
            if(InventoryList[i] == ItemRegistryEntry.InvalidId)
            {
                InventoryList[i] = instanceID;
                ItemRegistry.Instance.SetOwned(instanceID, true);
                itemPickedUp?.Invoke();
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Remove the item from the inventory
    /// </summary>
    /// <param name="inventoryIndex"></param>
    /// <returns>If drop succeeded</returns>
    public bool RemoveItem(int inventoryIndex)
    {
        Debug.Assert(IsServer, "Items can only be removed from an inventory by the server");
        if(!IsServer) return false;
        if(inventoryIndex < 0 || inventoryIndex > InventoryList.Count) return false;

        int instanceID = InventoryList[inventoryIndex];
        InventoryList[inventoryIndex] = ItemRegistryEntry.InvalidId;
        ItemRegistry.Instance.SetOwned(instanceID, false);

        return true;
    }

    /// <summary>
    /// Swap the position of two items in the inventory list
    /// </summary>
    /// <param name="itemIndexA"></param>
    /// <param name="itemIndexB"></param>
    [Rpc(SendTo.Server)]
    public void SwapItemsRpc(int itemIndexA, int itemIndexB)
    {
        int itemAInstanceID = InventoryList[itemIndexA];
        int itemBInstanceID = InventoryList[itemIndexB];
        InventoryList[itemIndexA] = itemBInstanceID;
        InventoryList[itemIndexB] = itemAInstanceID;
    }

    /// <summary>
    /// Get the inventory index of the item with intanceID
    /// </summary>
    /// <param name="instanceID"></param>
    /// <returns>The inventory index of the item with instanceID or -1 if the instanceID is not in the inventory</returns>
    public int GetInventoryIndex(int instanceID)
    {
        for(int i = 0; i < InventorySize; i++)
            if(InventoryList[i] == instanceID)
                return i;
        
        return -1;
    }

    /// <summary>
    /// Initialize the backpack and hotbar lists with invalid item instance IDs.
    /// </summary>
    private void InitializeInventoryList()
    {
        for (int i = 0; i < InventorySize; i++)
            InventoryList.Add(ItemRegistryEntry.InvalidId);
    }

    /// <summary>
    /// Count the number of valid items in the inventory
    /// </summary>
    /// <returns>Number of valid items in the inventory</returns>
    private int CountItemsInInventory()
    {
        int itemCount = 0;

        foreach(int instanceID in InventoryList)
            if(instanceID != ItemRegistryEntry.InvalidId)
                itemCount++;
        
        return itemCount;
    }
}