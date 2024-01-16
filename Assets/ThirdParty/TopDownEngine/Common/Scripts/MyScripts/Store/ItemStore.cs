using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

public class ItemStore : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<MMGameEvent>
{
    public TextMeshProUGUI price_Text;
    public TextMeshProUGUI price_Text2;
    public TextMeshProUGUI quantity_Text;
    public TextMeshProUGUI quantity_Text2;
    public TextMeshProUGUI playerGold;
    public TextMeshProUGUI playerGold2;
    public PlayerDataManager playerData;
    InventoryItem curItem;


    public int quantity = 1;
    int curPlayerGold;

    private void Start()
    {
        if (playerData == null)
            playerData = GameObject.FindGameObjectWithTag("PlayerDataManager").GetComponent<PlayerDataManager>();
    }

    public void SetPlayerGold()
    {
        playerGold.text = playerData.CurPlayerGold.ToString();
        playerGold2.text = playerData.CurPlayerGold.ToString();
        quantity = 1;
        SetPriceAndQuantity();
    }

    public int GetPlayerGold()
    {
        SetPlayerGold();
        return curPlayerGold;
    }

    public void UpButton()
    {
        quantity++;

        if (curItem.price * quantity > curPlayerGold)
        {
            quantity--;
        }
        else
        {

        }

        SetPriceAndQuantity();
    }

    public void DownButton()
    {
        quantity--;

        if (quantity <= 0)
        {
            quantity++;
        }
        else
        {

        }

        SetPriceAndQuantity();
    }

    public void MaxButton()
    {
        quantity = curPlayerGold / curItem.price;
        SetPriceAndQuantity();
    }

    public void SetPriceAndQuantity()
    {
        quantity_Text.text = quantity.ToString();
        quantity_Text2.text = quantity.ToString();
        if (curItem != null)
        {
            price_Text.text = (curItem.price * quantity).ToString();
            price_Text2.text = (curItem.price * quantity).ToString();
        }
        else
        {
            price_Text.text = "0";
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
                    curPlayerGold = playerData.CurPlayerGold;
                    quantity = 1;
                    SetPriceAndQuantity();
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
