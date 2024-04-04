using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ES3AutoSaveMgr;

public class StaminaAmplificationSkill : Skillbase, MMEventListener<MMGameEvent>
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
        maxLevel = 10;
        SkillNumber = 2;
        SetPrice();
        SkillLevel = DataManager.Instance.datas.S_Stamina;
        SkillName = TextUtil.GetText("game:skill:staminaamplification:name");
        ShortDescription = TextUtil.GetText("game:skill:healthamplification:name");
        Description = TextUtil.GetText("game:skill:staminaamplification:long") + $"<color=#00FF00> + {SkillLevel * 5}</color>";
        skillLevel.text = SkillLevel.ToString();
    }

    void SetPrice()
    {
        price[0] = 100;
        price[1] = 200;
        price[2] = 400;
        price[3] = 700;
        price[4] = 1100;
        price[5] = 1600;
        price[6] = 2200;
        price[7] = 2900;
        price[8] = 3700;
        price[9] = 4600;
        price[10] = 5600;
    }

    void SkillLevelRefresh()
    {
        SkillLevel = DataManager.Instance.datas.S_Stamina;
        skillLevel.text = SkillLevel.ToString();
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
