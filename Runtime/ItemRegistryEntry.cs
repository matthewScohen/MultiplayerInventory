using UnityEngine;
using Unity.Netcode;
using System;

[System.Serializable]
public struct DynamicItemData : INetworkSerializable
{
    public int CurrentAmmo;
    public int Durability;

    public DynamicItemData(int currentAmmo = -1, int durability = -1)
    {
        CurrentAmmo = currentAmmo;
        Durability = durability;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref CurrentAmmo);
        serializer.SerializeValue(ref Durability);
    }
}

public struct ItemRegistryEntry : INetworkSerializable, IEquatable<ItemRegistryEntry>
{
    public const int InvalidId = -1;

    public int TemplateID;
    public int InstanceID;
    public DynamicItemData DynamicItemData;
    public bool Owned;

    public readonly bool IsValid => TemplateID != InvalidId && InstanceID != InvalidId;

    public static ItemRegistryEntry Empty => new()
    {
        TemplateID = InvalidId,
        InstanceID = InvalidId,
        DynamicItemData = new DynamicItemData(),
        Owned = false
    };

    public ItemRegistryEntry(int templateID, int instanceID, DynamicItemData dynamicItemData)
    {
        TemplateID = templateID;
        InstanceID = instanceID;
        DynamicItemData = dynamicItemData;
        Owned = false;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref TemplateID);
        serializer.SerializeValue(ref InstanceID);
        serializer.SerializeValue(ref DynamicItemData);
        serializer.SerializeValue(ref Owned);
    }

    public readonly bool Equals(ItemRegistryEntry other)
    {
        return TemplateID == other.TemplateID &&
               InstanceID == other.InstanceID &&
               Owned == other.Owned &&
               DynamicItemData.CurrentAmmo == other.DynamicItemData.CurrentAmmo &&
               Mathf.Approximately(DynamicItemData.Durability, other.DynamicItemData.Durability);
    }

    public override readonly bool Equals(object obj) => obj is ItemRegistryEntry other && Equals(other);
    public override readonly int GetHashCode() => HashCode.Combine(TemplateID, InstanceID, DynamicItemData, DynamicItemData);
}