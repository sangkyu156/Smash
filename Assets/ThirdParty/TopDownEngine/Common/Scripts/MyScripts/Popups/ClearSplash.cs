using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static ICSharpCode.SharpZipLib.Zip.ZipEntryFactory;

public class ClearSplash : MonoBehaviour
{
    public TextMeshProUGUI clearTime;
    public TextMeshProUGUI rewards;
    public GameObject[] particle;

    public void ClearTimePrint(float time)
    {
        // 분과 초를 계산합니다.
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        clearTime.text = "Clear Time = " + string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    public void RewardsPrint()
    {
        switch (GameManager.Instance.stage)
        {
            case Define.Stage.Stage01: string _rewars = "70"; rewards.text = _rewars; StageRewards(_rewars); break;
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

        MMGameEvent.Trigger("StageClear");
    }

    public void StageRewards(string rewards)
    {
        DataManager.Instance.datas.CurPlayerGold += int.Parse(rewards);
        DataManager.Instance.DataSave();
    }

    public void GoToVillage_Clear()
    {
        GameManager.Instance.stage = Define.Stage.Stage00;
        LevelManager.Instance.isClear = true;
        DOTweenOff();
        LevelManager.Instance.GotoLevel("Village");
    }


    public void GoToRestart_Clear()
    {
        LevelManager.Instance.isClear = true;
        DOTweenOff();
        LevelManager.Instance.GotoLevel(SceneManager.GetActiveScene().name);
    }

    void DOTweenOff()
    {
        for (int i = 0; i < particle.Length; i++)
        {
            particle[i].GetComponent<ScaleAndSparkle>().ScaleUPandDownOff();
        }
    }
}
