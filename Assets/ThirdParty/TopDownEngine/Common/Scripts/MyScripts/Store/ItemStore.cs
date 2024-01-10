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
    public GameObject playerDataManager;
    public InventoryItem curItem;


    int quantity = 1;
    int CurPlayerGold;

    private void Start()
    {
        if (playerDataManager == null)
            playerDataManager = GameObject.FindGameObjectWithTag("PlayerDataManager");
    }

    public void SetPlayerGold()
    {
        playerGold.text = playerDataManager.GetComponent<PlayerDataManager>().CurPlayerGold.ToString();
        playerGold2.text = playerDataManager.GetComponent<PlayerDataManager>().CurPlayerGold.ToString();
        quantity = 1;
        SetPriceAndQuantity();
    }

    public int GetPlayerGold()
    {
        SetPlayerGold();
        return CurPlayerGold;
    }

    public void UpButton()
    {
        quantity++;

        if (curItem.price * quantity > CurPlayerGold)
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
        quantity = CurPlayerGold / curItem.price;
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

    public void OnMMEvent(MMInventoryEvent inventoryEvent)
    {
        switch (inventoryEvent.InventoryEventType)
        {
            case MMInventoryEventType.Select:
                if (inventoryEvent.EventItem != null)
                {
                    curItem = inventoryEvent.EventItem;
                    CurPlayerGold = playerDataManager.GetComponent<PlayerDataManager>().CurPlayerGold;
                    quantity = 1;
                    SetPriceAndQuantity();
                }
                break;
        }
    }

    public void OnMMEvent(MMGameEvent eventType)
    {
        switch (eventType.EventName)
        {
            case "inventoryOpens":
                SetPlayerGold();
                break;
        }
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
