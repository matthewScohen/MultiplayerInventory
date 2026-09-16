using System;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class CreateHeldItem : MonoBehaviour
{
    [SerializeField] private ItemTemplateDataBaseSO ItemTemplateDatabase;
    [SerializeField] private Transform HandTransform;

    public GameObject HeldGameObject;

    private Inventory Inventory;

    private void Awake()
    {
        Inventory = GetComponent<Inventory>();
        Inventory.ActiveInventorySlot.OnValueChanged += (preivousValue, newValue) => OnInventorySlotChanged();
        Inventory.InventoryList.OnListChanged += OnInventoryChanged;
        Inventory.itemPickedUp += OnInventorySlotChanged;
    }

    private void OnInventorySlotChanged()
    {
        Destroy(HeldGameObject);
        HeldGameObject = null;

        ItemRegistryEntry currentActiveItemEntry = ItemRegistry.Instance.GetEntry(Inventory.InventoryList[Inventory.ActiveInventorySlot.Value]);
        if(!currentActiveItemEntry.IsValid)
            return;

        ItemTemplateSO currentHeldItemTemplate = ItemTemplateDatabase.GetItemTemplate(currentActiveItemEntry.TemplateID);
        HeldGameObject = Instantiate(currentHeldItemTemplate.ItemPrefab, HandTransform);
    }

    private void OnInventoryChanged(NetworkListEvent<int> changeEvent)
    {
        if(changeEvent.Index == Inventory.ActiveInventorySlot.Value)
            OnInventorySlotChanged();
    }
}