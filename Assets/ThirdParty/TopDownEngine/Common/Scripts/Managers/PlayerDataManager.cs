using MoreMountains.InventoryEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public int CurPlayerGold {  get; private set; }

    private void Start()
    {
        //���� �ִ��� üũ�ϰ�(�ش� ��ǻ�Ϳ��� ó�� �����ϴ��� Ȯ���ϰ�) ������ ������ �ε��ϰ� ������ �ʱⰪ �־ �����ϰ� �ҷ��´�
        if (ES3.FileExists())
        {
            CurPlayerGold = ES3.Load<int>("PlayerGold");
        }
        else
        {
            ES3.Save("PlayerGold", CurPlayerGold = 777);
            CurPlayerGold = ES3.Load<int>("PlayerGold");
        }
    }

    public void BuyItem(InventoryItem item, int quantity)
    {
        CurPlayerGold = CurPlayerGold - (item.price * quantity);

        ES3.Save<int>("PlayerGold", CurPlayerGold);
    }
}
