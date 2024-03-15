using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class HealRockNormal : InventoryItem
{
    GameObject craftManual;

    private void OnEnable()
    {
        ItemSeting();
    }

    void ItemSeting()
    {
        grade = Grade.Normal;
        ItemName = TextUtil.GetText("game:itme:healrock:name");
        ShortDescription = TextUtil.GetText("game:itme:rock:short");
        Description = TextUtil.GetText("game:itme:healrocknormal:long");
    }

    public override bool Installation(string playerID)
    {
        base.Installation(playerID);
        Debug.Log($"{ItemID}을 설치중입니다.");
        Time.timeScale = 0;
        craftManual = Instantiate(Resources.Load<GameObject>("Prefabs/Object/CraftManual"));
        craftManual.GetComponent<CraftManual>().go_Preview = Resources.Load<GameObject>("Prefabs/Battlefield/Preview/HealRockPreview");
        craftManual.GetComponent<CraftManual>().go_Prefab = Resources.Load<GameObject>("Prefabs/Battlefield/Obstacles/HealRockNormal");
        craftManual.GetComponent<CraftManual>().inventory = GameObject.FindGameObjectWithTag("QuickSlots").GetComponent<Inventory>();

        return true;
    }

    public override bool InstallationCancel(string playerID)
    {
        base.InstallationCancel(playerID);
        Debug.Log($"{ItemID}을 설치를 취소했습니다.");
        craftManual.GetComponent<CraftManual>().BuildCancel();
        return true;
    }

    public override bool Use(string playerID)
    {
        base.Use(playerID);
        Debug.Log($"{ItemID}을 사용했습니다.");
        craftManual.GetComponent<CraftManual>().Build();

        return true;
    }
}
