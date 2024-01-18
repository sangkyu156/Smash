using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BarrierItem", menuName = "MoreMountains/InventoryEngine/BarrierItem", order = 3)]
public class BarrierItem : InventoryItem
{
    public override bool Use(string playerID)
    {
        base.Use(playerID);
        Debug.LogFormat($"{ItemID}을 사용하였습니다.");
        return true;
    }
}
