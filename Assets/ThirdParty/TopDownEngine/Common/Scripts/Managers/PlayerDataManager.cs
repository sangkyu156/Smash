using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public static class PlayerDataManager
{
    public enum ThirdAttack
    {
        Holy, Ice
    }

    private static int CurPlayerGold;
    private static int Power;
    private static float Speed;
    private static float Heath;
    private static float Stamina;
    private static ThirdAttack curThirdAttack = ThirdAttack.Holy;

    public static void PlayerSetting()
    {
        if(ES3.FileExists())
        {
            //돈
            CurPlayerGold = ES3.Load<int>("PlayerGold");
            //3타 스킬
            curThirdAttack = ES3.Load<ThirdAttack>("PlayerThirdAttack");
            //공격력
            Power = ES3.Load<int>("PlayerPower");
            //스피드
            Speed = ES3.Load<float>("PlayerSpeed");
            //체력
            Heath = ES3.Load<float>("PlayerHeath");
            //스테미나
            Stamina = ES3.Load<float>("PlayerStamina");
        }
        else
        {
            //돈
            ES3.Save("PlayerGold", CurPlayerGold = 0);
            CurPlayerGold = ES3.Load<int>("PlayerGold");
            //3타 스킬
            ES3.Save("PlayerThirdAttack", curThirdAttack = ThirdAttack.Holy);
            curThirdAttack = ES3.Load<ThirdAttack>("PlayerThirdAttack");
            //공격력
            ES3.Save("PlayerPower", Power = 10);
            Power = ES3.Load<int>("PlayerPower");
            //스피드
            ES3.Save("PlayerSpeed", Speed = 10);
            Speed = ES3.Load<float>("PlayerSpeed");
            //체력
            ES3.Save("PlayerHeath", Heath = 100);
            Heath = ES3.Load<float>("PlayerHeath");
            //스테미나
            ES3.Save("PlayerStamina", Stamina = 30);
            Stamina = ES3.Load<float>("PlayerStamina");
        }

        UnityEngine.Debug.Log($"돈 = {CurPlayerGold}\n3타 스킬 = {curThirdAttack}\n공격력 = {Power}");
    }

    public static void TEST()
    {
        //돈
        //ES3.Save("PlayerGold", CurPlayerGold = 0);
        //CurPlayerGold = ES3.Load<int>("PlayerGold");
        //3타 스킬
        ES3.Save("PlayerThirdAttack", curThirdAttack = ThirdAttack.Holy);
        curThirdAttack = ES3.Load<ThirdAttack>("PlayerThirdAttack");
        //공격력
        ES3.Save("PlayerPower", Power = 10);
        Power = ES3.Load<int>("PlayerPower");
        //스피드
        ES3.Save("PlayerSpeed", Speed = 10);
        Speed = ES3.Load<float>("PlayerSpeed");
        //체력
        ES3.Save("PlayerHeath", Heath = 100);
        Heath = ES3.Load<float>("PlayerHeath");
        //스테미나
        ES3.Save("PlayerStamina", Stamina = 30);
        Stamina = ES3.Load<float>("PlayerStamina");
    }

    public static void BuyItem(InventoryItem item, int quantity)
    {
        CurPlayerGold -= (item.price * quantity);

        ES3.Save<int>("PlayerGold", CurPlayerGold);
    }

    public static void SellItem(InventoryItem item, int quantity)
    {
        CurPlayerGold += (int)Mathf.Round((item.price * quantity) * 0.8f);

        ES3.Save<int>("PlayerGold", CurPlayerGold);
    }

    public static void StageRewards(string rewards)
    {
        CurPlayerGold += int.Parse(rewards);

        ES3.Save<int>("PlayerGold", CurPlayerGold);

        MMGameEvent.Trigger("StageClear");
    }

    public static int GetCurPlayerGold()
    {
        return CurPlayerGold;
    }

    public static ThirdAttack GetThirdAttack()
    {
        return curThirdAttack;
    }

    public static int GetPower()
    {
        return Power;
    }

    public static float GetSpeed()
    {
        return Speed;
    }

    public static float GetHealth()
    {
        return Heath;
    }

    public static float GetStamina()
    {
        return Stamina;
    }
}
