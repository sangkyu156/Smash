using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SpeedPotion : InventoryItem
{
    private void OnEnable()
    {
        ItemSeting();
    }

    void ItemSeting()
    {
        ItemName = TextUtil.GetText("game:itme:speedpotion:name");
        ShortDescription = TextUtil.GetText("game:itme:consumable:short");

        if (grade == Grade.Normal)
            Description = TextUtil.GetText("game:itme:speedpotion:normal:long");
        else if (grade == Grade.Rare)
            Description = TextUtil.GetText("game:itme:speedpotion:rare:long");
        else if (grade == Grade.Unique)
            Description = TextUtil.GetText("game:itme:speedpotion:unique:long");
    }

    public override bool Use(string playerID)
    {
        base.Use(playerID);
        Debug.Log($"{ItemID}을 사용했습니다.");
        if (grade == Grade.Normal)
            MMGameEvent.Trigger("SpeedPotionNormal");
        else if (grade == Grade.Rare)
            MMGameEvent.Trigger("SpeedPotionRare");
        else if (grade == Grade.Unique)
            MMGameEvent.Trigger("SpeedPotionUnique");

        return true;
    }
}
