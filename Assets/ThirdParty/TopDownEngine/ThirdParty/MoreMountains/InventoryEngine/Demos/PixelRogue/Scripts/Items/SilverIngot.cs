using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class SilverIngot : InventoryItem
{
    private void OnEnable()
    {
        ItemSeting();
    }

    void ItemSeting()
    {
        grade = Grade.Rare;
        ItemName = TextUtil.GetText("game:itme:silveringot:name");
        ShortDescription = TextUtil.GetText("game:itme:selling:short");
        Description = TextUtil.GetText("game:itme:silveringot:long");
    }
}
