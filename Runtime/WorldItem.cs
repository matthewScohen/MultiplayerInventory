using UnityEngine;
using Unity.Netcode;

public class WorldItem : NetworkBehaviour
{
    [SerializeField] private int TemplateID;
    [SerializeField] private DynamicItemData DynamicItemData;

    [HideInInspector] public int InstanceID;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;

        if (ItemRegistry.Instance != null && ItemRegistry.Instance.IsReady)
            RegisterSelf();
        else
            ItemRegistry.Instance.OnRegistryReady += RegisterSelf;
    }

    public void PickUp(Inventory inventory)
    {
        if(!IsServer) return;

        if(inventory.TryPickUpItem(InstanceID))
        {
            NetworkObject.Despawn(false);
            Destroy(gameObject);
        }
    }

    private void RegisterSelf()
    {
        InstanceID = ItemRegistry.Instance.RegisterItem(TemplateID, DynamicItemData);
    }
}