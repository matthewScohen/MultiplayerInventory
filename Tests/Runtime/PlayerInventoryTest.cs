using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Netcode;
using System.Collections.ObjectModel;

public class PlayerInventoryTest
{
    private GameObject NetworkManagerGameObject;
    private GameObject ItemRegistryGameObject;
    private GameObject PlayerInventoryGameObject;

    private Inventory inventory;
    private InventoryUI inventoryUI;

    private const int DefaultBackpackSize = 20;
    private const int DefaultHotbarSize = 5;

    [UnitySetUp]
    public IEnumerator Setup()
    {
        NetworkManagerGameObject = new GameObject("NetworkManager");
        NetworkManager networkManager = NetworkManagerGameObject.AddComponent<NetworkManager>();
        networkManager.NetworkConfig = new NetworkConfig { 
            NetworkTransport = NetworkManagerGameObject.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>() 
        };
        networkManager.StartHost();
        yield return null;

        ItemRegistryGameObject = new GameObject("ItemRegistry");
        NetworkObject networkObject = ItemRegistryGameObject.AddComponent<NetworkObject>();
        ItemRegistryGameObject.AddComponent<ItemRegistry>();

        System.Reflection.FieldInfo fieldInfo = typeof(NetworkObject).GetField("GlobalObjectIdHash", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        fieldInfo.SetValue(networkObject, (uint)ItemRegistryGameObject.name.GetHashCode());

        networkObject.Spawn();
        yield return null;

        PlayerInventoryGameObject = new GameObject("Inventory");
        NetworkObject playerInventoryNetworkObject = PlayerInventoryGameObject.AddComponent<NetworkObject>();
        inventory = PlayerInventoryGameObject.AddComponent<Inventory>();
        inventoryUI = PlayerInventoryGameObject.AddComponent<InventoryUI>();
        fieldInfo.SetValue(playerInventoryNetworkObject, (uint)PlayerInventoryGameObject.name.GetHashCode());
        playerInventoryNetworkObject.Spawn();
    }

    [UnityTest]
    public IEnumerator TestPickUpItem()
    {
        int instanceID = ItemRegistry.Instance.RegisterItem(1, new DynamicItemData(10, 100));
        inventory.TryPickUpItem(instanceID);

        Assert.AreEqual(inventory.InventoryList[0], instanceID);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestPickUpManyItems()
    {
        const int numItemsToTest = 10;
        for(int i = 0; i < numItemsToTest; i++)
            ItemRegistry.Instance.RegisterItem(1, new DynamicItemData(10, 100));
        
        for(int i = 0; i < numItemsToTest; i++)
            inventory.TryPickUpItem(i);

        Assert.AreEqual(inventory.InventoryList[0], 0);
        Assert.AreEqual(inventory.InventoryList[1], 1);
        Assert.AreEqual(inventory.InventoryList[2], 2);
        Assert.AreEqual(inventory.InventoryList[3], 3);
        Assert.AreEqual(inventory.InventoryList[4], 4);
        Assert.AreEqual(inventory.InventoryList[5], 5);
        Assert.AreEqual(inventory.InventoryList[6], 6);
        Assert.AreEqual(inventory.InventoryList[7], 7);
        Assert.AreEqual(inventory.InventoryList[8], 8);
        Assert.AreEqual(inventory.InventoryList[9], 9);
        Assert.AreEqual(inventory.InventoryList[10], -1);
        Assert.AreEqual(inventory.InventoryList[11], -1);
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestPickUpAndRemove()
    {
        // Create items
        const int numItemsToTest = 10;
        CreateItems(numItemsToTest);

        // Check items are all unowned
        for(int i = 0; i < numItemsToTest; i++)
            Assert.IsFalse(ItemRegistry.Instance.GetEntry(i).Owned);

        // Pick up all items
        for(int i = 0; i < numItemsToTest; i++)
            inventory.TryPickUpItem(i);
        
        // Check items are all owned
        for(int i = 0; i < numItemsToTest; i++)
            Assert.IsTrue(ItemRegistry.Instance.GetEntry(i).Owned);

        // Check inventory has correct number of items
        Assert.AreEqual(inventory.ItemCount, numItemsToTest);

        // Drop items in hotbar and check ownership
        for(int i = 0; i < DefaultHotbarSize; i++)
        {
            inventory.RemoveItem(i);
            Assert.AreEqual(inventory.ItemCount, numItemsToTest - i - 1);
            Assert.IsFalse(ItemRegistry.Instance.GetEntry(i).Owned);
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator TestPickUpInvalidItem()
    {
        int instanceID = ItemRegistry.Instance.RegisterItem(1, new DynamicItemData(10, 100));
        Assert.IsFalse(inventory.TryPickUpItem(instanceID + 1));
        yield return null;
    }

    [UnityTest]
    public IEnumerator TestInventoryUI()
    {
        // Get local inventory list from inventoryUI via reflection since its private
        var field = typeof(InventoryUI).GetField(
        "LocalInventoryList",
        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var localUIList = (ObservableCollection<int>)field.GetValue(inventoryUI);

        const int numberItemsToTest = 10;
        CreateItems(numberItemsToTest);

        // Fill inventory and make sure local UI inventory syncs
        for(int i = 0; i < numberItemsToTest; i++)
            Assert.IsTrue(inventory.TryPickUpItem(i));

        for(int i = 0; i < numberItemsToTest; i++)
            Assert.AreEqual(localUIList[i], inventory.InventoryList[i]);

        // Swap items in UI and make sure it syncs to network inventory
        inventoryUI.SwapItems(0, 1);
        Assert.AreEqual(localUIList[0], 1);
        Assert.AreEqual(localUIList[1], 0);
        Assert.AreEqual(inventory.InventoryList[0], localUIList[0]);
        Assert.AreEqual(inventory.InventoryList[1], localUIList[1]);

        // Remove item from network inventory and make sure it syncs to UI inventory
        inventory.RemoveItem(5);
        Assert.AreEqual(inventory.InventoryList[5], ItemRegistryEntry.InvalidId);
        Assert.AreEqual(localUIList[5], ItemRegistryEntry.InvalidId);

        // Swap empty item in inventory UI and make sure it syncs
        inventoryUI.SwapItems(3, 5);
        Assert.AreEqual(localUIList[3], -1);
        Assert.AreEqual(localUIList[5], 3);
        Assert.AreEqual(inventory.InventoryList[3], localUIList[3]);
        Assert.AreEqual(inventory.InventoryList[5], localUIList[5]);

        yield return null;
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        if (NetworkManagerGameObject != null) Object.Destroy(NetworkManagerGameObject);
        if (ItemRegistryGameObject != null) Object.Destroy(ItemRegistryGameObject);
        if (PlayerInventoryGameObject != null) Object.Destroy(PlayerInventoryGameObject);
        
        yield return null;
    }

#region Helpers
    private void CreateItems(int numberItemsToCreate, int templateID = 1)
    {
        for(int i = 0; i < numberItemsToCreate; i++)
            ItemRegistry.Instance.RegisterItem(templateID, new DynamicItemData(10, 100));
    }
#endregion
}