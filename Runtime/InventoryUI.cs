using UnityEngine;
using System.Collections.ObjectModel;

[RequireComponent(typeof(Inventory))]
public class InventoryUI : MonoBehaviour
{
    private readonly ObservableCollection<int> LocalInventoryList = new();
    private Inventory Inventory;

    private void Awake()
    {
        Inventory = GetComponent<Inventory>();
        IntializeLocalInventory();

        Inventory.InventoryList.OnListChanged += (changeEvent) => SyncLocalInventory();
        LocalInventoryList.CollectionChanged += (sender, eventArgs) => RefreshUI();
    }

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
    /// Update the UI with the current local inventory
    /// </summary>
    private void RefreshUI()
    {

    }

    /// <summary>
    /// Sync the local inventory used for UI to the truth source networked inventory list
    /// </summary>
    private void SyncLocalInventory()
    {
        for(int i = 0; i < Inventory.InventoryList.Count; i++)
            LocalInventoryList[i] = Inventory.InventoryList[i];
    }

    private void IntializeLocalInventory()
    {
        for(int i = 0; i < Inventory.InventorySize; i++)
            LocalInventoryList.Add(ItemRegistryEntry.InvalidId);
    }
}