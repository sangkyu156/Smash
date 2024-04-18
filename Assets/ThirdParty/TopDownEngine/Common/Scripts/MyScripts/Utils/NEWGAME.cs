using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NEWGAME : MonoBehaviour
{
    public GameObject newstartPopup;

    private string KeyName = "Datas";
    private string FileName = "SaveFile.es3";


    public void NewStartButton()
    {
        if (ES3.FileExists(FileName))//저장한 파일이 있는지 확인해서 있다면
        {
            newstartPopup.SetActive(true);
        }
        else
        {
            LevelManager.Instance.GotoLevel("StartCutScene");
        }
    }

    public void NewStartYES()
    {
        MMSaveLoadManager.DeleteSave("RogueMainInventory_Player1.inventory");
        ES3.DeleteFile();

        LevelManager.Instance.GotoLevel("StartCutScene");
    }

    public void NewStartNO()
    {
        newstartPopup.SetActive(false);
    }
}
