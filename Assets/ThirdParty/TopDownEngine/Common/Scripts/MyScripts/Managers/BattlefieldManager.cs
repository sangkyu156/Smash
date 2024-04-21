using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using static ES3AutoSaveMgr;

public class BattlefieldManager : MonoBehaviour, MMEventListener<MMGameEvent>
{
    public Transform nav;
    public Transform mission;
    public Transform deathPopup;
    public MMMultipleObjectPooler objectPooler;
    TextMeshProUGUI missionText1;
    TextMeshProUGUI missionText2;
    TextMeshProUGUI missionText3;
    TextMeshProUGUI missionText4;
    TextMeshProUGUI missionText5;
    GameObject mis; //미션 오브젝트 첫번째 자식(01,02,03 ...)
    GameObject mis_d; //죽었을때 생성되는 미션프리펩
    int enemy1 = 0;//죽인 에너미 숫자
    int enemy2 = 0;
    int enemy3 = 0;
    int enemy4 = 0;
    int enemy5 = 0;
    int satisfactionCount1 = 0;//클리어 조건 카운트숫자
    int satisfactionCount2 = 0;
    int satisfactionCount3 = 0;
    int satisfactionCount4 = 0;
    int satisfactionCount5 = 0;
    bool isClearZone = false; //클리어존이 생성된적이 있는지

    void Start()
    {
        CreateMission(GameManager.Instance.stage);
    }

    public void NavMeshBake()
    {
        nav.GetComponent<NavMeshSurface>().BuildNavMesh();

    }

    void CreateMission(Enum stage)
    {
        isClearZone = false;
        enemy1 = 0;
        enemy2 = 0;
        enemy3 = 0;
        enemy4 = 0;
        enemy5 = 0;

        SetRespawnManager();

        switch (stage)
        {
            case Define.Stage.Stage01: mis = CreateManager.Instantiate("Mission/01", mission); missionText1 = mis.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                                                                                               missionText2 = mis.transform.GetChild(1).GetComponent<TextMeshProUGUI>(); MissionSettings1(); break;
            case Define.Stage.Stage02: mis = CreateManager.Instantiate("Mission/02", mission); missionText1 = mis.transform.GetChild(0).GetComponent<TextMeshProUGUI>(); 
                                                                                               missionText2 = mis.transform.GetChild(1).GetComponent<TextMeshProUGUI>(); MissionSettings1(); break;
            case Define.Stage.Stage03: mis = CreateManager.Instantiate("Mission/03", mission); missionText1 = mis.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                                                                                               missionText2 = mis.transform.GetChild(1).GetComponent<TextMeshProUGUI>(); MissionSettings2(); break;
            case Define.Stage.Stage04: mis = CreateManager.Instantiate("Mission/04", mission); missionText1 = mis.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
                                                                                               missionText2 = mis.transform.GetChild(1).GetComponent<TextMeshProUGUI>(); MissionSettings3(); break;
            case Define.Stage.Stage05: mis = CreateManager.Instantiate("Mission/05", mission); missionText1 = mis.transform.GetChild(0).GetComponent<TextMeshProUGUI>(); MissionSettings1(); break;
            case Define.Stage.Stage06: break;
            case Define.Stage.Stage07: break;
            case Define.Stage.Stage08: break;
            case Define.Stage.Stage09: break;
            case Define.Stage.Stage10: break;
            case Define.Stage.Stage11: break;
            case Define.Stage.Stage12: break;
            case Define.Stage.Stage13: break;
            case Define.Stage.Stage14: break;
            case Define.Stage.Stage15: break;
            case Define.Stage.Stage16: break;
            case Define.Stage.Stage17: break;
            case Define.Stage.Stage18: break;
            case Define.Stage.Stage19: break;
            case Define.Stage.Stage20: break;
            case Define.Stage.Stage21: break;
            case Define.Stage.Stage22: break;
            case Define.Stage.Stage23: break;
            case Define.Stage.Stage24: break;
            case Define.Stage.Stage25: break;
            case Define.Stage.Stage26: break;
            case Define.Stage.Stage27: break;
            case Define.Stage.Stage28: break;
            case Define.Stage.Stage29: break;
            case Define.Stage.Stage30: break;
            case Define.Stage.Stage31: break;
            case Define.Stage.Stage32: break;
            case Define.Stage.Stage33: break;
            case Define.Stage.Stage34: break;
            case Define.Stage.Stage35: break;
            case Define.Stage.Stage36: break;
            case Define.Stage.Stage37: break;
            case Define.Stage.Stage38: break;
            case Define.Stage.Stage39: break;
            case Define.Stage.Stage40: break;
            case Define.Stage.Stage41: break;
            case Define.Stage.Stage42: break;
            case Define.Stage.Stage43: break;
            case Define.Stage.Stage44: break;
            case Define.Stage.Stage45: break;
            case Define.Stage.Stage46: break;
            case Define.Stage.Stage47: break;
            case Define.Stage.Stage48: break;
            case Define.Stage.Stage49: break;
            case Define.Stage.Stage50: break;
        }

        if (missionText1 != null)
            missionText1.text = enemy1.ToString() + " / " + satisfactionCount1.ToString();
        if (missionText2 != null)
            missionText2.text = enemy2.ToString() + " / " + satisfactionCount2.ToString();
        if (missionText3 != null)
            missionText3.text = enemy3.ToString() + " / " + satisfactionCount3.ToString();
        if (missionText4 != null)
            missionText4.text = enemy4.ToString() + " / " + satisfactionCount4.ToString();
        if (missionText5 != null)
            missionText5.text = enemy5.ToString() + " / " + satisfactionCount5.ToString();
    }

    void SetRespawnManager()
    {
        switch(GameManager.Instance.stage)
        {//0 - 슬라임, 1 - 버섯, 2 - 선인장, 3 - 거미, 4 - 해파리, 5 - 해골
            case Define.Stage.Stage01: objectPooler.Pool[0].Enabled = true; objectPooler.Pool[1].Enabled = true; break;//슬라임, 버섯
            case Define.Stage.Stage02: objectPooler.Pool[1].Enabled = true; objectPooler.Pool[2].Enabled = true; break;//버섯, 선인장
            case Define.Stage.Stage03: objectPooler.Pool[3].Enabled = true; objectPooler.Pool[5].Enabled = true; break;//거미, 해골
            case Define.Stage.Stage04: objectPooler.Pool[4].Enabled = true; objectPooler.Pool[5].Enabled = true; break;//해파리, 해골
        }
    }

    //미션 패배 팝업에 데이터 생성하는 함수
    void CreateMission_Defeat(Enum stage)
    {
        switch (stage)
        {
            case Define.Stage.Stage01: mis_d = CreateManager.Instantiate("Mission/01Defeat", deathPopup); mis_d.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = missionText1.text;
                                                                                                          mis_d.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = missionText2.text; break;
            case Define.Stage.Stage02: mis_d = CreateManager.Instantiate("Mission/02Defeat", deathPopup); mis_d.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = missionText1.text;
                                                                                                          mis_d.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = missionText2.text; break;
            case Define.Stage.Stage03: mis_d = CreateManager.Instantiate("Mission/03Defeat", deathPopup); mis_d.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = missionText1.text;
                                                                                                          mis_d.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = missionText2.text; break;
            case Define.Stage.Stage04: mis_d = CreateManager.Instantiate("Mission/04Defeat", deathPopup); mis_d.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = missionText1.text;
                                                                                                          mis_d.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = missionText2.text; break;
            case Define.Stage.Stage05: break;
            case Define.Stage.Stage06: break;
            case Define.Stage.Stage07: break;
            case Define.Stage.Stage08: break;
            case Define.Stage.Stage09: break;
            case Define.Stage.Stage10: break;
            case Define.Stage.Stage11: break;
            case Define.Stage.Stage12: break;
            case Define.Stage.Stage13: break;
            case Define.Stage.Stage14: break;
            case Define.Stage.Stage15: break;
            case Define.Stage.Stage16: break;
            case Define.Stage.Stage17: break;
            case Define.Stage.Stage18: break;
            case Define.Stage.Stage19: break;
            case Define.Stage.Stage20: break;
            case Define.Stage.Stage21: break;
            case Define.Stage.Stage22: break;
            case Define.Stage.Stage23: break;
            case Define.Stage.Stage24: break;
            case Define.Stage.Stage25: break;
            case Define.Stage.Stage26: break;
            case Define.Stage.Stage27: break;
            case Define.Stage.Stage28: break;
            case Define.Stage.Stage29: break;
            case Define.Stage.Stage30: break;
            case Define.Stage.Stage31: break;
            case Define.Stage.Stage32: break;
            case Define.Stage.Stage33: break;
            case Define.Stage.Stage34: break;
            case Define.Stage.Stage35: break;
            case Define.Stage.Stage36: break;
            case Define.Stage.Stage37: break;
            case Define.Stage.Stage38: break;
            case Define.Stage.Stage39: break;
            case Define.Stage.Stage40: break;
            case Define.Stage.Stage41: break;
            case Define.Stage.Stage42: break;
            case Define.Stage.Stage43: break;
            case Define.Stage.Stage44: break;
            case Define.Stage.Stage45: break;
            case Define.Stage.Stage46: break;
            case Define.Stage.Stage47: break;
            case Define.Stage.Stage48: break;
            case Define.Stage.Stage49: break;
            case Define.Stage.Stage50: break;
        }
    }

    void EnemyCount1()
    {
        enemy1++;
        if(enemy1 <= satisfactionCount1)
        {
            missionText1.text = enemy1.ToString() + " / " + satisfactionCount1.ToString();
            if(enemy1 == satisfactionCount1 )
            {
                missionText1.color = Color.red;
            }
        }
        Debug.Log($"enemy1 = {enemy1} / satisfactionCount1 = {satisfactionCount1}");
        if (ClearCheck())
        {
            CreateManager.Instance.SetRandomPosition(CreateManager.Instantiate("Battlefield/ClearZone"));
            Debug.Log("클리어 포탈 생성!");
        }
    }

    void EnemyCount2()
    {
        enemy2++;
        if (enemy2 <= satisfactionCount2)
        {
            missionText2.text = enemy2.ToString() + " / " + satisfactionCount2.ToString();
            if (enemy2 == satisfactionCount2)
            {
                missionText2.color = Color.red;
            }
        }
        Debug.Log($"enemy2 = {enemy2} / satisfactionCount2 = {satisfactionCount2}");
        if (ClearCheck())
        {
            CreateManager.Instance.SetRandomPosition(CreateManager.Instantiate("Battlefield/ClearZone"));
            Debug.Log("클리어 포탈 생성!");
        }
    }

    void MissionSettings1()
    {
        satisfactionCount1 = 15;
        satisfactionCount2 = 10;
        satisfactionCount3 = 0;
        satisfactionCount4 = 0;
        satisfactionCount5 = 0;
    }

    void MissionSettings2()
    {
        satisfactionCount1 = 7;
        satisfactionCount2 = 5;
        satisfactionCount3 = 0;
        satisfactionCount4 = 0;
        satisfactionCount5 = 0;
    }
    void MissionSettings3()
    {
        satisfactionCount1 = 5;
        satisfactionCount2 = 10;
        satisfactionCount3 = 0;
        satisfactionCount4 = 0;
        satisfactionCount5 = 0;
    }

    bool ClearCheck()
    {
        if(enemy1 >= satisfactionCount1 && 
            enemy2 >= satisfactionCount2 && 
            enemy3 >= satisfactionCount3 && 
            enemy4 >= satisfactionCount4 && 
            enemy5 >= satisfactionCount5 &&
            isClearZone == false)
        {
            isClearZone = true;
            return true; 
        }
        else
        { return false; }
    }

    public void OnMMEvent(MMGameEvent gameEvent)
    {
        if(gameEvent.EventName == "PlayerDie")
        {
            CreateMission_Defeat(GameManager.Instance.stage);
            //타임스케일은 팝업나올때 0으로 만들었음
        }
        if (gameEvent.EventName == "Installed")
        {
            NavMeshBake();
        }
        if(gameEvent.EventName == "Destroyed")
        {
            NavMeshBake();
        }
        if (gameEvent.EventName == "SlimeDie")
        {
            EnemyCount1();
        }
        if (gameEvent.EventName == "MushroomDie")
        {
            switch(GameManager.Instance.stage)
            {
                case Define.Stage.Stage01: EnemyCount2(); break;
                case Define.Stage.Stage02: EnemyCount1(); break;
            }
        }
        if (gameEvent.EventName == "CactusDie")
        {
            EnemyCount2();
        }
        if (gameEvent.EventName == "SpiderDie")
        {
            EnemyCount1();
        }
        if (gameEvent.EventName == "SkeletonDie")
        {
            EnemyCount2();
        }
        if (gameEvent.EventName == "BeholderDie")
        {
            EnemyCount1();
        }
    }

    void OnEnable()
    {
        this.MMEventStartListening<MMGameEvent>();
    }

    void OnDisable()
    {
        this.MMEventStopListening<MMGameEvent>();
    }

}