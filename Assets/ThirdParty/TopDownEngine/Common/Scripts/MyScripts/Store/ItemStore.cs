using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ItemStore : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
{
    public TextMeshProUGUI price_Text;
    public TextMeshProUGUI price_Text2;
    public TextMeshProUGUI quantity_Text;
    public TextMeshProUGUI quantity_Text2;
    public TextMeshProUGUI playerGold;
    public TextMeshProUGUI playerGold2;
    InventoryItem curItem;


    public int quantity = 1;
    int curPlayerGold;

    public void SetPlayerGold()
    {
        if(GameManager.Instance.stage == Define.Stage.Stage00)
        {
            playerGold.text = DataManager.Instance.datas.CurPlayerGold.ToString();
            playerGold2.text = DataManager.Instance.datas.CurPlayerGold.ToString();
            quantity = 1;
            SetPriceAndQuantity_Buy();
            SetPriceAndQuantity_Sell();
        }
        else
            playerGold2.text = DataManager.Instance.datas.CurPlayerGold.ToString();
    }

    public int GetPlayerGold()
    {
        SetPlayerGold();
        return curPlayerGold;
    }

    public void BuyUpButton()
    {
        quantity++;

        if (curItem.price * quantity > curPlayerGold)
        {
            quantity--;
        }
        else
        {

        }

        SetPriceAndQuantity_Buy();
    }

    public void BuyDownButton()
    {
        quantity--;

        if (quantity <= 0)
        {
            quantity++;
        }
        else
        {

        }

        SetPriceAndQuantity_Buy();
    }

    public void BuyMaxButton()
    {
        quantity = curPlayerGold / curItem.price;
        SetPriceAndQuantity_Buy();
    }

    public void SellUpButton()
    {
        quantity++;

        if (quantity > curItem.Quantity)
        {
            quantity--;
        }
        else
        {

        }

        SetPriceAndQuantity_Sell();
    }

    public void SellDownButton()
    {
        quantity--;

        if (quantity <= 0)
        {
            quantity++;
        }
        else
        {

        }

        SetPriceAndQuantity_Sell();
    }

    public void SellMaxButton()
    {
        quantity = curItem.Quantity;
        SetPriceAndQuantity_Sell();
    }

    public void SetPriceAndQuantity_Buy()
    {
        quantity_Text.text = quantity.ToString();
        if (curItem != null)
        {
            price_Text.text = (curItem.price * quantity).ToString();
        }
        else
        {
            price_Text.text = "0";
        }
    }

    public void SetPriceAndQuantity_Sell()
    {
        quantity_Text2.text = quantity.ToString();
        if (curItem != null)
        {
            price_Text2.text = Math.Round((curItem.price * quantity) * 0.8f).ToString();
        }
        else
        {
            price_Text2.text = "0";
        }
    }

    //상점, 이벤토리가 열릴떄마다 호출되는 이벤트
    //플레이어 현재 골드량 표시, 수량 1로 설정
    public void OnMMEvent(MMInventoryEvent inventoryEvent)
    {
        SetPlayerGold();
        switch (inventoryEvent.InventoryEventType)
        {
            case MMInventoryEventType.Select:
                if (inventoryEvent.EventItem != null)
                {
                    curItem = inventoryEvent.EventItem;
                    curPlayerGold = DataManager.Instance.datas.CurPlayerGold;
                    quantity = 1;
                    if (GameManager.Instance.stage == Define.Stage.Stage00)
                    {
                        SetPriceAndQuantity_Buy();
                        SetPriceAndQuantity_Sell();
                    }
                }
                break;
        }
    }

    //게임 이벤트가 포착되면 호출되는 이벤트
    public void OnMMEvent(MMGameEvent eventType)
    {
        //switch (eventType.EventName)
        //{
        //    case "inventoryOpens":
        //        SetPlayerGold();
        //        break;
        //}
    }

    /// <summary>
    /// 활성화하면 MMInventoryEvents,MMGameEvent 수신을 시작합니다.
    /// </summary>
    protected virtual void OnEnable()
    {
        this.MMEventStartListening<MMInventoryEvent>();
        this.MMEventStartListening<MMGameEvent>();
    }

    /// <summary>
    /// 비활성화되면 MMInventoryEvents,MMGameEvent 수신이 중지됩니다.
    /// </summary>
    protected virtual void OnDisable()
    {
        this.MMEventStopListening<MMInventoryEvent>();
        this.MMEventStartListening<MMGameEvent>();
    }



}
