using UnityEngine;

[RequireComponent(typeof(Inventory))]
public class DisplayHeldItem : MonoBehaviour
{
    [SerializeField] private ItemTemplateDataBaseSO ItemTemplateDatabase;
    [SerializeField] private Transform HandTransform;

    private Inventory Inventory;

    private void Awake()
    {
        Inventory = GetComponent<Inventory>();
        Inventory.ActiveInventorySlot.OnValueChanged += OnActiveInventorySlotChanged;
    }

    private void OnActiveInventorySlotChanged(int previousValue, int newValue)
    {
        ItemRegistryEntry currentActiveItemEntry = ItemRegistry.Instance.GetEntry(Inventory.InventoryList[newValue]);

        if(currentActiveItemEntry.IsValid)
        {
            // TODO show no model in hand
            return;
        }

        ItemTemplateSO currentHeldItemTemplate = ItemTemplateDatabase.GetItemTemplate(currentActiveItemEntry.TemplateID);
        GameObject currentHeldItemModel = currentHeldItemTemplate.ModelPrefab;
        // TODO create held model and put it in hand
    }
}