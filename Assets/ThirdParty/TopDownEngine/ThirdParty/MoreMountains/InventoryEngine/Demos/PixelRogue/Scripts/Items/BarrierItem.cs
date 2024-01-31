using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

[CreateAssetMenu(fileName = "BarrierItem", menuName = "MoreMountains/InventoryEngine/BarrierItem", order = 3)]
public class BarrierItem : InventoryItem
{
    GameObject craftManual;

    public override bool Installation(string playerID)
    {
        base.Installation(playerID);
        Debug.Log($"{ItemID}�� ��ġ���Դϴ�.");
        Time.timeScale = 0;
        craftManual = Instantiate(Resources.Load<GameObject>("Prefabs/Object/CraftManual"));
        craftManual.GetComponent<CraftManual>().go_Preview = Resources.Load<GameObject>("Prefabs/Battlefield/Preview/Obstacles_Preview_Rock1");
        craftManual.GetComponent<CraftManual>().go_Prefab = Resources.Load<GameObject>("Prefabs/Battlefield/Obstacles/Obstacles_Rock1");
        craftManual.GetComponent<CraftManual>().inventory = GameObject.FindGameObjectWithTag("QuickSlots").GetComponent<Inventory>();

        return true;
    }

    public override bool InstallationCancel(string playerID)
    {
        base.InstallationCancel(playerID);
        Debug.Log($"{ItemID}�� ��ġ�� ����߽��ϴ�.");
        return true;
    }

    public override bool Use(string playerID)
    {
        base.Use(playerID);
        Debug.Log($"{ItemID}�� ����߽��ϴ�.");
        craftManual.GetComponent<CraftManual>().Build();

        return true;
    }
}
