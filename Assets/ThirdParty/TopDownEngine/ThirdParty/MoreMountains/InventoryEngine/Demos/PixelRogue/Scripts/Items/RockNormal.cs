using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class RockNormal : InventoryItem
{
    GameObject craftManual;

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

    public override bool Installation(string playerID)
    {
        base.Installation(playerID);
        Debug.Log($"{ItemID}을 설치중입니다.");
        //MMGameEvent.Trigger("Installing");//이벤트 뿌리기 (구독중인 오브젝트는 자신의 스퀘어콜라이더 이즈트리거 켜라)
        Time.timeScale = 0;
        craftManual = Instantiate(Resources.Load<GameObject>("Prefabs/Object/CraftManual"));
        craftManual.GetComponent<CraftManual>().go_Preview = Resources.Load<GameObject>("Prefabs/Battlefield/Preview/RockPreview");
        craftManual.GetComponent<CraftManual>().go_Prefab = Resources.Load<GameObject>("Prefabs/Battlefield/Obstacles/RockNormal");
        craftManual.GetComponent<CraftManual>().inventory = GameObject.FindGameObjectWithTag("QuickSlots").GetComponent<Inventory>();

        return true;
    }

    public override bool InstallationCancel(string playerID)
    {
        base.InstallationCancel(playerID);
        Debug.Log($"{ItemID}을 설치를 취소했습니다.");
        //설치 취소시 자신의 콜라이더 킨사람 끄라고 이벤트 뿌리기
        craftManual.GetComponent<CraftManual>().BuildCancel();
        return true;
    }

    public override bool Use(string playerID)
    {
        base.Use(playerID);
        Debug.Log($"{ItemID}을 사용했습니다.");
        //설치 완료시 자신의 콜라이더 킨사람 끄라고 이벤트 뿌리기 (완료는 빌드()함수에서 MMGameEvent.Trigger("Installed"); 하고있음)
        craftManual.GetComponent<CraftManual>().Build();

        return true;
    }
}
