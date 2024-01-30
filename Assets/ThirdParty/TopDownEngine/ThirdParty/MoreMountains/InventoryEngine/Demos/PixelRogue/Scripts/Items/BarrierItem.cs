using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

[CreateAssetMenu(fileName = "BarrierItem", menuName = "MoreMountains/InventoryEngine/BarrierItem", order = 3)]
public class BarrierItem : InventoryItem
{
    public override bool Use(string playerID)
    {
        base.Use(playerID);
        Debug.Log($"{ItemID}을 사용하였습니다.");
        Time.timeScale = 0;
        GameObject craftManual = Instantiate(Resources.Load<GameObject>("Prefabs/Object/CraftManual"));
        craftManual.GetComponent<CraftManual>().go_Preview = Resources.Load<GameObject>("Prefabs/Obstacles_Preview_Rock1");

        return true;
    }
}
