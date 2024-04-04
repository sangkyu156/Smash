using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    public int CurPlayerGold = 77777;
    public int Power = 50;
    public float Speed = 10;
    public float Heath = 100;
    public float Stamina = 30;
    public ThirdAttack curThirdAttack = ThirdAttack.Holy;
    public int S_Health = 0; //스킬넘버 0
    public int S_ClearReward = 0; //스킬넘버 1
    public int S_Stamina = 0; //스킬넘버 2
}

public class DataManager : MMPersistentSingleton<DataManager>
{
    public Datas datas;

    private string KeyName = "Datas";
    private string FileName = "SaveFile.es3";

    void Start()
    {
        DataLoad();
        PlayerRefresh();
        Debug.Log($"S_Health = {datas.S_Health},S_ClearReward = {datas.S_ClearReward},S_Stamina = {datas.S_Stamina}");
    }

    public void DataSave()
    {
        ES3.Save(KeyName, datas);
        Debug.Log($"S_Health = {datas.S_Health},S_ClearReward = {datas.S_ClearReward},S_Stamina = {datas.S_Stamina}");
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
        int baseHealth = 100;
        datas.Heath = baseHealth + bounsHealth;
        int bounsStamina = datas.S_Stamina * 5;
        int baseStamina = 30;
        datas.Stamina = baseStamina + bounsStamina;
        LevelManager.Instance.Players[0].gameObject.GetComponent<Health>().UpdateMaxHealth();
        LevelManager.Instance.Players[0].gameObject.GetComponent<CharacterAbility>().StaminaChange();
    }
}
