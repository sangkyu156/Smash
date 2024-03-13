using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GoldIngot : InventoryItem
{
    private void OnEnable()
    {
        ItemSeting();
    }

    void ItemSeting()
    {
        grade = Grade.Unique;
        ItemName = TextUtil.GetText("game:itme:goldingot:name");
        ShortDescription = TextUtil.GetText("game:itme:selling:short");
        Description = TextUtil.GetText("game:itme:goldingot:long");
    }
}
