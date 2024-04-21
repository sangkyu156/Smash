using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LockSkill : Skillbase, MMEventListener<MMGameEvent>
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
        maxLevel = 0;
        SkillNumber = -1;
        SetPrice();
        SkillLevel = 0;
        SkillName = "";
        ShortDescription = "";
        Description = "";
        skillLevel.text = SkillLevel.ToString();
    }

    void SetPrice()
    {
        price[0] = 0;
    }

    void SkillLevelRefresh()
    {
        SkillLevel = DataManager.Instance.datas.S_Health;
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
