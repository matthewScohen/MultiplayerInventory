using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Inventory))]
public abstract class InventoryUI : MonoBehaviour
{
    protected readonly List<int> LocalInventoryList = new();
    private Inventory Inventory;

    private void Awake()
    {
        Inventory = GetComponent<Inventory>();
        IntializeLocalInventory();

        Inventory.InventoryList.OnListChanged += (changeEvent) => SyncLocalInventory();
    }

    /// <summary>
    /// Sync the UI with the current local inventory. Called whenever the local inventory is changed.
    /// The local inventory is synced whenever the server inventory changes.
    /// </summary>
    protected abstract void SyncUI();

    /// <summary>
    /// Swap two items in the inventory. Swap them locally first for UI responiveness and then send an
    /// Rpc to the server to perform the real swap.
    /// </summary>
    /// <param name="itemIndexA"></param>
    /// <param name="itemIndexB"></param>
    public void SwapItems(int itemIndexA, int itemIndexB)
    {
        // Adjust local list first for UI responsiveness then send update to server
        int itemAInstanceID = LocalInventoryList[itemIndexA];
        int itemBInstanceID = LocalInventoryList[itemIndexB];
        LocalInventoryList[itemIndexA] = itemBInstanceID;
        LocalInventoryList[itemIndexB] = itemAInstanceID;

        Inventory.SwapItemsRpc(itemIndexA, itemIndexB);
    }

    /// <summary>
    /// Sync the local inventory used for UI to the truth source networked inventory list
    /// </summary>
    private void SyncLocalInventory()
    {
        for(int i = 0; i < Inventory.InventoryList.Count; i++)
            LocalInventoryList[i] = Inventory.InventoryList[i];

        SyncUI();
    }

    private void IntializeLocalInventory()
    {
        for(int i = 0; i < Inventory.InventorySize; i++)
            LocalInventoryList.Add(ItemRegistryEntry.InvalidId);

        SyncUI();
    }
}