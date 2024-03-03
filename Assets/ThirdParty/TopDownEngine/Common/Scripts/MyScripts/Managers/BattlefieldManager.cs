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
    TextMeshProUGUI missionText1;
    TextMeshProUGUI missionText2;
    TextMeshProUGUI missionText3;
    TextMeshProUGUI missionText4;
    TextMeshProUGUI missionText5;
    GameObject mis; //미션 오브젝트 첫번째 자식(01,02,03 ...)
    GameObject mis_d; //죽었을때 생성되는 미션프리펩
    int enemy1 = 0;
    int enemy2 = 0;
    int enemy3 = 0;
    int enemy4 = 0;
    int enemy5 = 0;
    int satisfactionCount1 = 0;
    int satisfactionCount2 = 0;
    int satisfactionCount3 = 0;
    int satisfactionCount4 = 0;
    int satisfactionCount5 = 0;

    void Start()
    {
        CreateMission(GameManager.Instance.stage);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            NavMeshBake();
            Debug.Log("j누름");
        }
    }

    public void NavMeshBake()
    {
        nav.GetComponent<NavMeshSurface>().BuildNavMesh();

    }

    void CreateMission(Enum stage)
    {
        enemy1 = 0;
        enemy2 = 0;
        enemy3 = 0;
        enemy4 = 0;
        enemy5 = 0;

        switch (stage)
        {
            case Define.Stage.Stage01: mis = CreateManager.Instantiate("Mission/01", mission); missionText1 = mis.transform.GetChild(0).GetComponent<TextMeshProUGUI>(); MissionSettings1(); break;
            case Define.Stage.Stage02: mis = CreateManager.Instantiate("Mission/02", mission); missionText1 = mis.transform.GetChild(0).GetComponent<TextMeshProUGUI>(); 
                                                                                               missionText2 = mis.transform.GetChild(1).GetComponent<TextMeshProUGUI>(); break;
            case Define.Stage.Stage03: break;
            case Define.Stage.Stage04: break;
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

    void CreateMission_Defeat(Enum stage)
    {
        switch (stage)
        {
            case Define.Stage.Stage01: mis_d = CreateManager.Instantiate("Mission/01Defeat", deathPopup); mis_d.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = missionText1.text; break;
            case Define.Stage.Stage02: break;
            case Define.Stage.Stage03: break;
            case Define.Stage.Stage04: break;
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

        if (ClearCheck())
        {
            CreateManager.Instance.SetRandomPosition(CreateManager.Instantiate("Battlefield/ClearZone"));
            Debug.Log("클리어 포탈 생성!");
        }
    }

    void MissionSettings1()
    {
        satisfactionCount1 = 3;
        satisfactionCount2 = 0;
        satisfactionCount3 = 0;
        satisfactionCount4 = 0;
        satisfactionCount5 = 0;
    }

    bool ClearCheck()
    {
        if(enemy1 == satisfactionCount1 && 
            enemy2 == satisfactionCount2 && 
            enemy3 == satisfactionCount3 && 
            enemy4 == satisfactionCount4 && 
            enemy5 == satisfactionCount5)
        { return true; }
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
        if (gameEvent.EventName == "SlimeDie")
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