using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class BronzeIngot: InventoryItem
{
    private void OnEnable()
    {
        ItemSeting();
    }

    void ItemSeting()
    {
        grade = Grade.Normal;
        ItemName = TextUtil.GetText("game:itme:bronzeingot:name");
        ShortDescription = TextUtil.GetText("game:itme:selling:short");
        Description = TextUtil.GetText("game:itme:bronzeingot:long");
    }
}
