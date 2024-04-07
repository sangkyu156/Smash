using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rewards : MonoBehaviour
{
    //여기서 이놈이 해야할것
    //0. 묻따 RewardGold 프리펩을 자식으로 넣어준다.
    //3. 리워드추가보상 스킬이 있는지 확인하고 있으면 리워드추가보상 프리펩을 자식으로 소환한다.
    //1. 플레이어가 방금 클리어한 스테이지를 처음 클리어헀는지 확인한다.
    //2. 처음 클리어 했으면 알맞는 리워드프리펩을 자식으로 소환한다.

    private void Start()
    {
        CreateManager.Instantiate("Rewards/RewardGold", this.transform);

        if (DataManager.Instance.datas.S_ClearReward > 0)
            CreateManager.Instantiate("Rewards/RewardBonusGold", this.transform);

        CreateManager.Instantiate("Rewards/DragonflyAvailable", this.transform);//테스트
        int curStage = (int)GameManager.Instance.stage;
        if(isFirstClear(curStage) == false)//처음 클리어하는 스테이지면 false반환
        {
            StageChangeToTrue(curStage);

            switch (curStage)
            {
                case 0: CreateManager.Instantiate("Rewards/DragonflyAvailable", this.transform); break; //테스트
                case 1: CreateManager.Instantiate("Rewards/DragonflyAvailable", this.transform);break; //테스트
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25: CreateManager.Instantiate("Rewards/DragonflyAvailable", this.transform); break; //테스트
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                case 34:
                case 35:
                case 36:
                case 37:
                case 38:
                case 39:
                case 40:
                case 41:
                case 42:
                case 43:
                case 44:
                case 45:
                case 46:
                case 47:
                case 48:
                case 49:
                case 50: CreateManager.Instantiate("Rewards/DragonflyAvailable", this.transform); break; //테스트
            }
        }
    }

    //처음 클리어하는 스테이지면 false반환
    bool isFirstClear(int _curStage)
    {
        bool isFirst = false;

        switch (_curStage)
        {
            case 1:  isFirst = DataManager.Instance.datas.stage1; break;
            case 2:  isFirst = DataManager.Instance.datas.stage2; break;
            case 3:  isFirst = DataManager.Instance.datas.stage3; break;
            case 4:  isFirst = DataManager.Instance.datas.stage4; break;
            case 5:  isFirst = DataManager.Instance.datas.stage5; break;
            case 6:  isFirst = DataManager.Instance.datas.stage6; break;
            case 7:  isFirst = DataManager.Instance.datas.stage7; break;
            case 8:  isFirst = DataManager.Instance.datas.stage8; break;
            case 9:  isFirst = DataManager.Instance.datas.stage9; break;
            case 10: isFirst = DataManager.Instance.datas.stage10; break;
            case 11: isFirst = DataManager.Instance.datas.stage11; break;
            case 12: isFirst = DataManager.Instance.datas.stage12; break;
            case 13: isFirst = DataManager.Instance.datas.stage13; break;
            case 14: isFirst = DataManager.Instance.datas.stage14; break;
            case 15: isFirst = DataManager.Instance.datas.stage15; break;
            case 16: isFirst = DataManager.Instance.datas.stage16; break;
            case 17: isFirst = DataManager.Instance.datas.stage17; break;
            case 18: isFirst = DataManager.Instance.datas.stage18; break;
            case 19: isFirst = DataManager.Instance.datas.stage19; break;
            case 20: isFirst = DataManager.Instance.datas.stage20; break;
            case 21: isFirst = DataManager.Instance.datas.stage21; break;
            case 22: isFirst = DataManager.Instance.datas.stage22; break;
            case 23: isFirst = DataManager.Instance.datas.stage23; break;
            case 24: isFirst = DataManager.Instance.datas.stage24; break;
            case 25: isFirst = DataManager.Instance.datas.stage25; break;
            case 26: isFirst = DataManager.Instance.datas.stage26; break;
            case 27: isFirst = DataManager.Instance.datas.stage27; break;
            case 28: isFirst = DataManager.Instance.datas.stage28; break;
            case 29: isFirst = DataManager.Instance.datas.stage29; break;
            case 30: isFirst = DataManager.Instance.datas.stage30; break;
            case 31: isFirst = DataManager.Instance.datas.stage31; break;
            case 32: isFirst = DataManager.Instance.datas.stage32; break;
            case 33: isFirst = DataManager.Instance.datas.stage33; break;
            case 34: isFirst = DataManager.Instance.datas.stage34; break;
            case 35: isFirst = DataManager.Instance.datas.stage35; break;
            case 36: isFirst = DataManager.Instance.datas.stage36; break;
            case 37: isFirst = DataManager.Instance.datas.stage37; break;
            case 38: isFirst = DataManager.Instance.datas.stage38; break;
            case 39: isFirst = DataManager.Instance.datas.stage39; break;
            case 40: isFirst = DataManager.Instance.datas.stage40; break;
            case 41: isFirst = DataManager.Instance.datas.stage41; break;
            case 42: isFirst = DataManager.Instance.datas.stage42; break;
            case 43: isFirst = DataManager.Instance.datas.stage43; break;
            case 44: isFirst = DataManager.Instance.datas.stage44; break;
            case 45: isFirst = DataManager.Instance.datas.stage45; break;
            case 46: isFirst = DataManager.Instance.datas.stage46; break;
            case 47: isFirst = DataManager.Instance.datas.stage47; break;
            case 48: isFirst = DataManager.Instance.datas.stage48; break;
            case 49: isFirst = DataManager.Instance.datas.stage49; break;
            case 50: isFirst = DataManager.Instance.datas.stage50; break;
        }

        return isFirst;
    }

    //처음 클리어하는 스테이지 보상 받았으니 true로 바꾸고 저장하는 함수
    void StageChangeToTrue(int _curStage)
    {
        switch (_curStage)
        {
            case 1:  DataManager.Instance.datas.stage1 = true; break;
            case 2:  DataManager.Instance.datas.stage2 = true; break;
            case 3:  DataManager.Instance.datas.stage3 = true; break;
            case 4:  DataManager.Instance.datas.stage4 = true; break;
            case 5:  DataManager.Instance.datas.stage5 = true; break;
            case 6:  DataManager.Instance.datas.stage6 = true; break;
            case 7:  DataManager.Instance.datas.stage7 = true; break;
            case 8:  DataManager.Instance.datas.stage8 = true; break;
            case 9:  DataManager.Instance.datas.stage9 = true; break;
            case 10: DataManager.Instance.datas.stage10 = true; break;
            case 11: DataManager.Instance.datas.stage11 = true; break;
            case 12: DataManager.Instance.datas.stage12 = true; break;
            case 13: DataManager.Instance.datas.stage13 = true; break;
            case 14: DataManager.Instance.datas.stage14 = true; break;
            case 15: DataManager.Instance.datas.stage15 = true; break;
            case 16: DataManager.Instance.datas.stage16 = true; break;
            case 17: DataManager.Instance.datas.stage17 = true; break;
            case 18: DataManager.Instance.datas.stage18 = true; break;
            case 19: DataManager.Instance.datas.stage19 = true; break;
            case 20: DataManager.Instance.datas.stage20 = true; break;
            case 21: DataManager.Instance.datas.stage21 = true; break;
            case 22: DataManager.Instance.datas.stage22 = true; break;
            case 23: DataManager.Instance.datas.stage23 = true; break;
            case 24: DataManager.Instance.datas.stage24 = true; break;
            case 25: DataManager.Instance.datas.stage25 = true; break;
            case 26: DataManager.Instance.datas.stage26 = true; break;
            case 27: DataManager.Instance.datas.stage27 = true; break;
            case 28: DataManager.Instance.datas.stage28 = true; break;
            case 29: DataManager.Instance.datas.stage29 = true; break;
            case 30: DataManager.Instance.datas.stage30 = true; break;
            case 31: DataManager.Instance.datas.stage31 = true; break;
            case 32: DataManager.Instance.datas.stage32 = true; break;
            case 33: DataManager.Instance.datas.stage33 = true; break;
            case 34: DataManager.Instance.datas.stage34 = true; break;
            case 35: DataManager.Instance.datas.stage35 = true; break;
            case 36: DataManager.Instance.datas.stage36 = true; break;
            case 37: DataManager.Instance.datas.stage37 = true; break;
            case 38: DataManager.Instance.datas.stage38 = true; break;
            case 39: DataManager.Instance.datas.stage39 = true; break;
            case 40: DataManager.Instance.datas.stage40 = true; break;
            case 41: DataManager.Instance.datas.stage41 = true; break;
            case 42: DataManager.Instance.datas.stage42 = true; break;
            case 43: DataManager.Instance.datas.stage43 = true; break;
            case 44: DataManager.Instance.datas.stage44 = true; break;
            case 45: DataManager.Instance.datas.stage45 = true; break;
            case 46: DataManager.Instance.datas.stage46 = true; break;
            case 47: DataManager.Instance.datas.stage47 = true; break;
            case 48: DataManager.Instance.datas.stage48 = true; break;
            case 49: DataManager.Instance.datas.stage49 = true; break;
            case 50: DataManager.Instance.datas.stage50 = true; break;
        }

        DataManager.Instance.DataSave();
    }
}
