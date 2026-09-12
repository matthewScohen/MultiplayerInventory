using UnityEngine;
using Unity.Netcode;

public class WorldItem : NetworkBehaviour
{
    [SerializeField] private int TemplateID;
    [SerializeField] private DynamicItemData DynamicItemData;

    public int InstanceID;

    public override void OnNetworkSpawn()
    {
        if(!IsServer) return;

        InstanceID = ItemRegistry.Instance.RegisterItem(TemplateID, DynamicItemData);
    }

    public void PickUp(Inventory inventory)
    {
        if(!IsServer) return;

        if(inventory.TryPickUpItem(InstanceID))
            NetworkObject.Despawn();
    }
}