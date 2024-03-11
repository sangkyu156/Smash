using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class RockNormal : InventoryItem
{
    private void OnEnable()
    {
        ItemSeting();
    }

    void ItemSeting()
    {
        grade = Grade.Normal;
        ItemName = TextUtil.GetText("game:itme:rock:name");
        ShortDescription = TextUtil.GetText("game:itme:rock:short");
        Description = TextUtil.GetText("game:itme:rocknormal:long");

    }
}
