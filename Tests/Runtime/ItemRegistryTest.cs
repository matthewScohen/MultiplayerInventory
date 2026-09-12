using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Netcode;

public class ItemRegistryTest
{
    private GameObject NetworkManagerGameObject;
    private GameObject ItemRegistryGameObject;

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
    }

    [UnityTest]
    public IEnumerator Test_InstanceID()
    {
        yield return new WaitForSeconds(0.1f);
        
        ItemRegistryEntry entry = new(1, 1, new DynamicItemData(10, 100));
        int instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
        Assert.AreEqual(instanceID, 0);

        instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
        Assert.AreEqual(instanceID, 1);

        instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
        Assert.AreEqual(instanceID, 2);

        ItemRegistry.Instance.DeregisterItem(1);
        instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
        Assert.AreEqual(instanceID, 1);
    }

    [UnityTest]
    public IEnumerator Test_ManyInstanceID()
    {
        yield return new WaitForSeconds(0.1f);
        
        int instanceID;
        ItemRegistryEntry entry;

        for(int i = 0; i < 1000; i++)
        {
            entry = new(1, 1, new DynamicItemData(10, 100));
            instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
            Assert.AreEqual(instanceID, i);
        }

        for(int i = 200; i < 300; i+= 3)
        {
            ItemRegistry.Instance.DeregisterItem(i);
        }

        for(int i = 0; i*3 < 100; i++)
        {
            entry = new(1, 1, new DynamicItemData(10, 100));
            instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
            Assert.AreEqual(instanceID, 200 + (i * 3));
        }

        entry = new(1, 1, new DynamicItemData(10, 100));
        instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
        Assert.AreEqual(instanceID, 1000);
    }

    [UnityTest]
    public IEnumerator Test_GetEntry()
    {
        yield return new WaitForSeconds(0.1f);
        
        ItemRegistryEntry entry = new(1, 1, new DynamicItemData(10, 100));
        int instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
        Assert.AreEqual(instanceID, 0);

        ItemRegistryEntry retrievedEntry = ItemRegistry.Instance.GetEntry(instanceID);
        Assert.AreEqual(retrievedEntry.TemplateID, entry.TemplateID);
        Assert.AreEqual(retrievedEntry.InstanceID, instanceID);
        Assert.AreEqual(retrievedEntry.DynamicItemData.CurrentAmmo, entry.DynamicItemData.CurrentAmmo);
        Assert.AreEqual(retrievedEntry.DynamicItemData.Durability, entry.DynamicItemData.Durability);

        entry = new(5, 1, new DynamicItemData(27, 43));
        instanceID = ItemRegistry.Instance.RegisterItem(entry.TemplateID, entry.DynamicItemData);
        retrievedEntry = ItemRegistry.Instance.GetEntry(instanceID);
        Assert.AreEqual(retrievedEntry.TemplateID, entry.TemplateID);
        Assert.AreEqual(retrievedEntry.InstanceID, instanceID);
        Assert.AreEqual(retrievedEntry.DynamicItemData.CurrentAmmo, entry.DynamicItemData.CurrentAmmo);
        Assert.AreEqual(retrievedEntry.DynamicItemData.Durability, entry.DynamicItemData.Durability);
    }

    [UnityTearDown]
    public IEnumerator Teardown()
    {
        if (NetworkManager.Singleton != null) NetworkManager.Singleton.Shutdown();
        if (NetworkManagerGameObject != null) Object.Destroy(NetworkManagerGameObject);
        if (ItemRegistryGameObject != null) Object.Destroy(ItemRegistryGameObject);
        
        yield return null;
    }
}
