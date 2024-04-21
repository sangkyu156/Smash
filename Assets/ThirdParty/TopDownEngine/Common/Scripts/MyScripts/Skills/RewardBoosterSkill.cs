using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RewardBoosterSkill : Skillbase, MMEventListener<MMGameEvent>
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
        maxLevel = 20;
        SkillNumber = 1;
        SetPrice();
        SkillLevel = DataManager.Instance.datas.S_ClearReward;
        SkillName = TextUtil.GetText("game:skill:rewardbooster:name");
        ShortDescription = TextUtil.GetText("game:skill:healthamplification:name");
        Description = TextUtil.GetText("game:skill:rewardbooster:long") + $"<color=#00FF00> + {SkillLevel * 3}%</color>";
        skillLevel.text = SkillLevel.ToString();
    }

    void SetPrice()
    {
        price[0] = 100;
        price[1] = 250;
        price[2] = 550;
        price[3] = 1000;
        price[4] = 1600;
    }

    void SkillLevelRefresh()
    {
        SkillLevel = DataManager.Instance.datas.S_ClearReward;
        skillLevel.text = SkillLevel.ToString();
        if (SkillLevel == maxLevel)
            skillLevel.color = Color.red;
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if (gameEvent.EventName == "SkillBuy")
        {
            SkillLevelRefresh();
            SkillSeting();
        }

        if (gameEvent.EventName == "SkillPopupOpens")
        {
            SkillLevelRefresh();
            SkillSeting();
        }
    }
}
