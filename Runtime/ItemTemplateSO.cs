using UnityEngine;
using Unity.Netcode;

public abstract class ItemTemplateSO : ScriptableObject
{
    public int TemplateID;
    public string Name;
    public Sprite Icon;
    public GameObject ModelPrefab;

    /// <summary>
    /// Execute the server-side logic for using this item.
    /// </summary>
    /// <param name="playerOwner"></param>
    /// <param name="itemInstanceID"></param>
    [Rpc(SendTo.Server)] public abstract void ServerUseRpc(NetworkObject playerOwner, int itemInstanceID);

    /// <summary>
    /// Execute the client-side logic for using this item. This is typically used for effects like particles and sounds.
    /// </summary>
    /// <param name="playerOwner"></param>
    /// <param name="itemInstanceID"></param>
    [Rpc(SendTo.Everyone)] public abstract void ClientUseRpc(NetworkObject playerOwner, int itemInstanceID);

    /// <summary>
    /// Predict the dynamic data of this item after it is used. This is used client side for updating UI responsively.
    /// </summary>
    /// <param name="currentData"></param>
    public abstract DynamicItemData PredictDynamicDataAfterUse(DynamicItemData currentData);
}