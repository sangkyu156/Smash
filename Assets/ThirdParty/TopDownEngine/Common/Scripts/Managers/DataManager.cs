using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum ThirdAttack
{
    Holy, Ice
}

public enum Skills
{
    S_Health, S_Stamina, S_ClearReward
}

[System.Serializable]
public class Datas
{
    public int CurPlayerGold = 100;
    public int Power = 15;
    public float Speed = 8;
    public float Heath = 100;
    public float Stamina = 15;
    public ThirdAttack curThirdAttack = ThirdAttack.Holy;
    public bool firstVillage = false;
    public bool firstLevelSelect = false;
    public bool firstBattlefield = false;
    public int S_Health = 0; //스킬넘버 0
    public int S_ClearReward = 0; //스킬넘버 1
    public int S_Stamina = 0; //스킬넘버 2
    public bool stage0 = false;
    public bool stage1 = false;
    public bool stage2 = false;
    public bool stage3 = false;
    public bool stage4 = false;
    public bool stage5 = false;
    public bool stage6 = false;
    public bool stage7 = false;
    public bool stage8 = false;
    public bool stage9 = false;
    public bool stage10 = false;
    public bool stage11 = false;
    public bool stage12 = false;
    public bool stage13 = false;
    public bool stage14 = false;
    public bool stage15 = false;
    public bool stage16 = false;
    public bool stage17 = false;
    public bool stage18 = false;
    public bool stage19 = false;
    public bool stage20 = false;
    public bool stage21 = false;
    public bool stage22 = false;
    public bool stage23 = false;
    public bool stage24 = false;
    public bool stage25 = false;
    public bool stage26 = false;
    public bool stage27 = false;
    public bool stage28 = false;
    public bool stage29 = false;
    public bool stage30 = false;
    public bool stage31 = false;
    public bool stage32 = false;
    public bool stage33 = false;
    public bool stage34 = false;
    public bool stage35 = false;
    public bool stage36 = false;
    public bool stage37 = false;
    public bool stage38 = false;
    public bool stage39 = false;
    public bool stage40 = false;
    public bool stage41 = false;
    public bool stage42 = false;
    public bool stage43 = false;
    public bool stage44 = false;
    public bool stage45 = false;
    public bool stage46 = false;
    public bool stage47 = false;
    public bool stage48 = false;
    public bool stage49 = false;
    public bool stage50 = false;
}

public class DataManager : MMPersistentSingleton<DataManager>
{
    public Datas datas;
    GameObject inventory;

    private string KeyName = "Datas";
    private string FileName = "SaveFile.es3";

    private void OnEnable()
    {
        if(SceneManager.GetActiveScene().name != "TitleScene")
            DataLoad();
    }

    void Start()
    {
        if (SceneManager.GetActiveScene().name != "TitleScene")
            DataLoad();
    }

    public void DataSave()
    {
        ES3.Save(KeyName, datas);
    }

    public void DataLoad()
    {
        if(ES3.FileExists(FileName))//저장한 파일이 있는지 확인해서 있다면
        {
            ES3.LoadInto(KeyName, datas);
        }
        else//저장한 파일이 있는지 확인해서 없다면
        {
            DataSave();
        }
    }

    public void PlayerRefresh()
    {
        int bounsHealth = datas.S_Health * 10;
        float baseHealth = 100;
        datas.Heath = baseHealth + bounsHealth;
        int bounsStamina = datas.S_Stamina * 5;
        float baseStamina = 15;
        datas.Stamina = baseStamina + bounsStamina;
        LevelManager.Instance.Players[0].gameObject.GetComponent<Health>().UpdateMaxHealth();
        LevelManager.Instance.Players[0].gameObject.GetComponent<CharacterRun>().StaminaChange();
        Debug.Log("PlayerRefresh함");
    }

    public void FindSetStore()
    {
        Debug.Log("asdzxc2");
        inventory = GameObject.FindGameObjectWithTag("NPCInventory");
        Debug.Log($"inventory = {inventory.gameObject.name}");
        Debug.Log($"datas.stage1 = {datas.stage1}");
        Debug.Log($"datas.stage2 = {datas.stage2}");
        inventory.GetComponent<Inventory>().isStage1 = datas.stage1;
        inventory.GetComponent<Inventory>().isStage2 = datas.stage2;
        inventory.GetComponent<Inventory>().isStage3 = datas.stage3;
        inventory.GetComponent<Inventory>().isStage4 = datas.stage4;
        inventory.GetComponent<Inventory>().isStage5 = datas.stage5;
        inventory.GetComponent<Inventory>().isStage6 = datas.stage6;
        inventory.GetComponent<Inventory>().isStage7 = datas.stage7;
        inventory.GetComponent<Inventory>().isStage8 = datas.stage8;
        inventory.GetComponent<Inventory>().isStage9 = datas.stage9;
        inventory.GetComponent<Inventory>().isStage10 = datas.stage10;
    }
}
