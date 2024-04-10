using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderControler : MonoBehaviour
{
    public List<BoxCollider> colliders = new List<BoxCollider>();

    void Start()
    {
        SetColliders();
    }

    void SetColliders()
    {
        for(int i = 0; i < colliders.Count; i++)
        {
            if(isFirstClear(i) == true)//스테이지를 클리어 한적 없으면 false반환, ture를 반환 했다는건 스테이지를 클리어한적이 있다는말이니까 콜라이더해제
                colliders[i].enabled = false;
            else
                colliders[i].enabled = true;
        }
    }

    //처음 클리어하는 스테이지면 false반환
    bool isFirstClear(int _curStage)
    {
        bool isFirst = false;

        switch (_curStage)
        {
            case 0: isFirst = DataManager.Instance.datas.stage1; break;
            case 1: isFirst = DataManager.Instance.datas.stage2; break;
            case 2: isFirst = DataManager.Instance.datas.stage3; break;
            case 3: isFirst = DataManager.Instance.datas.stage4; break;
            case 4: isFirst = DataManager.Instance.datas.stage5; break;
            case 5: isFirst = DataManager.Instance.datas.stage6; break;
            case 6: isFirst = DataManager.Instance.datas.stage7; break;
            case 7: isFirst = DataManager.Instance.datas.stage8; break;
            case 8: isFirst = DataManager.Instance.datas.stage9; break;
            case 9: isFirst = DataManager.Instance.datas.stage10; break;
            case 10: isFirst = DataManager.Instance.datas.stage11; break;
            case 11: isFirst = DataManager.Instance.datas.stage12; break;
            case 12: isFirst = DataManager.Instance.datas.stage13; break;
            case 13: isFirst = DataManager.Instance.datas.stage14; break;
            case 14: isFirst = DataManager.Instance.datas.stage15; break;
            case 15: isFirst = DataManager.Instance.datas.stage16; break;
            case 16: isFirst = DataManager.Instance.datas.stage17; break;
            case 17: isFirst = DataManager.Instance.datas.stage18; break;
            case 18: isFirst = DataManager.Instance.datas.stage19; break;
            case 19: isFirst = DataManager.Instance.datas.stage20; break;
            case 20: isFirst = DataManager.Instance.datas.stage21; break;
            case 21: isFirst = DataManager.Instance.datas.stage22; break;
            case 22: isFirst = DataManager.Instance.datas.stage23; break;
            case 23: isFirst = DataManager.Instance.datas.stage24; break;
            case 24: isFirst = DataManager.Instance.datas.stage25; break;
            case 25: isFirst = DataManager.Instance.datas.stage26; break;
            case 26: isFirst = DataManager.Instance.datas.stage27; break;
            case 27: isFirst = DataManager.Instance.datas.stage28; break;
            case 28: isFirst = DataManager.Instance.datas.stage29; break;
            case 29: isFirst = DataManager.Instance.datas.stage30; break;
            case 30: isFirst = DataManager.Instance.datas.stage31; break;
            case 31: isFirst = DataManager.Instance.datas.stage32; break;
            case 32: isFirst = DataManager.Instance.datas.stage33; break;
            case 33: isFirst = DataManager.Instance.datas.stage34; break;
            case 34: isFirst = DataManager.Instance.datas.stage35; break;
            case 35: isFirst = DataManager.Instance.datas.stage36; break;
            case 36: isFirst = DataManager.Instance.datas.stage37; break;
            case 37: isFirst = DataManager.Instance.datas.stage38; break;
            case 38: isFirst = DataManager.Instance.datas.stage39; break;
            case 39: isFirst = DataManager.Instance.datas.stage40; break;
            case 40: isFirst = DataManager.Instance.datas.stage41; break;
            case 41: isFirst = DataManager.Instance.datas.stage42; break;
            case 42: isFirst = DataManager.Instance.datas.stage43; break;
            case 43: isFirst = DataManager.Instance.datas.stage44; break;
            case 44: isFirst = DataManager.Instance.datas.stage45; break;
            case 45: isFirst = DataManager.Instance.datas.stage46; break;
            case 46: isFirst = DataManager.Instance.datas.stage47; break;
            case 47: isFirst = DataManager.Instance.datas.stage48; break;
            case 48: isFirst = DataManager.Instance.datas.stage49; break;
            case 49: isFirst = DataManager.Instance.datas.stage50; break;
        }

        return isFirst;
    }
}
