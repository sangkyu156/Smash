using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class HealthPotion : InventoryItem
{
    private void OnEnable()
    {
        ItemSeting();
    }

    void ItemSeting()
    {
        ItemName = TextUtil.GetText("game:itme:healthpotion:name");
        ShortDescription = TextUtil.GetText("game:itme:consumable:short");

        if(grade == Grade.Normal)
            Description = TextUtil.GetText("game:itme:healthpotion:normal:long");
        else if(grade == Grade.Rare)
            Description = TextUtil.GetText("game:itme:healthpotion:rare:long");
        else if(grade == Grade.Unique)
            Description = TextUtil.GetText("game:itme:healthpotion:unique:long");
    }

    public override bool Use(string playerID)
    {
        base.Use(playerID);
        Debug.Log($"{ItemID}을 사용했습니다.");
        if(grade == Grade.Normal)
            MMGameEvent.Trigger("HealthPotionNormal");
        else if(grade == Grade.Rare)
            MMGameEvent.Trigger("HealthPotionRare");
        else if(grade == Grade.Unique)
            MMGameEvent.Trigger("HealthPotionUnique");

        return true;
    }
}
