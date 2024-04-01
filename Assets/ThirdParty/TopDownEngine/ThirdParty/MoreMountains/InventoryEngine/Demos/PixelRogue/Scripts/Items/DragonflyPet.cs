using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class DragonflyPet : InventoryItem
{
    private void OnEnable()
    {
        ItemSeting();
    }

    void ItemSeting()
    {
        grade = Grade.Normal;
        ItemName = TextUtil.GetText("game:itme:dragonflypet:name");
        ShortDescription = TextUtil.GetText("game:itme:consumable:short");
        Description = TextUtil.GetText("game:itme:dragonflypet:long");
    }

    public override bool Use(string playerID)
    {
        base.Use(playerID);
        Debug.Log($"{ItemID}을 사용했습니다.");
        MMGameEvent.Trigger("DragonflySummon");

        return true;
    }
}
