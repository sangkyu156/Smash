using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagePopupManager : MonoBehaviour, MMEventListener<MMInventoryEvent>
{
    public GameObject buyPopup;
    public GameObject sellPopup;
    public ItemStore itemStore;
    public PlayerDataManager playerData;
    public Inventory inventory;
    public InventoryDisplay inventoryDisplay;
    public InventoryDetails inventoryDetails;
    int curQuantity;
    string newItemName;
    InventoryItem curItem;
    InventorySlot curSolt;
    public InventoryItem[] InventoryItems = new InventoryItem[50];

    private void Start()
    {
        OffBuyPopup();
        OffSellPopup();
        InventoryItems = Resources.LoadAll<InventoryItem>($"Prefabs/Items");
    }

    public void OnBuyPopup()
    { buyPopup.SetActive(true); }

    public void OffBuyPopup()
    { buyPopup.SetActive(false); }

    public void OnSellPopup()
    { sellPopup.SetActive(true); }

    public void OffSellPopup()
    { sellPopup.SetActive(false); }

    //����� OK ��ư ������ ����Ǵ� �޼ҵ� ��������
    public void BuyItem()
    {
        curQuantity = itemStore.quantity;
        playerData.BuyItem(curItem, curQuantity);
        itemStore.SetPlayerGold();
        newItemName = curItem.ItemID.Replace("_npc", "");
        //�������� ������ ã�Ƽ� �κ��� �־��ֱ�
        foreach (InventoryItem item in InventoryItems)
        {
            if(item.ItemID == newItemName)
            {
                inventory.AddItem(item, curQuantity);
            }
        }
        inventory.SaveInventory();

        OffBuyPopup();
    }

    public void SellItem()
    {
        curQuantity = itemStore.quantity;
        playerData.SellItem(curItem, curQuantity);
        itemStore.SetPlayerGold();
        inventory.RemoveItem(curSolt.Index, curQuantity);
        inventory.SaveInventory();
        inventoryDisplay.RedrawInventoryDisplay();
        inventoryDisplay.Focus();
        inventoryDetails.DisplayDetails(null);

        OffSellPopup();
    }

    public void OnMMEvent(MMInventoryEvent inventoryEvent)
    {
        switch (inventoryEvent.InventoryEventType)
        {
            case MMInventoryEventType.Pick:
                break;
            case MMInventoryEventType.Select:
                break;
            case MMInventoryEventType.Click:
                curSolt = inventoryEvent.Slot;
                curItem = curSolt.CurrentlySelectedItem();
                break;
            case MMInventoryEventType.Move:
                break;
            case MMInventoryEventType.UseRequest:
                break;
            case MMInventoryEventType.ItemUsed:
                break;
            case MMInventoryEventType.EquipRequest:
                break;
            case MMInventoryEventType.ItemEquipped:
                break;
            case MMInventoryEventType.UnEquipRequest:
                break;
            case MMInventoryEventType.ItemUnEquipped:
                break;
            case MMInventoryEventType.Drop:
                break;
            case MMInventoryEventType.Destroy:
                break;
            case MMInventoryEventType.Error:
                break;
            case MMInventoryEventType.Redraw:
                break;
            case MMInventoryEventType.ContentChanged:
                break;
            case MMInventoryEventType.InventoryOpens:
                break;
            case MMInventoryEventType.InventoryCloseRequest:
                break;
            case MMInventoryEventType.InventoryCloses:
                break;
            case MMInventoryEventType.InventoryLoaded:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Ȱ��ȭ�ϸ� MMInventoryEvents ������ �����մϴ�.
    /// </summary>
    protected virtual void OnEnable()
    {
        this.MMEventStartListening<MMInventoryEvent>();
    }

    /// <summary>
    /// ��Ȱ��ȭ�Ǹ� MMInventoryEvents ������ �����˴ϴ�.
    /// </summary>
    protected virtual void OnDisable()
    {
        this.MMEventStopListening<MMInventoryEvent>();
    }
}
