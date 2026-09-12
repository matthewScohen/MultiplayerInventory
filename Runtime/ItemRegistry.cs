using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class ItemRegistry : NetworkBehaviour
{
    public static ItemRegistry Instance { get; private set; }

    public readonly NetworkList<ItemRegistryEntry> ItemRegistryEntries = new();
    private readonly SortedSet<int> AvailableInstanceIDs = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
            
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public int RegisterItem(int templateID, DynamicItemData dynamicItemData)
    {
        Debug.Assert(IsServer, "Only the server should register items.");
        if(!IsServer) return -1;

        int instanceID;

        if(AvailableInstanceIDs.Count == 0)
        {
            instanceID = ItemRegistryEntries.Count;
            ItemRegistryEntries.Add(new ItemRegistryEntry(templateID, instanceID, dynamicItemData));
            return instanceID;
        }

        instanceID = AvailableInstanceIDs.Min;
        AvailableInstanceIDs.Remove(instanceID);
        ItemRegistryEntries[instanceID] = new ItemRegistryEntry(templateID, instanceID, dynamicItemData);
        return instanceID;
    }

    public void DeregisterItem(int instanceID)
    {
        Debug.Assert(IsServer, "Only the server should deregister items.");
        if(!IsServer) return;

        ItemRegistryEntries[instanceID] = ItemRegistryEntry.Empty;
        AvailableInstanceIDs.Add(instanceID);
    }

    public ItemRegistryEntry GetEntry(int instanceID)
    {
        if(instanceID < 0 || instanceID >= ItemRegistryEntries.Count)
            return ItemRegistryEntry.Empty;
        
        return ItemRegistryEntries[instanceID];
    }

    public void SetDynamicData(int instanceID, DynamicItemData dynamicItemData)
    {
        Debug.Assert(IsServer, "Only the server should modify item data.");
        if(!IsServer) return;

        ItemRegistryEntry entry = ItemRegistryEntries[instanceID];
        entry.DynamicItemData = dynamicItemData;
        ItemRegistryEntries[instanceID] = entry;
    }

    public void SetOwned(int instanceID, bool owned)
    {
        Debug.Assert(IsServer, "Only the server should modify item ownership.");
        if(!IsServer) return;

        ItemRegistryEntry entry = ItemRegistryEntries[instanceID];
        entry.Owned = owned;
        ItemRegistryEntries[instanceID] = entry;
    }
}