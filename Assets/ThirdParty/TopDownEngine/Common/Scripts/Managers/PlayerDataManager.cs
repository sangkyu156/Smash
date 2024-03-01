using MoreMountains.InventoryEngine;
using MoreMountains.TopDownEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public enum ThirdAttack
    {
        Holy, Ice
    }

    public int CurPlayerGold {  get; private set; }
    public Inventory inventory;
    public ThirdAttack curThirdAttack = ThirdAttack.Holy;

    private void Start()
    {
        PlayerSetting();

        //파일 있는지 체크하고(해당 컴퓨터에서 처음 실행하는지 확인하고) 파일이 있으면 로드하고 없으면 초기값 넣어서 저장하고 불러온다
        if (ES3.FileExists())
        {
            CurPlayerGold = ES3.Load<int>("PlayerGold");
        }
        else
        {
            ES3.Save("PlayerGold", CurPlayerGold = 777);
            CurPlayerGold = ES3.Load<int>("PlayerGold");
        }

        //ES3.Save("PlayerGold", CurPlayerGold = 777);
        //CurPlayerGold = ES3.Load<int>("PlayerGold");
    }

    public void BuyItem(InventoryItem item, int quantity)
    {
        CurPlayerGold -= (item.price * quantity);

        ES3.Save<int>("PlayerGold", CurPlayerGold);
    }

    public void SellItem(InventoryItem item, int quantity)
    {
        CurPlayerGold += (int)Mathf.Round((item.price * quantity) * 0.8f);

        ES3.Save<int>("PlayerGold", CurPlayerGold);
    }

    public void StageRewards(string rewards)
    {
        CurPlayerGold += int.Parse(rewards);

        ES3.Save<int>("PlayerGold", CurPlayerGold);

        SaveInventory_Player();
    }

    public void SaveInventory_Player()
    {
        inventory.SaveInventory();
    }

    void PlayerSetting()
    {
        GameObject gameManager_ = GameObject.FindGameObjectWithTag("GameManager");
        if(gameManager_.GetComponent<GameManager>().playerThirdAttack == string.Empty)
            gameManager_.GetComponent<GameManager>().playerThirdAttack = curThirdAttack.ToString();
        gameManager_.GetComponent<GameManager>().playerThirdAttack = curThirdAttack.ToString();
    }
}
