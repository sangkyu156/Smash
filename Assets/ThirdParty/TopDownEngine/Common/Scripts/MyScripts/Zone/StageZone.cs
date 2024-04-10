using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageZone : MonoBehaviour
{
    string curStage = string.Empty;
    public bool isChallenge = false;
    public GameObject possibleEffects;
    public GameObject impossibleEffect;


    void Start()
    {
        curStage = gameObject.name;

        ChallengeCheck();//게임오브젝트 이름으로 도전가능한지 체크

        //도전 여부에 따라 이펙트 다르게 설정
        if(isChallenge == true) 
        {
            possibleEffects.SetActive(true);
            impossibleEffect.SetActive(false);
        }
        else
        {
            possibleEffects.SetActive(false);
            impossibleEffect.SetActive(true);
        }

    }

    //스테이지 도전가능한지 결정하는 함수
    void ChallengeCheck()
    {
        switch (curStage)
        {
            case "01": isChallenge = true; break;
            case "02": isChallenge = DataManager.Instance.datas.stage1; break;
            case "03": isChallenge = DataManager.Instance.datas.stage2; break;
            case "04": isChallenge = DataManager.Instance.datas.stage3; break;
            case "05": isChallenge = DataManager.Instance.datas.stage4; break;
            case "06": isChallenge = DataManager.Instance.datas.stage5; break;
            case "07": isChallenge = DataManager.Instance.datas.stage6; break;
            case "08": isChallenge = DataManager.Instance.datas.stage7; break;
            case "09": isChallenge = DataManager.Instance.datas.stage8; break;
            case "10": isChallenge = DataManager.Instance.datas.stage9; break;
            case "11": isChallenge = DataManager.Instance.datas.stage10; break;
            case "12": isChallenge = DataManager.Instance.datas.stage11; break;
            case "13": isChallenge = DataManager.Instance.datas.stage12; break;
            case "14": isChallenge = DataManager.Instance.datas.stage13; break;
            case "15": isChallenge = DataManager.Instance.datas.stage14; break;
            case "16": isChallenge = DataManager.Instance.datas.stage15; break;
            case "17": isChallenge = DataManager.Instance.datas.stage16; break;
            case "18": isChallenge = DataManager.Instance.datas.stage17; break;
            case "19": isChallenge = DataManager.Instance.datas.stage18; break;
            case "20": isChallenge = DataManager.Instance.datas.stage19; break;
            case "21": isChallenge = DataManager.Instance.datas.stage20; break;
            case "22": isChallenge = DataManager.Instance.datas.stage21; break;
            case "23": isChallenge = DataManager.Instance.datas.stage22; break;
            case "24": isChallenge = DataManager.Instance.datas.stage23; break;
            case "25": isChallenge = DataManager.Instance.datas.stage24; break;
            case "26": isChallenge = DataManager.Instance.datas.stage25; break;
            case "27": isChallenge = DataManager.Instance.datas.stage26; break;
            case "28": isChallenge = DataManager.Instance.datas.stage27; break;
            case "29": isChallenge = DataManager.Instance.datas.stage28; break;
            case "30": isChallenge = DataManager.Instance.datas.stage29; break;
            case "31": isChallenge = DataManager.Instance.datas.stage30; break;
            case "32": isChallenge = DataManager.Instance.datas.stage31; break;
            case "33": isChallenge = DataManager.Instance.datas.stage32; break;
            case "34": isChallenge = DataManager.Instance.datas.stage33; break;
            case "35": isChallenge = DataManager.Instance.datas.stage34; break;
            case "36": isChallenge = DataManager.Instance.datas.stage35; break;
            case "37": isChallenge = DataManager.Instance.datas.stage36; break;
            case "38": isChallenge = DataManager.Instance.datas.stage37; break;
            case "39": isChallenge = DataManager.Instance.datas.stage38; break;
            case "40": isChallenge = DataManager.Instance.datas.stage39; break;
            case "41": isChallenge = DataManager.Instance.datas.stage40; break;
            case "42": isChallenge = DataManager.Instance.datas.stage41; break;
            case "43": isChallenge = DataManager.Instance.datas.stage42; break;
            case "44": isChallenge = DataManager.Instance.datas.stage43; break;
            case "45": isChallenge = DataManager.Instance.datas.stage44; break;
            case "46": isChallenge = DataManager.Instance.datas.stage45; break;
            case "47": isChallenge = DataManager.Instance.datas.stage46; break;
            case "48": isChallenge = DataManager.Instance.datas.stage47; break;
            case "49": isChallenge = DataManager.Instance.datas.stage48; break;
            case "50": isChallenge = DataManager.Instance.datas.stage49; break;
            default: isChallenge = false; break;
        }
    }
}
