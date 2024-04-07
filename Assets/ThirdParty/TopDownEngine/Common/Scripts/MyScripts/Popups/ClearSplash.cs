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
    public GameObject[] particle;

    public int _rewars = 0;

    public void ClearTimePrint(float time)
    {
        // 분과 초를 계산합니다.
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        clearTime.text = "Clear Time = " + string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    public void RewardsPrint()
    {
        _rewars = GameManager.Instance.SetRewardGold();
        StageRewards(_rewars);//돈 넣어줌

        MMGameEvent.Trigger("StageClear");
    }

    public void StageRewards(int rewards)
    {
        DataManager.Instance.datas.CurPlayerGold += rewards;
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
