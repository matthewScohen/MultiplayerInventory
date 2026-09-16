using UnityEngine;
using System.Collections.Generic;

public class TestLocalInventory : LocalInventory
{
    public List<int> GetLocalInventory()
    {
        return LocalInventoryList;    
    } 

    protected override void SyncUI()
    {
        
    }
}