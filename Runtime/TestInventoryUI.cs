using UnityEngine;
using System.Collections.Generic;

public class TestInventoryUI : InventoryUI
{
    [SerializeField] private int hotbarSize = 5;

    public List<int> GetLocalInventory()
    {
        return LocalInventoryList;    
    } 

    protected override void SyncUI()
    {
        
    }
}