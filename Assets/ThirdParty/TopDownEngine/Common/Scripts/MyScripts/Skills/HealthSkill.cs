using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HealthSkill : Skillbase, MMEventListener<MMGameEvent>
{
    public TextMeshProUGUI skillLevel;

    void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
        SkillSeting();
    }
    void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

    void SkillSeting()
    {
        maxLevel = 5;
        SkillNumber = 0;
        SetPrice();
        SkillLevel = DataManager.Instance.datas.S_Health;
        SkillName = TextUtil.GetText("game:skill:healthamplification:name");
        ShortDescription = TextUtil.GetText("game:skill:healthamplification:name");
        Description = TextUtil.GetText("game:skill:healthamplification:long") + $"<color=#00FF00> + {SkillLevel * 10}</color>";
        skillLevel.text = SkillLevel.ToString();
    }

    void SetPrice()
    {
        price[0] = 100;
        price[1] = 200;
        price[2] = 400;
        price[3] = 700;
        price[4] = 1100;
    }

    void SkillLevelRefresh()
    {
        SkillLevel = DataManager.Instance.datas.S_Health;
        skillLevel.text = SkillLevel.ToString();
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "SkillBuy")
        {
            SkillLevelRefresh();
            SkillSeting();
        }

        if(gameEvent.EventName == "SkillPopupOpens")
        {
            SkillLevelRefresh();
            SkillSeting();
        }
    }
}
